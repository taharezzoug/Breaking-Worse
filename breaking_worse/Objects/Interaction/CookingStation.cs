using System;
using System.Collections.Generic;
using breaking_worse.Input;
using breaking_worse.Input.Enums;
using breaking_worse.Objects.Collisions;
using breaking_worse.Objects.Items;
using breaking_worse.Objects.Player;
using breaking_worse.Screens;
using breaking_worse.Screens.Clickables;
using breaking_worse.Screens.ScreenTypes.InGameScreens;
using breaking_worse.Sound;
using breaking_worse.State.Enums;
using breaking_worse.State.Serializable;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Achievement = breaking_worse.State.Enums.Achievement;

namespace breaking_worse.Objects.Interaction;

public class CookingStation : AGameObject
{ 
    private readonly GameManager _gameManager;
    public Texture2D Texture { get; set; }
    private Texture2D _backgroundCookingStation;
    private Texture2D _progressBar;
    private Texture2D _progressIndicator;
    private SpriteFont _font;
    private SpriteFont _fontNumbers;
    
    private const float ImageScalingFactor = 0.1f;
  
    // HitBox stuff
    private const int DynamicHitBoxWidth = 80;
    private const int DynamicHitBoxHeight = 20;
    
    // interaction radius
    private const int Radius = 3;
    
    private Rectangle _backgroundSize;
    private Point _borderSize;
    private float _scale;
    private int _originalScreenWidth;
    
    private Vector2 _positionDialogueWalt;
    private Vector2 _positionDialogueJesse;
    private bool _waltCooking;
    
    public MiniGame MiniGame;


    private TextBox _cookingDesc;
    private TextBox _cookingLabel;

    private Rectangle _miniGameProgressBar;
    private Rectangle _miniGameProgressIndicator;
    private Rectangle _miniGameWinIndicator;
    private List<(Rectangle, (Texture2D, Color))> _miniGameWaltKeyChain;
    private List<(Rectangle, (Texture2D, Color))> _miniGameJesseKeyChain;
    private Rectangle _waltHitBar;
    private Rectangle _jesseHitBar;
    
    private Texture2D _miniGameHealth;
    
    private Texture2D _miniGameKeywDark;
    private Texture2D _miniGameKeywLight;
    private Texture2D _miniGameKeyaDark;
    private Texture2D _miniGameKeyaLight;
    private Texture2D _miniGameKeyssDark;
    private Texture2D _miniGameKeyssLight;
    private Texture2D _miniGameKeysdDark;
    private Texture2D _miniGameKeysdLight;
    private Texture2D _miniGameKeyssUpDark;
    private Texture2D _miniGameKeyssUpLight;
    private Texture2D _miniGameKeyssDownDark;
    private Texture2D _miniGameKeyssDownLight;
    private Texture2D _miniGameKeyssLeftDark;
    private Texture2D _miniGameKeyssLeftLight;
    private Texture2D _miniGameKeyssRightDark;
    private Texture2D _miniGameKeyssRightLight;
    
    private Texture2D _miniGameFlask0;
    private Texture2D _miniGameFlask1;
    private Texture2D _miniGameFlask2;
    private Texture2D _miniGameFlask3;
    private Texture2D _miniGameFlask4;
    private Texture2D _miniGameFlaskWon;
    private Texture2D _miniGameFlaskLost;

    private Texture2D _hitBarBackGround;
    
    
    public readonly Inventory Inventory;

    public int upgradeLevel { get; set; }
    
    private List<Recipe> _recipes;


    private int _ticker;
    
