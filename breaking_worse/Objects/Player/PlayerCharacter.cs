using System;
using System.Linq;
using breaking_worse.Input.Enums;
using breaking_worse.Objects.Animations;
using breaking_worse.Objects.Collisions;
using breaking_worse.Screens.ScreenTypes.InGameScreens;
using breaking_worse.Objects.Combat;
using breaking_worse.Objects.Interaction;
using breaking_worse.Screens.Clickables;
using breaking_worse.Screens.ScreenTypes.MenuScreens;
using breaking_worse.State.Serializable;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using breaking_worse.Sound;
using breaking_worse.State;
using breaking_worse.State.Enums;

namespace breaking_worse.Objects.Player;

public class PlayerCharacter : AGameObject
{
    /// <summary>
    /// Represents a Player Character Object that can move in a direction
    /// and draw itself on the given spritebatch
    /// </summary>
    // walt or jesse
    public PlayerCharacterId PlayerCharacterId { get; }
    
    // texture
    private const float ImageScalingFactor = 1f;
    
    // HitBox dimensions
    private const int DynamicHitBoxWidth = 20;
    private const int DynamicHitBoxHeight = 20;
    
    // animation
    public Vector2 DirectionFov { get; private set; }
    private Vector2 _directionAnimation;
    private Vector2 _movingAverageDirection;
    
    // combat
    private const int Radius = 8;
    private const int Fov = 60;
    
    // suspicious actions
    private const int AlertRadius = 8;
    
    // status
    public bool IsHidden { get; set; }
    public bool IsInCar { get; set; }
    public bool IsInteracting { get; set; }
    public bool IsInventoryOpen { get; private set; }
    public Weapon EquippedWeapon { get; private set; } = Weapon.Fist;
    private DifficultySettings settings;
    
    // inventory
    private readonly InventoryScreen _inventoryScreen;
    private BackgroundScreen _wastedScreen;
    private float _timeSinceIsCaught = -1;
    private TextBox _textboxWasted;
    
    public PlayerCharacter(GameManager gameManager,  PlayerCharacterId playerCharacterId) : base(gameManager)
    {
        PlayerCharacterId = playerCharacterId;
        Type = playerCharacterId == PlayerCharacterId.Walt ? NpcType.Walt : NpcType.Jesse;
        settings = GameManager.SettingsManager.getDifficultySettings();
        if (playerCharacterId == PlayerCharacterId.Jesse)
            Position = new Vector2(4680, 2500);
        if (playerCharacterId == PlayerCharacterId.Walt)
            Position = new Vector2(4710, 2500);

        _inventoryScreen = new InventoryScreen(GameManager, GameManager.ScreenManager.InGameScreen.PlayerState.Inventory, [playerCharacterId]);
        _wastedScreen = new BackgroundScreen(GameManager, GameManager.AssetManager.Images["wasted"], 0.8f);
        _wastedScreen.UpdateLower = true;
        _wastedScreen.DrawLower = true;
        _textboxWasted = new TextBox(GameManager, new Vector2(GameManager.SettingsManager.Resolution.Width / 2f, GameManager.SettingsManager.Resolution.Height / 2f), "You are arrested!!!!", GameManager.AssetManager.Fonts["numbers"], 1.5f, Color.White);
        
        addComponent(new Animatable());
        addComponent(new ActiveCombatant(gameManager, Radius, Fov, () => EquippedWeapon,getComponent<Animatable>(), (int) (20 * settings.PlayerHealthMultiplier), true));
        addComponent(new Collider(gameManager, () => Position,
            -new Vector2(DynamicHitBoxWidth / 2f, DynamicHitBoxHeight / 2f), 
            getComponent<ActiveCombatant>()));
        
        var dynamicHitBox = getComponent<Collider>().DynamicHitBox;
        dynamicHitBox.Width = DynamicHitBoxWidth;
        dynamicHitBox.Height = DynamicHitBoxHeight;
        getComponent<Collider>().IsPlayer = true;
    }

    public override void loadContent()
    {
        loadAnimations();
    }

