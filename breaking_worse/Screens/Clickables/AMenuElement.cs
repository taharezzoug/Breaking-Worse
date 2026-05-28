using Microsoft.Xna.Framework;

namespace breaking_worse.Screens.Clickables;

public abstract class AMenuElement(GameManager gameManager, ButtonCollection buttonCollection)
{
    protected readonly GameManager GameManager = gameManager;
    
    // buttonCollection this MenuElement is part of
    protected readonly ButtonCollection ButtonCollection = buttonCollection;
    
    public bool IsSelectedBySettings {get; set;}
    public bool IsSelectedByButtonCollection { get; set; }
    public bool HasUnsavedChanges { get; set; } = false;
    public Rectangle HitBox;
    public abstract string Text { get; set; }
    
    public bool IsUnlocked { get; set; }

    public abstract void draw();
    public abstract bool isClicked();
    public abstract void updateShade();
}
