using breaking_worse.Input.Enums;
using breaking_worse.input.KeyTypes;
using breaking_worse.Objects;
using breaking_worse.Objects.Collisions;
using breaking_worse.Objects.Player;
using breaking_worse.Objects.Scene;
using breaking_worse.Screens.ScreenTypes.MenuScreens;
using breaking_worse.State.Serializable;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace breaking_worse.Screens;

public class InGameScreen : AScreen

{
    /// <summary>
    /// most important Screen of all
    /// holds all the gameObjects
    /// </summary>

    public InGameScreen(GameManager gameManager, int gameStateId = -1, GameState gameState = null,
        bool techDemo = false) : base(gameManager, false, false)
    {
        GameState = gameState;
        GameStateId = gameStateId;
        if (techDemo) TechDemo = new TechDemo(gameManager);
    }

    public int GameStateId { get; set; }
    private GameState GameState { get; set; }
    public TechDemo TechDemo { get; set; } = null;

    public CollisionManager CollisionManager { get; private set; }

    private bool _firstRender;
    
    // scene stuff
    public Map Map { get; private set; }
    public Camera Camera { get; private set; }

    // players stored here so that they can be distributed
    public PlayerState PlayerState { get; private set; }
    public GameObjectManager GameObjectManager { get; private set; }
    
    private readonly KeyboardKey _resetWanted = new(Keys.N);


    public void initialize()
    {
        // order matters, do not change if you're not sure

        Map = new Map(GameManager);
        // needs to be loaded when creating GameObjectManager
        Map.loadContent();
        // Collision Manager takes a withoutFence matrix to handle the shooting over fences
        CollisionManager = new CollisionManager(Map.getCollisionMatrix(withoutFence: true));
        PlayerState = new PlayerState(GameManager);
        GameObjectManager = new GameObjectManager(GameManager, this);
        PlayerState.balanceHealth();
        
        Camera = new Camera(GameManager, this);
        
        loadContent();
        loadGameState(GameState);
        GameManager.SoundManager.playMusic("MusicTemplate_InGame");
    }

    public override void update(GameTime gameTime)
    {
        CollisionManager.update(GameObjectManager.getObjectsThatHaveComponent<Collider>());
        Camera.update(gameTime);
        GameObjectManager.update(gameTime);
        PlayerState.update(gameTime);
        checkForEnding();
        TechDemo?.update(gameTime);
        if (GameManager.InputHandler.isPressed(_resetWanted, PressType.PressAndRelease))
        {
            GameManager.ScreenManager.InGameScreen.PlayerState.SuspiciousActivityCounter = 0;
        }
    }
    
    private void checkForEnding()
    {
        if (PlayerState.Timer != 0 && PlayerState.Deaths < 3) return;
        GameManager.ScreenManager.removeFromStack(this);
        GameManager.ScreenManager.addScreen(new BackgroundScreen(GameManager, GameManager.AssetManager.Images["gameOverImage"]));
        GameManager.ScreenManager.addScreen(new GameEndedScreen(GameManager, false));
    }
    
    public void render(GameTime gameTime)
    {
        if (!_firstRender)
        {
            // render first layers of map to base render target if it's the first render time
            GameManager.GraphicsDeviceManager.GraphicsDevice.SetRenderTarget(GameManager.InGameScreenRenderTarget2DBase);
            GameManager.GraphicsDeviceManager.GraphicsDevice.Clear(Color.Black);
            
            GameManager.SpriteBatch.Begin(SpriteSortMode.FrontToBack);
            Map.render(gameTime, true);
            GameManager.SpriteBatch.End();
            
            // render second to last layer of map to top render target
            GameManager.GraphicsDeviceManager.GraphicsDevice.SetRenderTarget(GameManager.InGameScreenRenderTarget2DTop);
            GameManager.GraphicsDeviceManager.GraphicsDevice.Clear(Color.Transparent);
            
            GameManager.SpriteBatch.Begin(SpriteSortMode.FrontToBack);
            Map.render(gameTime, false);
            GameManager.SpriteBatch.End();
            
            // render last layer of map to mini map render target
            GameManager.GraphicsDeviceManager.GraphicsDevice.SetRenderTarget(GameManager.MiniMapRenderTarget2D);
            GameManager.GraphicsDeviceManager.GraphicsDevice.Clear(Color.Transparent);
            
            GameManager.SpriteBatch.Begin(SpriteSortMode.FrontToBack);
            Map.render(gameTime, false, true);
            GameManager.SpriteBatch.End();
            
            _firstRender = true;
        }

        // render everything else
        GameManager.GraphicsDeviceManager.GraphicsDevice.SetRenderTarget(GameManager.InGameScreenRenderTarget2D);
        GameManager.GraphicsDeviceManager.GraphicsDevice.Clear(Color.Transparent);
        GameManager.SpriteBatch.Begin(SpriteSortMode.FrontToBack);
        
        // only for the paint tile collisions functions
        Map.draw();
        GameObjectManager.render(gameTime);
        
        GameManager.SpriteBatch.End();
        GameManager.GraphicsDeviceManager.GraphicsDevice.SetRenderTarget(null);
    }

    public override void draw(GameTime gameTime)
    {
        GameManager.SpriteBatch.Draw(GameManager.InGameScreenRenderTarget2DBase,
            new Rectangle(0, 0, GameManager.SettingsManager.Resolution.Width, GameManager.SettingsManager.Resolution.Height),
            Camera.Bounds, Color.White);
        GameManager.SpriteBatch.Draw(GameManager.InGameScreenRenderTarget2D,
            new Rectangle(0, 0, GameManager.SettingsManager.Resolution.Width, GameManager.SettingsManager.Resolution.Height),
            Camera.Bounds, Color.White);
        GameManager.SpriteBatch.Draw(GameManager.InGameScreenRenderTarget2DTop,
            new Rectangle(0, 0, GameManager.SettingsManager.Resolution.Width, GameManager.SettingsManager.Resolution.Height),
            Camera.Bounds, Color.White);
        GameObjectManager.draw(gameTime);
    }
    
    private void loadGameState(GameState gameState)
    {
        if (gameState == null)
        {
            GameManager.ProgressManager.resetStats();
            return;
        }
        PlayerState.loadGameState(gameState.SavedPlayerState);
        GameObjectManager.loadState(gameState);
        GameManager.ProgressManager.loadStatistics(gameState);
    }

    public void saveGameState()
    {
        if (GameStateId == -1) return;
        var gameState = new GameState();
        var achivementState = new AchievementState();
        PlayerState.saveGameState(gameState);
        GameObjectManager.saveState(gameState);
        GameManager.ProgressManager.saveState(gameState, achivementState);
        GameManager.SaveManager.writeGameStateToFile(gameState, GameStateId);
        GameManager.SaveManager.saveAchievementsToFile(achivementState);
    }

    public override void loadContent()
    {
        GameObjectManager.loadContent();
    }

    public override void changeResolution()
    {
        Camera.changeScreenResolution();
    }
}
