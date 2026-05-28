using breaking_worse.Screens.Clickables;
using breaking_worse.State;
using Microsoft.Xna.Framework;

namespace breaking_worse.Screens.ScreenTypes.SettingsScreens;

public class GraphicsSettingsScreen : AMenuScreen
{
    /// <summary>
    /// Screen for Settings (all settings can be selected)
    /// Creates the layout for the screen objects (button, texBoxes, headline)
    /// Definition of actions if a button in the screen is clicked
    /// </summary>

    public GraphicsSettingsScreen(GameManager gameManager) : base(gameManager)
    {
        createMenuElements();
    }

    protected sealed override void createMenuElements()
    {
        createHeadline("Gme Settings", Color.White);

        ButtonCollection.addButton("720p", "720p");
        ButtonCollection.addButton("1080p", "1080p");
        ButtonCollection.addButton("1440p", "1440p");
        ButtonCollection.addButton("Fullscreen", "Fullscreen");
        
        ButtonCollection.addButton("save", "save");
        ButtonCollection.addButton("reset", "reset");
        ButtonCollection.addButton("return", "return");
    }
    
    public override void update(GameTime gameTime)
    {
        base.update(gameTime);

        var isFullScreen = GameManager.SettingsManager.Resolution.IsFullScreen;
        ButtonCollection["720p"].IsSelectedBySettings = !isFullScreen && GameManager.SettingsManager.Resolution.Height == 720;
        ButtonCollection["1080p"].IsSelectedBySettings = !isFullScreen && GameManager.SettingsManager.Resolution.Height == 1080;
        ButtonCollection["1440p"].IsSelectedBySettings =  !isFullScreen && GameManager.SettingsManager.Resolution.Height == 1440;
        ButtonCollection["Fullscreen"].IsSelectedBySettings = isFullScreen;

        if (ButtonCollection["720p"].isClicked())
        {
            GameManager.SettingsManager.isFullScreen(false);
            GameManager.SettingsManager.changeResolution(1280, 720);
            ButtonCollection["save"].HasUnsavedChanges = true;
        }

        if (ButtonCollection["1080p"].isClicked())
        {
            GameManager.SettingsManager.isFullScreen(false);
            GameManager.SettingsManager.changeResolution(1920, 1080);
            ButtonCollection["save"].HasUnsavedChanges = true;
        }

        if (ButtonCollection["1440p"].isClicked())
        {
            GameManager.SettingsManager.isFullScreen(false);
            GameManager.SettingsManager.changeResolution(2560, 1440);
            ButtonCollection["save"].HasUnsavedChanges = true;
        }

        if (ButtonCollection["Fullscreen"].isClicked())
        {
            GameManager.SettingsManager.isFullScreen(!GameManager.SettingsManager.Resolution.IsFullScreen);
            if(!GameManager.SettingsManager.Resolution.IsFullScreen)
                GameManager.SettingsManager.changeResolution(1920, 1080);
            ButtonCollection["save"].HasUnsavedChanges = true;
        }

        if (ButtonCollection["save"].isClicked())
        {
            GameManager.SaveManager.saveSettingsToFile(GameManager.SettingsManager.buildSettingsState());
            ButtonCollection["save"].HasUnsavedChanges = false;
        }
        
        if (ButtonCollection["reset"].isClicked())
        {
            GameManager.SettingsManager.loadSettings(GameManager.SettingsManager.createDefaultGraphicsSettings());
            GameManager.SettingsManager.changeResolution(1920, 1080);
            ButtonCollection["save"].HasUnsavedChanges = false;
            changeResolution();
        }


        if (ButtonCollection["return"].isClicked())
            GameManager.ScreenManager.removeFromStack(this);
    }
}
