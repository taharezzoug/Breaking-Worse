using System.Linq;
using breaking_worse.Objects.Player;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace breaking_worse.Screens.ScreenTypes.SettingsScreens;

public class ControlSelectionScreen : AMenuScreen
{
    private bool _conflictDetected;
    private readonly string _warning = "Walt and Jesse need different input devices!";
    
    public ControlSelectionScreen(GameManager gameManager) : base(gameManager)
    {
        gameManager.InputHandler.InputDevices.checkForInputDevices();
        ButtonCollection.setButtonSize(400);
        createMenuElements();
    }

    protected sealed override void createMenuElements()
    {
        createHeadline("Control Selection", Color.White);
        
        ButtonCollection.addButton("inputWalt" , $"Walt: {GameManager.InputHandler.InputDevices.getInputDevice(PlayerCharacterId.Walt)}", 0.6f);
        ButtonCollection.addButton("inputJesse" , $"Jesse: {GameManager.InputHandler.InputDevices.getInputDevice(PlayerCharacterId.Jesse)}", 0.6f);
        
        foreach (var inputDevice in GameManager.InputHandler.InputDevices.AvailableInputDevices)
            ButtonCollection.addButton($"{inputDevice}", $"{inputDevice}", 0.6f);
        
        ButtonCollection.addButton("return", "return", 0.6f);
    }
    
    public override void update(GameTime gameTime)
    {
        base.update(gameTime);
        
        if (ButtonCollection["inputWalt"].isClicked())
            GameManager.ScreenManager.addScreen(new DeviceSelectionScreen(GameManager, PlayerCharacterId.Walt));
        if (ButtonCollection["inputJesse"].isClicked())
            GameManager.ScreenManager.addScreen(new DeviceSelectionScreen(GameManager, PlayerCharacterId.Jesse));
        
        foreach (var inputDevice in GameManager.InputHandler.InputDevices.AvailableInputDevices.Where(inputDevice => ButtonCollection[$"{inputDevice}"].isClicked()))
            GameManager.ScreenManager.addScreen(new ControlAssignmentScreen(GameManager, inputDevice));
        
        if (ButtonCollection["return"].isClicked())
            if (GameManager.InputHandler.InputDevices.getInputDevice(PlayerCharacterId.Walt) !=
                GameManager.InputHandler.InputDevices.getInputDevice(PlayerCharacterId.Jesse))
            {
                _conflictDetected = false;
                GameManager.ScreenManager.removeFromStack(this);
            }
            else _conflictDetected = true;
    }
    
    public override void draw(GameTime gameTime)
    {
        base.draw(gameTime);

        if (!_conflictDetected) return;
        
        var font = GameManager.AssetManager.Fonts["gabriele"];
        var textSize = font.MeasureString(_warning);
        
        var centeredX = GameManager.SettingsManager.Resolution.Width / 2f - (textSize.X * Scale);
        var position = new Vector2(centeredX, GameManager.SettingsManager.Resolution.Height * (5f/6f));
        
        GameManager.SpriteBatch.DrawString(font, _warning, position, Color.Red, 0f, Vector2.Zero, Scale*2f, SpriteEffects.None, 0f);
    }
}