    public override void update(GameTime gameTime)
    {
        checkForButtonPresses();
        getComponent<Animatable>().CurrentAnimation.update(gameTime);
        
        if(_timeSinceIsCaught > 0)
        {
            _timeSinceIsCaught--;
        }
        if (_timeSinceIsCaught == 0 && GameManager.ScreenManager.InGameScreen.PlayerState.Health > 0)
        {
            GameManager.ScreenManager.InGameScreen.PlayerState.IsCaught = false;
            GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position = new Vector2(790, 680);
            GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.Position = new Vector2(820, 680);
            GameManager.ScreenManager.InGameScreen.PlayerState.Inventory.Money -= (int) (2000 * settings.PenaltyMultiplier);
            GameManager.ScreenManager.InGameScreen.PlayerState.Timer -= (int) (70 * settings.PenaltyMultiplier);
            GameManager.ScreenManager.InGameScreen.PlayerState.SuspiciousActivityCounter = 0;
            GameManager.ScreenManager.InGameScreen.PlayerState.Arrests++;
            GameManager.ScreenManager.InGameScreen.PlayerState.CaughtTime = null;
            GameManager.ScreenManager.InGameScreen.PlayerState.IsCaught = false;
            _timeSinceIsCaught = -1;
            
            GameManager.ProgressManager.updateStatistics(Statistics.TimesJail, 1, 1);
            GameManager.ProgressManager.updateAchievements(Statistics.TimesJail, Achievement.FirstJail);
            GameManager.ScreenManager.InGameScreen.PlayerState.wentToJail();
        
            if(PlayerCharacterId == PlayerCharacterId.Walt) GameManager.SoundManager.playSoundEffect(Sfx.Prison);
        }
        if ((!IsInteracting || (IsInteracting && IsInCar)) &&
            !IsHidden &&
            !IsInventoryOpen &&
            GameManager.ScreenManager.InGameScreen.PlayerState.Health > 0 &&
            !GameManager.ScreenManager.InGameScreen.PlayerState.IsCaught)
        {
            move(gameTime); 
            GameManager.ScreenManager.InGameScreen.PlayerState.increaseSteps(Math.Abs(
                GameManager.UserActionHandler.getMovement(PlayerCharacterId).Length() * 
                gameTime.ElapsedGameTime.Milliseconds / 300));
        }

        if (GameManager.ScreenManager.InGameScreen.PlayerState.Health <= 0 && !GameManager.ScreenManager.InGameScreen.PlayerState.IsCaught)
            handleDeath();

        if (checkForCaught())
            handleCaught();
    }

    private void move(GameTime gameTime)
    {
        var direction = GameManager.UserActionHandler.getMovement(PlayerCharacterId)
                        * (float)gameTime.ElapsedGameTime.TotalMilliseconds
                        * GameManager.ScreenManager.InGameScreen.PlayerState.MovementSpeed * (IsInCar ? 2 : 1);
        
        // direction scaled so that player won't collide
        var scaledDirection = Vector2.Zero;
        _movingAverageDirection += (direction - _movingAverageDirection) / 10f;
        if (direction.Length() > 0)
            scaledDirection = getComponent<Collider>().scaleDirectionByCollisions(direction, gameTime);

        // control facing of fov
        if (scaledDirection != Vector2.Zero) 
            DirectionFov = scaledDirection;
        // control facing of character sprite
        if (scaledDirection.X != 0)
            _directionAnimation = scaledDirection;

        var animatable = getComponent<Animatable>();
        if (scaledDirection != Vector2.Zero)
        {
            if (animatable.CurrentAnimationName != "running")
                animatable.startAnimation("running");
            
            GameManager.SoundManager.playSoundEffect(Sfx.Step);
        }
        else
        {
            if (animatable.CurrentAnimationName != "idle")
                animatable.startAnimation("idle");
        }
        
        Position += scaledDirection;
    }
    
    private void switchWeapon()
    {
        EquippedWeapon = EquippedWeapon == Weapon.Fist ? Weapon.Gun : Weapon.Fist;
    }

    private void handleDeath()
    {
        if (!GameManager.ScreenManager.containsScreen(_wastedScreen) && PlayerCharacterId == PlayerCharacterId.Walt)
        {
            GameManager.SoundManager.pauseMusic();
            GameManager.SoundManager.playSoundEffect(Sfx.Wasted);
            GameManager.ScreenManager.addScreen(_wastedScreen);
        }
        
        if (GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.getComponent<ActiveCombatant>().shouldDespawn()
            || GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.getComponent<ActiveCombatant>().shouldDespawn())
        { 
            if(PlayerCharacterId == PlayerCharacterId.Walt)
            {
                GameManager.SoundManager.resumeMusic();
                GameManager.ScreenManager.removeFromStack(_wastedScreen, 1);
            }
            
            GameManager.ScreenManager.InGameScreen.PlayerState.Inventory.Money -= (int) (5000 * settings.PenaltyMultiplier); 
            GameManager.ScreenManager.InGameScreen.PlayerState.Timer -= (int) (20 * settings.PenaltyMultiplier);
            GameManager.ScreenManager.InGameScreen.PlayerState.Deaths++;
            GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position = new Vector2(3390, 2500);
            GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.Position = new Vector2(3440, 2500);
            GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.getComponent<ActiveCombatant>().Health = (int) (3 * settings.PlayerHealthMultiplier);
            GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.getComponent<ActiveCombatant>().Health = (int) (3 * settings.PlayerHealthMultiplier);
            GameManager.ScreenManager.InGameScreen.PlayerState.balanceHealth();
            GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.getComponent<Animatable>().startAnimation("idle", true);
            GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.getComponent<Animatable>().startAnimation("idle", true);
            GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.getComponent<ActiveCombatant>().DeathTime = null;
            GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.getComponent<ActiveCombatant>().DeathTime = null;
            
            GameManager.ProgressManager.updateStatistics(Statistics.HospitalVisits, 1, 1);
        }
    }

