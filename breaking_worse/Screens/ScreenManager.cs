using System;
using System.Collections.Generic;
using System.Linq;
using breaking_worse.Screens.ScreenTypes.InGameScreens;
using breaking_worse.Screens.ScreenTypes.MenuScreens;
using Microsoft.Xna.Framework;

namespace breaking_worse.Screens;

public class ScreenManager(GameManager gameManager)
{
    /// <summary>
    /// creates a stack with all active screens
    /// methods to add or remove a screen from the stack
    /// </summary>

    private readonly List<AScreen> _screenStack = [];
    private readonly HashSet<AScreen> _toBeRemoved = [];
    public InGameScreen InGameScreen { get; set; } = null;
    public TechDemo TechDemo { get; set; }
    public void initialize()
    {
        addScreen(new BackgroundScreen(gameManager, gameManager.AssetManager.Images["menuImage"]));
        addScreen(new MainMenuScreen(gameManager));
        foreach (var screen in _screenStack)
            screen.loadContent();
    }

    public void addScreen(AScreen screen)
    {
        _screenStack.Add(screen);
    }
    
    /// <summary>
    /// removes given amount of screens from stack relative to given Screen
    /// direction: true -> upwards, false -> downwards (default: upwards)
    /// self: true -> remove parameter screen itself (default: true)
    /// amount: -1 -> remove all screens (default: 0)
    ///
    /// common use cases:
    /// - remove just screen itself -> removeFromStack(this)
    /// - remove all above including self -> removeFromStack(this, -1)
    /// - remove self and one downwards -> removeFromStack(this, 1, true, false)
    /// </summary>
    public void removeFromStack(AScreen screen, int amount = 0, bool self = true, bool direction = true)
    {
        var index = _screenStack.IndexOf(screen);
        if (index == -1) return;

        if (amount == -1) amount = _screenStack.Count;

        for (var i = 1; i <= amount; i++)
        {
            var targetIndex = direction ? index + i : index - i;
            if (targetIndex >= 0 && targetIndex < _screenStack.Count)
                _toBeRemoved.Add(_screenStack[targetIndex]);
        }

        if (self) _toBeRemoved.Add(screen);
    }

    public void update(GameTime gameTime)
    {
        // updates current screen and checks if the lower screens should be updated, if yes updates them
        for (var i = _screenStack.Count - 1; i >= 0; i--)
        {
            _screenStack[i].update(gameTime);

            //checking if DialogueScreen is on top of InventoryScreen or vice versa, not updating the lower Screen
            if(_screenStack[i].GetType() == typeof(InventoryScreen) && _screenStack[i-1].GetType() == typeof(DialogueScreen))
                i -= 1;
            if(_screenStack[i].GetType() == typeof(DialogueScreen) && _screenStack[i-1].GetType() == typeof(InventoryScreen))
                i -= 1;
            
            if (!_screenStack[i].UpdateLower)
                break;
        }

        //removes all screens in the removeSet
        foreach (var screen in _toBeRemoved)
            _screenStack.Remove(screen);
        
        _toBeRemoved.Clear();
    }

    public void renderInGameScreen(GameTime gameTime)
    {
        InGameScreen?.render(gameTime);
    }
    

    public void draw(GameTime gameTime)
    {
        var lowestDrawn = 0;
        for (var i = _screenStack.Count - 1; i >= 0; i--)
        {
            if (_screenStack[i].DrawLower) continue;
            lowestDrawn = i;
            break;
        }

        for (var i = lowestDrawn; i <= _screenStack.Count - 1; i++)
        { 
            if (i != _screenStack.Count - 1 && _screenStack[i].DontDrawWhenNotOnTopOfStack) continue;
            _screenStack[i].draw(gameTime);
        }
    }
    
    public bool containsScreen(AScreen screen)
    {
        return _screenStack.Contains(screen);
    }

    public void changeScreenResolution()
    {
        foreach (var screen in _screenStack)
        {
            screen.changeResolution();
        }
    }
}
