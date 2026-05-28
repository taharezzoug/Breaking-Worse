using breaking_worse.Objects.Player;
using breaking_worse.Screens.Clickables;
using Microsoft.Xna.Framework;

namespace breaking_worse.Screens.ScreenTypes.MenuScreens;

public class GameEndedScreen : AMenuScreen
{
    /// <summary>
    /// Screen for the end of the game, shows if you won or lost
    /// </summary>
    private TextBox _info;

    private bool _winning;

    public GameEndedScreen(GameManager gameManager, bool winning) : base(gameManager)
    {
        _winning = winning; 
        ButtonCollection = new ButtonCollection(GameManager, new Vector2(GameManager.SettingsManager.Resolution.Width / 2f, 200 * yScale), [PlayerCharacterId.Walt, PlayerCharacterId.Jesse]);

        createMenuElements();
    }

    public override void update(GameTime gameTime)
    {
        ButtonCollection.update();
        
        if (ButtonCollection["return"].isClicked())
        {
            GameManager.ScreenManager.removeFromStack(this, -1, true, false);
            GameManager.ScreenManager.addScreen(new BackgroundScreen(GameManager, GameManager.AssetManager.Images["menuImage"]));
            GameManager.ScreenManager.addScreen(new MainMenuScreen(GameManager));
        }
        if (ButtonCollection.isInCollection("continue") && ButtonCollection["continue"].isClicked())
        {
            GameManager.ScreenManager.InGameScreen.PlayerState.Timer = int.MaxValue;
            GameManager.ScreenManager.removeFromStack(this, 1, true, false);
        }
    }
    
    public override void draw(GameTime gameTime)
    {
        _info.draw();
        ButtonCollection.draw();
    }

    protected sealed override void createMenuElements()
    {
        createHeadline(_winning ? "You Won" : "Gme Over");
        
        if(_winning)
        {
            ButtonCollection.addButtonAtPosition("return", "return", new Vector2(GameManager.SettingsManager.Resolution.Width * (1f/4f), 200));
            ButtonCollection.addButtonAtPosition("continue", "continue", new Vector2(GameManager.SettingsManager.Resolution.Width * (3f / 4f), 200));
        }
        else
           ButtonCollection.addButton("return", "return");
        
        _info = new TextBox(GameManager, new Vector2(GameManager.SettingsManager.Resolution.Width / 2f, 500 * yScale),
            _winning ? "You have beaten cancer" : "You lost", GameManager.AssetManager.Fonts["font"],
            2.0f, Color.White);
    }
}