    private void handleCaught()
    {
        if (GameManager.ScreenManager.InGameScreen.PlayerState.Health <= 0)
        {
            GameManager.ScreenManager.InGameScreen.PlayerState.IsCaught = false;
            return;
        }
        
        if (GameManager.ScreenManager.InGameScreen.PlayerState.shouldTeleport())
        {
            GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position = new Vector2(790, 680);
            GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.Position = new Vector2(820, 680);
            GameManager.ScreenManager.InGameScreen.PlayerState.Inventory.Money -= (int) (2000 * settings.PenaltyMultiplier);
            GameManager.ScreenManager.InGameScreen.PlayerState.Timer -= (int) (70 * settings.PenaltyMultiplier);
            GameManager.ScreenManager.InGameScreen.PlayerState.SuspiciousActivityCounter = 0;
            GameManager.ScreenManager.InGameScreen.PlayerState.Arrests++;
            GameManager.ScreenManager.InGameScreen.PlayerState.CaughtTime = null;
            GameManager.ScreenManager.InGameScreen.PlayerState.IsCaught = false;
            _timeSinceIsCaught = 0;
            
            GameManager.ProgressManager.updateStatistics(Statistics.TimesJail, 1, 1);
            GameManager.ProgressManager.updateAchievements(Statistics.TimesJail, Achievement.FirstJail);
            GameManager.ScreenManager.InGameScreen.PlayerState.wentToJail();
        
            if(PlayerCharacterId == PlayerCharacterId.Walt) GameManager.SoundManager.playSoundEffect(Sfx.Prison);
        }
    }

    public void checkForPoliceInRadius()
    {
        var neighbors = GameManager.ScreenManager.InGameScreen.CollisionManager.getNeighborsInRadius(getComponent<Collider>(), AlertRadius);
        if (neighbors.Any(neighbor => neighbor.IsPolice && neighbor.InactiveCombatant.Health > 0))
        {
            GameManager.ScreenManager.InGameScreen.PlayerState.SuspiciousActivityCounter += 5;
            GameManager.ScreenManager.InGameScreen.PlayerState.LastTimeSeen = DateTime.Now;
        }
    }

    private void findClosestNpc(PlayerCharacterId playerId)
    {
        var neighbors = GameManager.ScreenManager.InGameScreen.GameObjectManager.getObjectsThatHaveComponent<Dialogable>();
        
        AGameObject closestNpc = null;
        AGameObject previousNpc = null;
        float minDistance = float.MaxValue;
        
        foreach (var neighbor in neighbors)
        {
            
            if (neighbor is { } npc && !IsInteracting)
            {
                float distance = Vector2.Distance(Position, npc.Position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    if (closestNpc != null)
                    {
                        if (playerId == PlayerCharacterId.Walt)
                            previousNpc.CanBeTalkedToWalt = false;
                        else
                            previousNpc.CanBeTalkedToJesse = false;
                    }
                    closestNpc = npc;
                    previousNpc = closestNpc;

                    if (playerId == PlayerCharacterId.Walt)
                        closestNpc.CanBeTalkedToWalt = true;
                    else
                        closestNpc.CanBeTalkedToJesse = true;
                }
                else
                {
                    if (playerId == PlayerCharacterId.Walt)
                        npc.CanBeTalkedToWalt = false;
                    else
                        npc.CanBeTalkedToJesse = false;
                }
            }
        }
    
    }

