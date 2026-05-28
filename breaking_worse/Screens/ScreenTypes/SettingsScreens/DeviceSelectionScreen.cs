using System.Linq;
using breaking_worse.Objects.Player;
using Microsoft.Xna.Framework;

namespace breaking_worse.Screens.ScreenTypes.SettingsScreens;

public class DeviceSelectionScreen : AMenuScreen
{
    private readonly PlayerCharacterId _playerCharacterId;
    
    public DeviceSelectionScreen(GameManager gameManager, PlayerCharacterId playerCharacterId) : base(gameManager)
    {
        _playerCharacterId = playerCharacterId;
        createMenuElements();
    }

    protected sealed override void createMenuElements()
    {
        createHeadline("Device Selection", Color.White);
        
        foreach (var inputDevice in GameManager.InputHandler.InputDevices.AvailableInputDevices)
            ButtonCollection.addButton($"{inputDevice}", $"{inputDevice}");
        
        ButtonCollection.addButton("return", "return");
    }
    
    public override void update(GameTime gameTime)
    {
        base.update(gameTime);

        foreach (var inputDevice in GameManager.InputHandler.InputDevices.AvailableInputDevices.Where(
                     inputDevice => ButtonCollection[$"{inputDevice}"].isClicked()))
        {
            GameManager.InputHandler.InputDevices.setInputDevice(_playerCharacterId, inputDevice);
            GameManager.ScreenManager.removeFromStack(this, 1, true, false);
            GameManager.ScreenManager.addScreen(new ControlSelectionScreen(GameManager));
        }
        
        if (ButtonCollection["return"].isClicked())
            GameManager.ScreenManager.removeFromStack(this);
    }
}