    public CookingStation(GameManager gameManager, InGameScreen inGameScreen) : base(gameManager)
    {
        Type = NpcType.CookingStation;
        _gameManager = gameManager;
        Position = new Vector2(146 * 32 - 15, 71 * 32 - 8);
        Inventory = inGameScreen.PlayerState.Inventory;
        MiniGame = new MiniGame(gameManager);
        
        addComponent(new Dialogable(gameManager, createDialogueScreen, Radius, true));
        addComponent(new Collider(gameManager, () => Position,
           Vector2.Zero));
        
        var dynamicHitBox = getComponent<Collider>().DynamicHitBox;
        dynamicHitBox.Width = DynamicHitBoxWidth;
        dynamicHitBox.Height = DynamicHitBoxHeight;
        
        upgradeLevel = 0;
        _recipes = new List<Recipe>();
        
        var _xScale = GameManager.GraphicsDeviceManager.GraphicsDevice.Viewport.Width / 1920f;
        var _yScale = GameManager.SettingsManager.Resolution.Height / 1080f;
        _scale = Math.Min(_xScale, _yScale);               
        _originalScreenWidth = gameManager.SettingsManager.Resolution.Width;
        _positionDialogueWalt = new Vector2(200 * _scale, GameManager.SettingsManager.Resolution.Height * 0.2f / _scale);
        _positionDialogueJesse = new Vector2(GameManager.SettingsManager.Resolution.Width - 200 * _scale, GameManager.SettingsManager.Resolution.Height * 0.2f / _scale);
    }
    
    private DialogueScreen createDialogueScreen(PlayerCharacterId playerCharacterId)
    {
        _waltCooking = playerCharacterId == PlayerCharacterId.Walt;
        var dialogueScreen = _waltCooking ? 
            new DialogueScreen(GameManager, _positionDialogueWalt, [PlayerCharacterId.Walt, PlayerCharacterId.Jesse]) : 
            new DialogueScreen(GameManager, _positionDialogueJesse, [PlayerCharacterId.Walt, PlayerCharacterId.Jesse]);
        dialogueScreen = createOnScreenElements(dialogueScreen);
        GameManager.SoundManager.playSoundEffect(Sfx.CraftingStation);
        return dialogueScreen;
    }

    public override void update(GameTime gameTime)
    {
        getComponent<Dialogable>().update(getComponent<Collider>());
        
        if (_originalScreenWidth != GameManager.SettingsManager.Resolution.Width)
        {
            var _xScale = GameManager.GraphicsDeviceManager.GraphicsDevice.Viewport.Width / 1920f;
            var _yScale = GameManager.SettingsManager.Resolution.Height / 1080f;
            _scale = Math.Min(_xScale, _yScale);            
            _originalScreenWidth = GameManager.SettingsManager.Resolution.Width;
            _positionDialogueWalt = new Vector2(200 * _scale, GameManager.SettingsManager.Resolution.Height * 0.2f / _scale);
            _positionDialogueJesse = new Vector2(GameManager.SettingsManager.Resolution.Width - 200 * _scale, GameManager.SettingsManager.Resolution.Height * 0.2f / _scale);
        }
    }
    
