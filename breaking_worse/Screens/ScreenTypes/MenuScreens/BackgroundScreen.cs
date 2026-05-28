using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace breaking_worse.Screens.ScreenTypes.MenuScreens;

public class BackgroundScreen(GameManager gameManager, Texture2D backgroundTexture, float opacity = 1f) : AScreen(gameManager, false, false)
{
    public override void update(GameTime gameTime)
    {
        // nothing to update
    }

    public override void draw(GameTime gameTime)
    {
        GameManager.SpriteBatch.Draw(BackGroundTexture, new Rectangle(0, 0, GameManager.SettingsManager.Resolution.Width, GameManager.SettingsManager.Resolution.Height), Color.White * opacity);
    }

    public override void loadContent()
    {
        // nothing to load
    }

    public override void changeResolution() { }

    public Texture2D BackGroundTexture { get; set; } = backgroundTexture;
}
