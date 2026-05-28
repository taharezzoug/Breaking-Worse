using breaking_worse.Screens.ScreenTypes.InGameScreens;
using Microsoft.Xna.Framework;

namespace breaking_worse.Screens.ScreenTypes.MenuScreens;

public class LoadGameScreen : AMenuScreen
{
    /// <summary>
    /// Screen to Load saved Games
    /// Creates the layout for the screen objects (button, texBoxes, headline)
    /// Definition of actions if a button in the screen is clicked
    /// </summary>

    private GameManager _gameManager;
    public LoadGameScreen(GameManager gameManager) : base(gameManager)
    {
        _gameManager = gameManager;
        createMenuElements();
    }

    protected sealed override void createMenuElements()
    {
        createHeadline("load Gme");
        for (int i = 1; i <= 4; i++)
        {
            var gameState = _gameManager.SaveManager.readGameStateFromFile(i);
            if (gameState is not null)
                ButtonCollection.addButton($"game{i}", $"Load Game {i}");
            else
                ButtonCollection.addButton($"game{i}", "");
        }
        ButtonCollection.addButton("return", "return");
    }

    public override void update(GameTime gameTime)
    {
        base.update(gameTime);

        for (var i = 1; i <= 4; i++)
            if (ButtonCollection[$"game{i}"].isClicked())
                createInGameScreen(i);
        
        if (ButtonCollection["return"].isClicked())
            GameManager.ScreenManager.removeFromStack(this);
    }

    private void createInGameScreen(int id)
    {
        var gameState = GameManager.SaveManager.readGameStateFromFile(id);
        if (gameState == null) return;
        
        var inGameScreen = new InGameScreen(GameManager, id, gameState);
        GameManager.ScreenManager.removeFromStack(this);
        GameManager.ScreenManager.InGameScreen = inGameScreen;
        inGameScreen.initialize();
        GameManager.ScreenManager.addScreen(inGameScreen);
        GameManager.ScreenManager.addScreen(new OnScreenDisplay(GameManager));
        GameManager.ScreenManager.addScreen(new MiniMapScreen(GameManager));
    }
}