    public override void render(GameTime gameTime)
    {
        float layerDepth = Position.Y / 100000f;
        // scales the size of the rendered image by ImageScalingFactor
        var scaledSize = new Point((int)Math.Round(Texture.Width * ImageScalingFactor), (int)Math.Round(Texture.Height * ImageScalingFactor));
        GameManager.SpriteBatch.Draw(Texture, new Rectangle(Position.ToPoint(), scaledSize), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
        if (_gameManager.InputHandler.isPressed(_gameManager.SettingsManager.DrawHitBoxKey, PressType.HoldWithoutRelease))
        {
            getComponent<Collider>().drawHitBox();
            getComponent<Collider>().drawNeighboursInRadius(Radius);
        }
    }
    
    public bool hasIngredients(List<(Item, int)> ingredients)
    {
        var inventoryDic = new Dictionary<ItemName, int>();
        foreach (var item in Inventory.Items)
        {
            if (!inventoryDic.ContainsKey(item.Key)) { inventoryDic.Add(item.Key, item.Value); }
            else
            {
                inventoryDic[item.Key] += item.Value;
            }
        }

        foreach ((Item, int) ingredient in ingredients)
        {
            if (!inventoryDic.ContainsKey(ingredient.Item1.Name) ||
                inventoryDic[ingredient.Item1.Name] < ingredient.Item2)
            {
                return false;
            }
        }
        return true;
    }
    
    public override void loadContent()
    {
        Texture = GameManager.AssetManager.Images["CookingStation1"];
        _backgroundCookingStation = GameManager.AssetManager.Images["CookingStationBackground"];
        _hitBarBackGround = GameManager.AssetManager.Images["DialogBackground"];
        _progressBar = GameManager.AssetManager.Images["progressBar"];
        _progressIndicator = GameManager.AssetManager.Images["progressIndicator"];
        
        
        _font = GameManager.AssetManager.Fonts["font"];
        _fontNumbers = GameManager.AssetManager.Fonts["numbers"];
        
        _miniGameKeywDark = GameManager.AssetManager.Images["wFull"];
        _miniGameKeywLight = GameManager.AssetManager.Images["wLight"];
        _miniGameKeyaDark = GameManager.AssetManager.Images["aFull"];
        _miniGameKeyaLight = GameManager.AssetManager.Images["aLight"];
        _miniGameKeyssDark = GameManager.AssetManager.Images["sFull"];
        _miniGameKeyssLight = GameManager.AssetManager.Images["sLight"];
        _miniGameKeysdDark = GameManager.AssetManager.Images["dFull"];
        _miniGameKeysdLight = GameManager.AssetManager.Images["dLight"];
        _miniGameKeyssUpDark = GameManager.AssetManager.Images["upFull"];
        _miniGameKeyssUpLight = GameManager.AssetManager.Images["upLight"];
        _miniGameKeyssDownDark = GameManager.AssetManager.Images["downFull"];
        _miniGameKeyssDownLight = GameManager.AssetManager.Images["downLight"];
        _miniGameKeyssLeftDark = GameManager.AssetManager.Images["leftFull"];
        _miniGameKeyssLeftLight = GameManager.AssetManager.Images["leftLight"];
        _miniGameKeyssRightDark = GameManager.AssetManager.Images["rightFull"];
        _miniGameKeyssRightLight = GameManager.AssetManager.Images["rightLight"];
        
        _miniGameFlask0 = GameManager.AssetManager.Images["Flask0"];
        _miniGameFlask1 = GameManager.AssetManager.Images["Flask1"];
        _miniGameFlask2 = GameManager.AssetManager.Images["Flask2"];
        _miniGameFlask3 = GameManager.AssetManager.Images["Flask3"];
        _miniGameFlask4 = GameManager.AssetManager.Images["Flask4"];
        _miniGameFlaskWon = GameManager.AssetManager.Images["FlaskWon"];
        _miniGameFlaskLost = GameManager.AssetManager.Images["FlaskLost"];
        
        _miniGameHealth = GameManager.AssetManager.Images["health"];
    }
    
    public override void draw(GameTime gameTime) {}

    private DialogueScreen createOnScreenElements(DialogueScreen dialogueScreen)
    {
        // Positions of Background Boxes
        _backgroundSize = new Rectangle(
            (int)(GameManager.SettingsManager.Resolution.Width * 0.1f),
            (int)(GameManager.SettingsManager.Resolution.Height * 0.1f),
            (int)(GameManager.SettingsManager.Resolution.Width * 0.8f),
            (int)(GameManager.SettingsManager.Resolution.Height * 0.8f)
        );
        _borderSize = new Point(
            (int)(_backgroundSize.Width * 0.2f),
            (int)(_backgroundSize.Height * 0.05f));
        
        dialogueScreen.Rectangles.Add((_backgroundSize, (_backgroundCookingStation, Color.White)));
        
        // Title Section
        var cookingText = "Chemistry Station";
        _cookingDesc = new TextBox(GameManager, new Vector2(_backgroundSize.X + _backgroundSize.Width / 6f, _backgroundSize.Y + _backgroundSize.Height / 12f), cookingText, _font,1.3f ,Color.White, false);
        _cookingLabel = new TextBox(GameManager, new Vector2(_backgroundSize.X + _backgroundSize.Width * 0.8f, _backgroundSize.Y + _backgroundSize.Height / 12f), "Level " + upgradeLevel, _fontNumbers,1.3f ,Color.White);
        dialogueScreen.TextBoxes.Add(_cookingDesc);
        dialogueScreen.TextBoxes.Add(_cookingLabel);
        
        // Button drawer
        dialogueScreen.ButtonCollection.setButtonSize(460, 80);
        dialogueScreen.ButtonCollection.addButtonAtPosition("selectRecipe", "Select Recipe", new Vector2(_backgroundSize.X + _backgroundSize.Width / 2f,
            (_backgroundSize.Y + _backgroundSize.Height / 4f) / _scale));
        // dialogueScreen.ButtonCollection.addButton("tutorial", "Tutorial");
        if (Inventory.Money < 15000 + 15000 * upgradeLevel)
            dialogueScreen.ButtonCollection.addButton("upgrade",
                "Upgrade with " + 15000 + 15000 * upgradeLevel + "$");
        else if (upgradeLevel < 3)
            dialogueScreen.ButtonCollection.addButton("upgrade", "Upgrade to Level " + (upgradeLevel + 1));
        else
        {
            dialogueScreen.ButtonCollection.addButton("upgraded", "Fully Upgraded");
        }
        dialogueScreen.ButtonCollection.addButton("return-1", "Close");
        
        
        // initialize Recipes
        _recipes.Clear();
        foreach (var itemname in Enum.GetValues<ItemName>())
        {
            Recipe recipe = new Recipe(itemname);
            if (recipe.RecipeItemList.Count > 0)
                _recipes.Add(recipe);
        }
        
        _miniGameProgressBar = new Rectangle(new Point(_backgroundSize.X + _backgroundSize.Width * 2 / 5, _backgroundSize.Y + _backgroundSize.Height / 5), new Point((int)(400 * _scale), (int)(40 * _scale)));
        _miniGameProgressIndicator = new Rectangle(
            new Point(_backgroundSize.X + _backgroundSize.Width * 2 / 5, _backgroundSize.Y + _backgroundSize.Height / 5),
            new Point((int)(400 * _scale), (int)(40 * _scale)));
        
        _jesseHitBar = new Rectangle(
            new Point(_backgroundSize.X + _backgroundSize.Width * 3 / 4 + _borderSize.X / 4, _backgroundSize.Y + _backgroundSize.Height * 3 / 4), 
            new Point(_backgroundSize.Width / 4 - _borderSize.X / 2, (int)(72 * _scale)));
        _waltHitBar = new Rectangle(
            new Point(_backgroundSize.X + _backgroundSize.Width / 4 - _borderSize.X * 3 / 4, _backgroundSize.Y + _backgroundSize.Height * 3 / 4), 
            new Point(_backgroundSize.Width / 4 - _borderSize.X / 2, (int)(72 * _scale)));
        
        return dialogueScreen;
    }

    private Texture2D matchIkeyToTexture(IKey key, bool dark = false)
    {
        switch (key.ToString())
        {
            case "W":
                return dark ? _miniGameKeywDark : _miniGameKeywLight;
            case "A":
                return dark ? _miniGameKeyaDark : _miniGameKeyaLight;
            case "S":
                return dark ? _miniGameKeyssDark : _miniGameKeyssLight;
            case "D":
                return dark ? _miniGameKeysdDark : _miniGameKeysdLight;
            case "Up":
                return dark ? _miniGameKeyssUpDark : _miniGameKeyssUpLight;
            case "Down":
                return dark ? _miniGameKeyssDownDark : _miniGameKeyssDownLight;
            case "Left":
                return dark ? _miniGameKeyssLeftDark : _miniGameKeyssLeftLight;
            case "Right":
                return dark ? _miniGameKeyssRightDark : _miniGameKeyssRightLight;
            case "DPadUp":
                return dark ? _miniGameKeyssUpDark : _miniGameKeyssUpLight;
            case "DPadDown":
                return dark ? _miniGameKeyssDownDark : _miniGameKeyssDownLight;
            case "DPadLeft":
                return dark ? _miniGameKeyssLeftDark : _miniGameKeyssLeftLight;
            case "DPadRight":
                return dark ? _miniGameKeyssRightDark : _miniGameKeyssRightLight;
            default:
                return null;
        }
    }

    private (Rectangle, (Texture2D, Color)) getMiniGameState()
    {
        _miniGameWinIndicator = new Rectangle(
            new Point(_backgroundSize.X + _backgroundSize.Width / 3, _backgroundSize.Y + _backgroundSize.Height / 4), 
            new Point((int)(600 * _scale), (int)(600 * _scale)));
        if (MiniGame.gameWon())
        {
            return (_miniGameWinIndicator, (_miniGameFlaskWon, Color.White));
        }
        if (MiniGame.gameOver())
        {
            return (_miniGameWinIndicator, (_miniGameFlaskLost, Color.White));
        }
        switch (MiniGame.CurrentCorrectHits / (float)MiniGame.TotalGameRounds)
        {
            case <= 0.2f:
                return (_miniGameWinIndicator, (_miniGameFlask0, Color.White));
            case <= 0.4f:
                return (_miniGameWinIndicator, (_miniGameFlask1, Color.White));
            case <= 0.6f:
                return (_miniGameWinIndicator, (_miniGameFlask2, Color.White));
            case <= 0.8f:
                return (_miniGameWinIndicator, (_miniGameFlask3, Color.White));
            case <= 1f:
                return (_miniGameWinIndicator, (_miniGameFlask4, Color.White));
            default:
                return (_miniGameWinIndicator, (_miniGameFlask0, Color.White));
        }
    }

    private void getCurrentKeys(int trace)
    {
        _miniGameJesseKeyChain = _miniGameWaltKeyChain = [];
        var KeyPositionWalt = new Point(_backgroundSize.X + _backgroundSize.Width / 4 - _borderSize.X / 2, _backgroundSize.Y + _backgroundSize.Height * 3 / 4);
        var KeyPositionJesse = new Point(_backgroundSize.X + _backgroundSize.Width * 3 / 4 + _borderSize.X / 2, _backgroundSize.Y + _backgroundSize.Height * 3 / 4);
        var KeySize = new Point((int)(64 * _scale), (int)(64 * _scale));
        _miniGameWaltKeyChain.Add((new Rectangle(KeyPositionWalt, KeySize), 
            (matchIkeyToTexture(_gameManager.SettingsManager.Controls[
            (_gameManager.InputHandler.InputDevices.getInputDevice(PlayerCharacterId.Walt), 
                MiniGame.currentKeyWalt())],
                true), Color.White)));
        _miniGameJesseKeyChain.Add((new Rectangle(KeyPositionJesse, KeySize), 
            (matchIkeyToTexture(_gameManager.SettingsManager.Controls[
                    (_gameManager.InputHandler.InputDevices.getInputDevice(PlayerCharacterId.Jesse), 
                        MiniGame.currentKeyJesse())], true), Color.White)));
        KeySize = new Point((int)(56 * _scale), (int)(56 * _scale));
        if (trace + 1 < MiniGame.nextKeyWalt().Count)
        {
            for (int i = MiniGame.nextKeyWalt().Count - (2 + trace); i >= 0; i--)
            {
                KeyPositionWalt = new Point(KeyPositionWalt.X, KeyPositionWalt.Y - (int)(64 * _scale));
                KeyPositionJesse = new Point(KeyPositionJesse.X, KeyPositionJesse.Y - (int)(64 * _scale));
                _miniGameJesseKeyChain.Add((new Rectangle(KeyPositionJesse, KeySize), 
                    (matchIkeyToTexture(_gameManager.SettingsManager.Controls[(_gameManager.InputHandler.InputDevices.getInputDevice(PlayerCharacterId.Jesse), 
                        MiniGame.nextKeyJesse()[i])]), Color.White)));
                _miniGameWaltKeyChain.Add((new Rectangle(KeyPositionWalt, KeySize),
                    (matchIkeyToTexture(_gameManager.SettingsManager.Controls
                    [(_gameManager.InputHandler.InputDevices.getInputDevice(PlayerCharacterId.Walt), 
                    MiniGame.nextKeyWalt()[i])]), Color.White)));
            }
        }
    }

    private List<(Rectangle, (Texture2D, Color))> setHealth()
    {
        var health = new List<(Rectangle, (Texture2D, Color))>();
        var healthPos = new Point(_backgroundSize.X + _backgroundSize.Width / 3 + _borderSize.X / 2, _borderSize.Y + _backgroundSize.Height / 5);
        var healthSize = new Point((int)(64 * _scale), (int)(64 * _scale));
        for (int i = 0; i < MiniGame.remainingMistakes(); i++)
        {
            health.Add((new Rectangle(healthPos, healthSize), (_miniGameHealth, Color.White)));
            healthPos = new Point(healthPos.X + (int)(72 * _scale), healthPos.Y);
        }
        return health;
    }
    

    private ButtonCollection resetButtons()
    {
        return _waltCooking ? new ButtonCollection(GameManager, _positionDialogueWalt, [PlayerCharacterId.Walt, PlayerCharacterId.Jesse]) : new ButtonCollection(GameManager, _positionDialogueJesse, [PlayerCharacterId.Walt, PlayerCharacterId.Jesse]);
    }

    public ItemName matchNameToItem(string name)
    {
        switch (name)
        {
            case "Pseudoephedrine":
                return ItemName.Pseudoephedrine;
            case "Red Phosphorus":
                return ItemName.RedPhosphorus;
            case "Meth Small":
                return ItemName.MethSmall;
            case "Aluminium Granules":
                return ItemName.AluminumGranules;
            case "Meth":
                return ItemName.Meth;
            case "Cocaine":
                return ItemName.Cocaine;
            case "Crack":
                return ItemName.Crack;
            default:
                return ItemName.MethSmall;
        }
    }

    private string matchItemToName(ItemName item)
    {
        switch (item)
        {
            case ItemName.Pseudoephedrine:
                return "Pseudoephedrine";
            case ItemName.RedPhosphorus:
                return "Red Phosphorus";
            case ItemName.MethSmall:
                return "Meth Small";
            case ItemName.AluminumGranules:
                return "Aluminium Granules";
            case ItemName.Meth:
                return "Meth";
            case ItemName.Cocaine:
                return "Cocaine";
            case ItemName.Crack:
                return "Crack";
            default:
                return "";
        }
    }

    public ButtonCollection updateButtons(ButtonCollection buttons, int phaseIndex)
    {
        switch (phaseIndex) 
        {
            case 0:
                
                buttons = resetButtons();
                
                buttons.setButtonSize(460);
                buttons.addButtonAtPosition("selectRecipe", "Select Recipe", new Vector2(_backgroundSize.X + _backgroundSize.Width / 2f,
                    (_backgroundSize.Y + _backgroundSize.Height / 4f) / _scale));
                if (Inventory.Money < 15000 + 15000 * upgradeLevel)
                    buttons.addButton("upgrade",
                        "Upgrade with " + (15000 + 15000 * upgradeLevel) + "$");
                else if (upgradeLevel < 3)
                    buttons.addButton("upgrade", "Upgrade to Level " + (upgradeLevel + 1));
                else
                    buttons.addButton("upgraded", "Fully Upgraded");
                buttons.addButton("return-1", "Close");
                return buttons;
            case 1:
                
                buttons = resetButtons();
                
                var recipesUnlocked = 0;
                var buttonSize = new Point(400, 220);
                buttons.setButtonSize(buttonSize.X, buttonSize.Y);
                for (int i = 0; i < _recipes.Count; i++)
                {
                    var buttonPosition = new Vector2(
                        _backgroundSize.X + _borderSize.X + buttonSize.X * 1.1f * _scale * (i % 3),
                        (_backgroundSize.Y + _borderSize.Y * 2 + buttonSize.Y * _scale * (int)(i / 3)));
                    if (_recipes[i].UpgradeLevel <= upgradeLevel)
                    {
                        var texture = hasIngredients(_recipes[i].RecipeItemList) ? "button" + _recipes[i].Name : "redButton" + _recipes[i].Name; 
                        buttons.addButtonAtPosition(matchItemToName(_recipes[i].Name),
                            matchItemToName(_recipes[i].Name), new Vector2(buttonPosition.X, buttonPosition.Y / _scale), texture, 60f * _scale);
                        recipesUnlocked += 1;
                        GameManager.ProgressManager.updateStatistics(Statistics.RecipesUnlocked, recipesUnlocked, 2);
                        GameManager.ProgressManager.updateAchievements(Statistics.RecipesUnlocked, Achievement.AllRecipes);
                    }
                    else
                        buttons.addButtonAtPosition("dark" + matchItemToName(_recipes[i].Name), "???",
                            new Vector2(buttonPosition.X, buttonPosition.Y / _scale), "darkButton" + _recipes[i].Name, 60f * _scale);
                }
                buttons.setButtonSize();
                buttons.addButtonAtPosition("return0", "Return", new Vector2(_backgroundSize.X + _backgroundSize.Width * 0.85f,
                    (_backgroundSize.Y + _backgroundSize.Height * 0.85f) / (GameManager.SettingsManager.Resolution.Height / 1080f)));
                return buttons;
            case 2:
                buttons = resetButtons();
                buttons.setButtonSize(600);
                buttons.addButtonAtPosition("cook", "Start MiniGame", new Vector2(_backgroundSize.X + _backgroundSize.Width / 2f,
                    (_backgroundSize.Y + _backgroundSize.Height / 4f) / _scale));
                buttons.addButton("cheat", "Cheating: " + (MiniGame.IsCheating ? "on" : "off") + " [toggle]");
                buttons.setButtonSize();
                buttons.addButtonAtPosition("return1", "Return", new Vector2(_backgroundSize.X + _backgroundSize.Width * 0.85f,
                    (_backgroundSize.Y + _backgroundSize.Height * 0.85f) / (GameManager.SettingsManager.Resolution.Height / 1080f)));
                return buttons;
            case 3:
                buttons = resetButtons();

                buttons.addButtonAtPosition("return1", "Return", new Vector2(_backgroundSize.X + _backgroundSize.Width /2f,
                    (_backgroundSize.Y + _backgroundSize.Height * 0.85f) / (GameManager.SettingsManager.Resolution.Height / 1080f)));
                buttons.inMiniGame();
                return buttons;
            case 4:
                buttons = resetButtons();
                buttons.addButtonAtPosition("return2", "Return", new Vector2(_backgroundSize.X + _backgroundSize.Width /2f,
                    (_backgroundSize.Y + _backgroundSize.Height * 0.85f) / (GameManager.SettingsManager.Resolution.Height / 1080f)));
                return buttons;
        }
        return buttons;
    }

    public List<TextBox> updateTextBoxes(List<TextBox> textBoxes, int phaseIndex, string selecteditem = "")
    {
        switch (phaseIndex)
        {
            case 0:
                textBoxes.Clear();
                _cookingDesc = new TextBox(GameManager, new Vector2(_backgroundSize.X + _backgroundSize.Width / 6f, _backgroundSize.Y + _backgroundSize.Height / 12f), "Chemistry Station", _font,1.3f ,Color.Black, false);
                _cookingLabel = new TextBox(GameManager, new Vector2(_backgroundSize.X + _backgroundSize.Width * 0.8f, _backgroundSize.Y + _backgroundSize.Height / 12f), "Level " + upgradeLevel, _fontNumbers,1.3f ,Color.White);
                textBoxes.Add(_cookingDesc);
                textBoxes.Add(_cookingLabel);
                return textBoxes;
            case 1:
                textBoxes.Clear();
                return textBoxes;
            case 2:
                textBoxes.Clear();
                return textBoxes;
            case 3:
                string text = MiniGame.gameWon() ? selecteditem + " made successfully!" : "Failed to cook " + selecteditem + "!";
                var stringLength = _fontNumbers.MeasureString(text) * _scale;
                textBoxes.Add(new TextBox(GameManager, new Vector2(_backgroundSize.X + _backgroundSize.Width / 2f - stringLength.X / 2f, _backgroundSize.Y + _backgroundSize.Height / 12f), text, _fontNumbers, 1f, Color.White,false));
                return textBoxes;
        }
        return textBoxes;
    }

    public List<(Rectangle, (Texture2D, Color))> updateRectangles(List<(Rectangle, (Texture2D, Color))> rectangles, int phaseIndex, Color colorWalt = new Color(), Color colorJesse = new Color())
    {
        switch (phaseIndex)
        {
            case 0:
                rectangles.Clear();
                _ticker = 0;
                rectangles.Add((_backgroundSize, (_backgroundCookingStation, Color.White)));
                break;
            case 1:
                rectangles.Clear();
                rectangles.Add((_backgroundSize, (_backgroundCookingStation, Color.White)));
                return rectangles;
            case 2:
                rectangles.Clear();
                rectangles.Add((_backgroundSize, (_backgroundCookingStation, Color.White)));
                return rectangles;
            case 3:
                rectangles.Clear();
                rectangles.Add((_backgroundSize, (_backgroundCookingStation, Color.White)));
                rectangles.AddRange(setHealth());
                var remainingTime = MiniGame.remainingTime() / 10;
                _miniGameProgressIndicator = new Rectangle(
                    new Point(_backgroundSize.X + _backgroundSize.Width * 2 / 5,
                        _backgroundSize.Y + _backgroundSize.Height / 5),
                    new Point((int)(remainingTime * _scale), (int)(40 * _scale)));
                rectangles.Add((_waltHitBar, (_hitBarBackGround, colorWalt)));
                rectangles.Add((_jesseHitBar, (_hitBarBackGround, colorJesse)));
                _ticker = MiniGame.TotalGameRounds - MiniGame.roundCount();
                if (_ticker < 10)
                {
                    getCurrentKeys(_ticker);
                    rectangles.AddRange(_miniGameJesseKeyChain);
                    rectangles.AddRange(_miniGameWaltKeyChain);
                }
                rectangles.Add(getMiniGameState());
                var indicatorColor = remainingTime < 200 ? Color.Yellow : Color.Green;
                if (remainingTime < 100)
                    indicatorColor = Color.Red;
                rectangles.Add((_miniGameProgressIndicator, (_progressIndicator, indicatorColor)));
                rectangles.Add((_miniGameProgressBar, (_progressBar, Color.White)));
                return rectangles;
            case 4:
                rectangles.Clear();
                rectangles.Add((_backgroundSize, (_backgroundCookingStation, Color.White)));
                rectangles.Add(getMiniGameState());
                return rectangles;
        }
        return rectangles;
    }

    public void quit()
    {
        getComponent<Dialogable>().quit();
    }
    
    public override void saveState(GameState gameState)
    {
        gameState.SavedGameObjects.Add(new SavedGameObject(ObjectId, NpcType.CookingStation, Position.X, Position.Y, upgradeLevel));
    }

    public override void loadState(SavedGameObject gameObject)
    {
        ObjectId = gameObject.ObjectId;
        Position = new Vector2(gameObject.X, gameObject.Y);
        upgradeLevel = gameObject.UpgradeLevel;
    }
}
