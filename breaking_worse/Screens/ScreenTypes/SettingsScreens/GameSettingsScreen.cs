using breaking_worse.State;
using breaking_worse.State.Enums;
using Microsoft.Xna.Framework;

namespace breaking_worse.Screens.ScreenTypes.SettingsScreens;

public class GameSettingsScreen : AMenuScreen
{
    /// <summary>
    /// Screen for Settings (all settings can be selected)
    /// Creates the layout for the screen objects (button, texBoxes, headline)
    /// Definition of actions if a button in the screen is clicked
    /// </summary>

    public GameSettingsScreen(GameManager gameManager) : base(gameManager)
    {
        createMenuElements();
    }

    protected sealed override void createMenuElements()
    {
        createHeadline("Game Settings", Color.White);
        
        ButtonCollection.addButton("easy", "easy");
        ButtonCollection.addButton("normal", "normal");
        ButtonCollection.addButton("hard", "hard");
        
        ButtonCollection.addButton("save", "save");
        ButtonCollection.addButton("reset", "reset");
        ButtonCollection.addButton("return", "return");
    }

    public override void update(GameTime gameTime)
    {
        base.update(gameTime);
        
        ButtonCollection["easy"].IsSelectedBySettings = (GameManager.SettingsManager.Difficulty == Difficulty.Easy);
        ButtonCollection["normal"].IsSelectedBySettings = (GameManager.SettingsManager.Difficulty == Difficulty.Normal);
        ButtonCollection["hard"].IsSelectedBySettings = (GameManager.SettingsManager.Difficulty == Difficulty.Hard);
        
        if (ButtonCollection["easy"].isClicked())
        {
            GameManager.SettingsManager.Difficulty = Difficulty.Easy;
            ButtonCollection["save"].HasUnsavedChanges = true;
        }

        
        if (ButtonCollection["normal"].isClicked())
        {
            GameManager.SettingsManager.Difficulty = Difficulty.Normal;
            ButtonCollection["save"].HasUnsavedChanges = true;
        }

        
        if (ButtonCollection["hard"].isClicked())
        {
            GameManager.SettingsManager.Difficulty = Difficulty.Hard;
            ButtonCollection["save"].HasUnsavedChanges = true;
        }


        if (ButtonCollection["save"].isClicked())
        {
            GameManager.SaveManager.saveSettingsToFile(GameManager.SettingsManager.buildSettingsState());
            ButtonCollection["save"].HasUnsavedChanges = false;
        }

        
        if (ButtonCollection["reset"].isClicked())
        {
            GameManager.SettingsManager.loadSettings(GameManager.SettingsManager.createDefaultDifficultySettings());
            ButtonCollection["save"].HasUnsavedChanges = false;
            changeResolution();
        }

        
        if (ButtonCollection["return"].isClicked())
            GameManager.ScreenManager.removeFromStack(this);
    }
}