    private void checkForButtonPresses()
    {
        
        findClosestNpc(PlayerCharacterId);
        
        // open pause menu
        if (GameManager.UserActionHandler.isUserAction(PlayerCharacterId, UserAction.PauseMenu, PressType.PressAndRelease) && !GameManager.ScreenManager.InGameScreen.PlayerState.IsCaught && !(GameManager.ScreenManager.InGameScreen.PlayerState.Health <= 0))
        {
            var background = new BackgroundScreen(GameManager, GameManager.AssetManager.Images["PauseScreenBackground"], 0.5f);
            background.DrawLower = true;
            GameManager.ScreenManager.addScreen(background);
            GameManager.ScreenManager.addScreen(new PauseScreen(GameManager));
        }
        
        if (GameManager.ScreenManager.InGameScreen.PlayerState.Health <= 0) return;
        
        // check for weapon switch
        if (GameManager.UserActionHandler.isUserAction(PlayerCharacterId, UserAction.SwitchWeapon, PressType.PressAndRelease)) 
            switchWeapon();
        
        // check for weapon execution
        if (GameManager.UserActionHandler.isUserAction(PlayerCharacterId, UserAction.Execute,
                PressType.PressAndRelease) && !IsInteracting && !IsInventoryOpen)
            getComponent<ActiveCombatant>().attack(getComponent<Collider>(), DirectionFov, PlayerCharacterId);

        // check for inventory
        if (GameManager.UserActionHandler.isUserAction(PlayerCharacterId, UserAction.Inventory,
                PressType.PressAndRelease))
        {
            if (!GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.IsInventoryOpen && !GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.IsInventoryOpen)
            {
                GameManager.ScreenManager.addScreen(_inventoryScreen);
                IsInventoryOpen = true;
            }
            else
            {
                GameManager.ScreenManager.removeFromStack(_inventoryScreen);
                IsInventoryOpen = false;
            }
        }
    }

    private bool checkForCaught()
    {
        if (GameManager.ScreenManager.InGameScreen.PlayerState.WantedLevel == 0 || GameManager.ScreenManager.InGameScreen.PlayerState.WantedLevel >= 4 || IsHidden || GameManager.ScreenManager.InGameScreen.PlayerState.Health >= 5 * settings.EnemyHealthMultiplier) return false;
        
        var neighbors = GameManager.ScreenManager.InGameScreen.CollisionManager.getNeighborsInRadius(getComponent<Collider>(), 1);

        if (neighbors.Any(neighbor => neighbor.IsPolice && neighbor.InactiveCombatant.Health > 0))
            if (!GameManager.ScreenManager.InGameScreen.PlayerState.IsCaught && GameManager.ScreenManager.InGameScreen.PlayerState.Health > 0)
            {
                GameManager.ScreenManager.InGameScreen.PlayerState.CaughtTime = DateTime.Now;
                GameManager.ScreenManager.InGameScreen.PlayerState.IsCaught = true;
                _timeSinceIsCaught = 6.5f * 60;
            }
        
        return GameManager.ScreenManager.InGameScreen.PlayerState.IsCaught;
    }
    
    public override void render(GameTime gameTime)
    {
        // currently selected animation that should be drawn
        var currentAnimation = getComponent<Animatable>().CurrentAnimation;
        // rectangle that represents the image on the SpriteSheet that is currently needed for animation
        var sourceRectangle = currentAnimation.CurrentRectangle;
        
        // offset to position of the rendered image, so that the actual position is in the middle of the feet
        var renderPositionOffSet = new Point(-(int)(sourceRectangle.Width * ImageScalingFactor) / 2, -((int)(sourceRectangle.Height * ImageScalingFactor)));
        // scales the size of the rendered image by ImageScalingFactor
        var scaledSize = new Point((int)Math.Round(sourceRectangle.Width * ImageScalingFactor), (int)Math.Round(sourceRectangle.Height * ImageScalingFactor));
        // rectangle the image should be drawn in
        var destinationRectangle = new Rectangle(Position.ToPoint() + renderPositionOffSet, scaledSize);
        
        // check if last direction was to left or right and based on that mirror the image
        //var spriteEffect = _directionAnimation.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        var spriteEffect = _movingAverageDirection.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        
        // almost 100,000 different layer depths for drawing the characters
        float layerDepth = Position.Y / 100000;
        
        if(!IsHidden && !IsInCar)
            GameManager.SpriteBatch.Draw(currentAnimation.SpriteSheet, destinationRectangle, sourceRectangle, Color.White, 0f, Vector2.Zero, spriteEffect, layerDepth);
        
        if (GameManager.InputHandler.isPressed(GameManager.SettingsManager.DrawHitBoxKey, PressType.HoldWithoutRelease))
            getComponent<Collider>().drawHitBox(); 
        if (GameManager.InputHandler.isPressed(GameManager.SettingsManager.DrawTileCollisionRects, PressType.HoldWithoutRelease))
            getComponent<Collider>().drawTileCollisionHitBox();
        if (EquippedWeapon == Weapon.Gun || GameManager.InputHandler.isPressed(GameManager.SettingsManager.DrawHitBoxKey, PressType.HoldWithoutRelease))
            getComponent<Collider>().drawNeighboursInFov(Radius, DirectionFov, Fov);
    }

