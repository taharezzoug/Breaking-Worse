using Microsoft.Xna.Framework;

namespace breaking_worse.Screens.ScreenTypes.MenuScreens;

public class SaveScreen : AMenuScreen
{
    /// <summary>
    /// Screen to Load saved Games
    /// Creates the layout for the screen objects (button, texBoxes, headline)
    /// Definition of actions if a button in the screen is clicked
    /// </summary>

    private GameManager _gameManager;
    
    public SaveScreen(GameManager gameManager) : base(gameManager)
    {
        _gameManager = gameManager;
        createMenuElements();
    }

    protected sealed override void createMenuElements()
    {
        createHeadline("save Gme");
        for (int i = 1; i <= 4; i++)
        {
            var gameState = _gameManager.SaveManager.readGameStateFromFile(i);
            if (gameState is not null)
                ButtonCollection.addButton($"game{i}", $"Game {i}");
            else
                ButtonCollection.addButton($"game{i}", "");
        }
        ButtonCollection.addButton("return", "return");
    }
    
    public override void update(GameTime gameTime)
    {
        base.update(gameTime);
        
        for (var i = 1; i <= 4; i++)
        {
            if (ButtonCollection[$"game{i}"].isClicked())
            {
                saveInGameScreen(i);
                GameManager.ScreenManager.removeFromStack(this);
            }
        }
        
        if (ButtonCollection["return"].isClicked())
            GameManager.ScreenManager.removeFromStack(this);
    }

    private void saveInGameScreen(int id)
    {
        GameManager.ScreenManager.InGameScreen.GameStateId = id;
        GameManager.ScreenManager.InGameScreen.saveGameState();
    }
}
