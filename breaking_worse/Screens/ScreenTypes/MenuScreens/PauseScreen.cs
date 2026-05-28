using breaking_worse.Screens.ScreenTypes.SettingsScreens;
using Microsoft.Xna.Framework;

namespace breaking_worse.Screens.ScreenTypes.MenuScreens;

public class PauseScreen : AMenuScreen
{
    /// <summary>
    /// Screen for the main menu (navigates to other menus)
    /// creates the layout for the screen objects (button, texBoxes, headline)
    /// Definition of actions if a button in the screen is clicked
    /// </summary>

    public PauseScreen(GameManager gameManager) : base(gameManager)
    {
        createMenuElements();
        GameManager.SoundManager.playMusic("MusicTemplate_Menu");
    }

    protected sealed override void createMenuElements()
    {
        createHeadline("Pause");
        ButtonCollection.addButton("continue", "Continue");
        if(GameManager.ScreenManager.InGameScreen.TechDemo == null) ButtonCollection.addButton("save", "Save");
        ButtonCollection.addButton("statistics", "Statistics");
        ButtonCollection.addButton("achievements", "Achievements");
        ButtonCollection.addButton("settings", "Settings");
        ButtonCollection.addButton("mainMenu", "Main Menu");
    }

    public override void update(GameTime gameTime)
    {
        base.update(gameTime);
        
        if (ButtonCollection["continue"].isClicked())
        {
            GameManager.SoundManager.playMusic("MusicTemplate_InGame");
            GameManager.ScreenManager.removeFromStack(this, 1, true, false);
        }

        if (GameManager.ScreenManager.InGameScreen.TechDemo == null && ButtonCollection["save"].isClicked())
        {
            GameManager.ScreenManager.addScreen(new SaveScreen(GameManager));
        }

        if (ButtonCollection["statistics"].isClicked())
        {
            GameManager.ScreenManager.addScreen(new StatisticsScreen(GameManager, false));
        }

        if (ButtonCollection["achievements"].isClicked())
        {
            GameManager.ScreenManager.addScreen(new AchievementsScreen(GameManager));
        }

        if (ButtonCollection["settings"].isClicked())
        {
            GameManager.ScreenManager.addScreen(new BackgroundScreen(GameManager, GameManager.AssetManager.Images["settingsImage"]));
            GameManager.ScreenManager.addScreen(new SettingsScreen(GameManager, false));
        }
        
        if (ButtonCollection["mainMenu"].isClicked())
        {
            // removes InGameScreen and everything above from stack
            GameManager.ScreenManager.removeFromStack(GameManager.ScreenManager.InGameScreen, -1);
            GameManager.ScreenManager.InGameScreen = null;
        }
    }
}