    public override void draw(GameTime gameTime)
    {
        if(_timeSinceIsCaught > 0)
        {
            _textboxWasted = new TextBox(GameManager, new Vector2(GameManager.SettingsManager.Resolution.Width / 2f, GameManager.SettingsManager.Resolution.Height / 2f), "You are arrested!!!!", GameManager.AssetManager.Fonts["numbers"], 1.5f, Color.White);
            _textboxWasted.draw();
        }
    }

    public override void saveState(GameState gameState)
    {
        NpcType type = PlayerCharacterId == PlayerCharacterId.Walt ? NpcType.Walt : NpcType.Jesse;
        gameState.SavedGameObjects.Add(new SavedGameObject(
            ObjectId, type, Position.X, Position.Y, PlayerCharacterId,
            IsHidden, IsInCar, IsInventoryOpen, EquippedWeapon,
                    (DirectionFov.X, DirectionFov.Y), (_directionAnimation.X, _directionAnimation.Y)));
    }

    public override void loadState(SavedGameObject gameObject)
    {
        ObjectId = gameObject.ObjectId;
        Position = new Vector2(gameObject.X, gameObject.Y);
        IsHidden = gameObject.IsHidden;
        IsInCar = gameObject.IsInteracting;
        EquippedWeapon = gameObject.EquippedWeapon;
        DirectionFov = new Vector2(gameObject.DirectionFov.Item1, gameObject.DirectionFov.Item2);
        _directionAnimation = new Vector2(gameObject.DirectionAnimation.Item1, gameObject.DirectionAnimation.Item2);
    }
    
    private void loadAnimations()
    {
        const int rectangleWidth = 128; 
        const int rectangleHeight = 128; 
        var animatable = getComponent<Animatable>();
        
        animatable.addAnimation("idle", new Animation(PlayerCharacterId switch
        {
            PlayerCharacterId.Walt => GameManager.AssetManager.Images["Walt_Idle"],
            PlayerCharacterId.Jesse => GameManager.AssetManager.Images["Jesse_Idle"],
            _ => null
        }, 7, rectangleWidth, rectangleHeight, 150, 0, true));
        
        animatable.addAnimation("running", new Animation(PlayerCharacterId switch
        {
            PlayerCharacterId.Walt => GameManager.AssetManager.Images["Walt_Running"],
            PlayerCharacterId.Jesse => GameManager.AssetManager.Images["Jesse_Running"],
            _ => null
        }, 10, rectangleWidth, rectangleHeight, 60, 0, true));
        
        animatable.addAnimation("punch", new Animation(PlayerCharacterId switch
        {
            PlayerCharacterId.Walt => GameManager.AssetManager.Images["Walt_Punch"],
            PlayerCharacterId.Jesse => GameManager.AssetManager.Images["Jesse_Punch"],
            _ => null
        }, PlayerCharacterId switch
        {
            PlayerCharacterId.Walt => 5,
            PlayerCharacterId.Jesse => 6,
            _ => 0
        }, rectangleWidth, rectangleHeight, 60, 1, false));
        
        animatable.addAnimation("shoot", new Animation(PlayerCharacterId switch
        {
            PlayerCharacterId.Walt => GameManager.AssetManager.Images["Walt_Shot"],
            PlayerCharacterId.Jesse => GameManager.AssetManager.Images["Jesse_Shot"],
            _ => null
        }, 12, rectangleWidth, rectangleHeight, 40, 1, false));
        
        animatable.addAnimation("hurt", new Animation(PlayerCharacterId switch
        {
            PlayerCharacterId.Walt => GameManager.AssetManager.Images["Walt_Hurt"],
            PlayerCharacterId.Jesse => GameManager.AssetManager.Images["Jesse_Hurt"],
            _ => null
        }, 4, rectangleWidth, rectangleHeight, 60, 2, false));
        
        animatable.addAnimation("die", new Animation(PlayerCharacterId switch
        {
            PlayerCharacterId.Walt => GameManager.AssetManager.Images["Walt_Dead"],
            PlayerCharacterId.Jesse => GameManager.AssetManager.Images["Jesse_Dead"],
            _ => null
        }, PlayerCharacterId switch
        {
            PlayerCharacterId.Walt => 5,
            PlayerCharacterId.Jesse => 4,
            _ => 0
        }, rectangleWidth, rectangleHeight, 150, 3, false, true));
        
        animatable.CurrentAnimationName = "idle";
        animatable.CurrentAnimation.start();
    }
}
