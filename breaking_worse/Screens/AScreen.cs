using Microsoft.Xna.Framework;

namespace breaking_worse.Screens;

public abstract class AScreen(GameManager gameManager, bool updateLower, bool drawLower)
{
    protected readonly GameManager GameManager = gameManager;

    
    protected float xScale { get; set; } = gameManager.GraphicsDeviceManager.GraphicsDevice.Viewport.Width / 1920f;
    protected float yScale { get; set; } = gameManager.GraphicsDeviceManager.GraphicsDevice.Viewport.Height / 1080f;
    protected float Scale { get; set; } = gameManager.GraphicsDeviceManager.GraphicsDevice.Viewport.Width/ 1920f < gameManager.GraphicsDeviceManager.GraphicsDevice.Viewport.Height / 1080f
        ? gameManager.GraphicsDeviceManager.GraphicsDevice.Viewport.Width / 1920f
        : gameManager.GraphicsDeviceManager.GraphicsDevice.Viewport.Height / 1080f;

    public bool UpdateLower { get; set; } = updateLower;
    public bool DrawLower { get; set; } = drawLower;
    public bool DontDrawWhenNotOnTopOfStack { get; protected init; }

    public abstract void update(GameTime gameTime);
    public abstract void draw(GameTime gameTime);
    public abstract void loadContent();

    public abstract void changeResolution();
}
