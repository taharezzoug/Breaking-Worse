using Microsoft.Xna.Framework;

namespace breaking_worse.Objects.Hud;

public abstract class HudElement(GameManager gameManager, Vector2 position)
{
    protected readonly GameManager GameManager = gameManager;
    protected Vector2 Position = position;
    
    public abstract void draw();
    public abstract void update();
}
