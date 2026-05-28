using breaking_worse.Screens.ScreenTypes.InGameScreens;
using breaking_worse.Screens.ScreenTypes.SettingsScreens;
using Microsoft.Xna.Framework;

namespace breaking_worse.Screens.ScreenTypes.MenuScreens;

public class MainMenuScreen : AMenuScreen
{
    /// <summary>
    /// Screen for the main menu (navigates to other menus)
    /// creates the layout for the screen objects (button, texBoxes, headline)
    /// Definition of actions if a button in the screen is clicked
    /// </summary>

    public MainMenuScreen(GameManager gameManager) : base(gameManager)
    {
        createMenuElements();
        
        GameManager.SoundManager.playMusic("MusicTemplate_Menu");
    }

    protected sealed override void createMenuElements()
    {
        createHeadline("Breaking Worse");
        ButtonCollection.addButton("newGame", "New Game");
        ButtonCollection.addButton("loadGame", "Load Game");
        ButtonCollection.addButton("tuto", "Tutorial");
        ButtonCollection.addButton("statistics", "Statistics");
        ButtonCollection.addButton("achievements", "Achievements");
        ButtonCollection.addButton("techDemo", "Techdemo");
        ButtonCollection.addButton("settings", "Settings");
        ButtonCollection.addButton("quit", "Quit");
    }
    
    public override void update(GameTime gameTime)
    {
        base.update(gameTime);
        
        if (ButtonCollection["newGame"].isClicked())
        {
            var inGameScreen = new InGameScreen(GameManager);
            GameManager.ScreenManager.InGameScreen = inGameScreen;
            inGameScreen.initialize();
            GameManager.ScreenManager.addScreen(inGameScreen);
            GameManager.ScreenManager.addScreen(new MiniMapScreen(GameManager));
            GameManager.ScreenManager.addScreen(new OnScreenDisplay(GameManager));
            
            GameManager.ScreenManager.addScreen(new TutorialScreen(GameManager));
        }

        if (ButtonCollection["loadGame"].isClicked())
        {
            GameManager.ScreenManager.addScreen(new LoadGameScreen(GameManager));
        }

        if (ButtonCollection["statistics"].isClicked())
        {
            GameManager.ScreenManager.addScreen(new StatisticsScreen(GameManager, true));
        }

        if (ButtonCollection["achievements"].isClicked())
        {
            GameManager.ScreenManager.addScreen(new AchievementsScreen(GameManager));
        }

        if (ButtonCollection["settings"].isClicked())
        {
            GameManager.ScreenManager.addScreen(new BackgroundScreen(GameManager, GameManager.AssetManager.Images["settingsImage"]));
            GameManager.ScreenManager.addScreen(new SettingsScreen(GameManager, true));
        }

        if (ButtonCollection["techDemo"].isClicked())
        {
            var techDemoScreen = new InGameScreen(GameManager, techDemo: true);
            GameManager.ScreenManager.InGameScreen = techDemoScreen;
            techDemoScreen.initialize();
            GameManager.ScreenManager.addScreen(techDemoScreen);
            GameManager.ScreenManager.addScreen(new MiniMapScreen(GameManager));
            
            var techDemoDisplay = new OnScreenDisplay(GameManager) { ToggleTechDemo = true };
            GameManager.ScreenManager.addScreen(techDemoDisplay);
        }
        
        if (ButtonCollection["quit"].isClicked())
        {
            GameManager.Game.Exit();
        }

        if (ButtonCollection["tuto"].isClicked())
        {
            GameManager.ScreenManager.addScreen(new TutorialScreen(GameManager));
        }
    }
}
