using System;
using breaking_worse.Objects.Player;
using breaking_worse.Screens.Clickables;
using Microsoft.Xna.Framework;

namespace breaking_worse.Screens;

public abstract class AMenuScreen : AScreen
{
    protected ButtonCollection ButtonCollection;
    private TextBox _headline;

    protected AMenuScreen(GameManager gameManager) : base(gameManager, false, true)
    {
        ButtonCollection = new ButtonCollection(GameManager, new Vector2(GameManager.SettingsManager.Resolution.Width / 2f, 200), [PlayerCharacterId.Walt, PlayerCharacterId.Jesse]);
        DontDrawWhenNotOnTopOfStack = true;
    }
    
    protected void createHeadline(string text, Color textColor = default)
    {   
        _headline = new TextBox(GameManager, new Vector2(GameManager.SettingsManager.Resolution.Width / 2f, 80 * yScale), text, GameManager.AssetManager.Fonts["font"], 1.5f, textColor == default ? Color.Black : textColor);
    }

    public override void update(GameTime gameTime)
    {
        ButtonCollection.update();
    }

    public override void draw(GameTime gameTime)
    {
        ButtonCollection.draw();
        _headline.draw();
    }

    public override void loadContent()
    {
        // nothing to load
    }

    protected abstract void createMenuElements();
    
    public override void changeResolution()
    {
        xScale = GameManager.GraphicsDeviceManager.GraphicsDevice.Viewport.Width / 1920f;
        yScale = GameManager.GraphicsDeviceManager.GraphicsDevice.Viewport.Height / 1080f;
        Scale = Math.Min(xScale, yScale);        
        ButtonCollection = new ButtonCollection(GameManager, new Vector2(GameManager.SettingsManager.Resolution.Width / 2f, 200), [PlayerCharacterId.Walt, PlayerCharacterId.Jesse]);
        createMenuElements(); 
    }
}
