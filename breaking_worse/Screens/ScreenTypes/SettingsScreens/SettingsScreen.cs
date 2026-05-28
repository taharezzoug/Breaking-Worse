using System;
using breaking_worse.State;
using Microsoft.Xna.Framework;

namespace breaking_worse.Screens.ScreenTypes.SettingsScreens;

public class SettingsScreen : AMenuScreen
{
    /// <summary>
    /// Screen to choose the individual settings screens from
    /// </summary>
    private readonly bool _fromMainMenu;
    public SettingsScreen(GameManager gameManager, bool fromMainMenu) : base(gameManager)
    {
        _fromMainMenu = fromMainMenu;
        createMenuElements();
    }

    protected sealed override void createMenuElements()
    {
        createHeadline("Settings", Color.White);
        ButtonCollection.addButton("sound", "Sound");
        if (_fromMainMenu)
            ButtonCollection.addButton("game", "Game");
        ButtonCollection.addButton("controls", "Controls");
        ButtonCollection.addButton("graphics", "Graphics");
        ButtonCollection.addButton("reset", "reset");
        ButtonCollection.addButton("return", "return");
    }
    
    public override void update(GameTime gameTime)
    {
        base.update(gameTime);
        if (ButtonCollection.isInCollection("game") && ButtonCollection["game"].isClicked())
            GameManager.ScreenManager.addScreen(new GameSettingsScreen(GameManager));

        if (ButtonCollection["controls"].isClicked())
            GameManager.ScreenManager.addScreen(new ControlSelectionScreen(GameManager));
        
        if (ButtonCollection["graphics"].isClicked())
            GameManager.ScreenManager.addScreen(new GraphicsSettingsScreen(GameManager));
        
        if (ButtonCollection["sound"].isClicked())
            GameManager.ScreenManager.addScreen(new SoundSettingsScreen(GameManager));
        
        if (ButtonCollection["reset"].isClicked())
        {
            GameManager.SettingsManager.loadSettings(SettingsManager.createDefaultSettingsState());
            changeResolution();
        }
        
        if (ButtonCollection["return"].isClicked())
            GameManager.ScreenManager.removeFromStack(this, 1, true, false);
    }
}
