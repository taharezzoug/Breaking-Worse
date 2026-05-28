using System;
using System.Collections.Generic;
using breaking_worse.Input.Enums;
using breaking_worse.Objects.Player;
using breaking_worse.State.Enums;
using Microsoft.Xna.Framework;

namespace breaking_worse.Objects.Interaction;

public class MiniGame
{
    private readonly GameManager _gameManager;
    private bool _isPlaying;
    public bool IsCheating;
    private const double MiniGameDuration = 4000;
    private double _miniGameRoundWaitTime;
    public readonly int TotalGameRounds;
    public int CurrentCorrectHits;
    private int _remainingMistakes;
    private double _gameTime;
    private int _roundCount;
    private bool _gameWon;
    private bool _gameOver;
    
    private Difficulty _difficulty;

    private bool _jesseHit;
    private bool _waltHit;
    
    private UserAction _currentKeyWalt;
    private readonly List<UserAction> _keysWalt = [];
    private UserAction _currentKeyJesse;
    private readonly List<UserAction> _keysJesse = [];

    private readonly List<UserAction> _keyLookupList;
    
    
    public MiniGame(GameManager gameManager, bool isCheating = false)
    {
        _gameManager = gameManager;
        _difficulty = gameManager.SettingsManager.Difficulty;
        _isPlaying = false;
        IsCheating = isCheating;
        _keyLookupList = [UserAction.Down, UserAction.Up, UserAction.Left, UserAction.Right];
        _remainingMistakes = 4 - (int)_difficulty;
        _roundCount = TotalGameRounds = 10;
        generateKeySet();
        _currentKeyWalt = _keysWalt[_roundCount - 1];
        _currentKeyJesse = _keysJesse[_roundCount - 1];
        _miniGameRoundWaitTime = 500;
    }

    private void generateKeySet()
    {
        for (int i = 0; i < _roundCount; i++)
        {
            _keysJesse.Add(_keyLookupList[new Random().Next(0, _keyLookupList.Count)]);
            _keysWalt.Add(_keyLookupList[new Random().Next(0, _keyLookupList.Count)]);
        }
    }
    
    private void roundOver()
    {
        _roundCount--;
        if (_roundCount <= 0)
            return;
        _currentKeyWalt = _keysWalt[_roundCount - 1];
        _currentKeyJesse = _keysJesse[_roundCount - 1];
        _jesseHit = _waltHit = false;
        _gameTime = 0;
        _miniGameRoundWaitTime = 500;
    }
    
    private void roundWon()
    {
        CurrentCorrectHits++;
        roundOver();
    }

    private void roundLost()
    {
        roundOver();
        _remainingMistakes--;
    }
    
    public void play()
    {
        _gameWon = false;
        _gameOver = false;
        _isPlaying = true;
        _gameTime = 0;
    }

    public void update(GameTime gameTime)
    {
        if (IsCheating)
        {
            _gameWon = true;
            _gameOver = true;
            stop();
            return;
        }
        if (_miniGameRoundWaitTime > 0)
        {
            _miniGameRoundWaitTime -= gameTime.ElapsedGameTime.TotalMilliseconds;
            return;
        }
        _roundCount = _remainingMistakes <= 0 ? 0 : _roundCount;
        if (_roundCount <= 0)
        {
            if (_remainingMistakes > 0)
            {
                _gameWon = true;
            }
            stop();
            _gameOver = true;
            return;
        }

        if (_jesseHit && _waltHit)
        {
            roundWon();
            return;
        }
        
        _gameTime += gameTime.ElapsedGameTime.Milliseconds * (0.5f + 0.5 * (int)_difficulty);
        
        foreach (var key in _keyLookupList)
        {
            if (_gameManager.UserActionHandler.isUserAction(PlayerCharacterId.Walt, key, PressType.PressAndRelease))
            {
                if (key == _currentKeyWalt)
                {
                    _waltHit = true;
                }
                else
                {
                    _waltHit = false;
                    roundLost();
                }
            }
        }
        foreach (var key in _keyLookupList)
        {
            if (_gameManager.UserActionHandler.isUserAction(PlayerCharacterId.Jesse, key, PressType.PressAndRelease))
            {
                if (key == _currentKeyJesse)
                {
                    _jesseHit = true;
                }
                else
                {
                    _jesseHit = false;
                    roundLost();
                }
            }
        }
        
        if (_gameTime >= MiniGameDuration)
        {
            roundLost();
        }
    }
    
    public bool hasJesseHit() => _jesseHit;
    
    public bool hasWaltHit() => _waltHit;
    
    private void stop()
    {
        _isPlaying = false;
    }

    public int remainingTime()
    {
        return (int)MiniGameDuration - (int)_gameTime;
    }

    public UserAction currentKeyWalt()
    {
        return _currentKeyWalt;
    }

    public UserAction currentKeyJesse()
    {
        return _currentKeyJesse;
    }

    public List<UserAction> nextKeyWalt()
    {
        return _keysWalt;
    }

    public List<UserAction> nextKeyJesse()
    {
        return _keysJesse;
    }

    public int roundCount() => _roundCount;
    
    public int remainingMistakes() => _remainingMistakes;
    
    public bool isPlaying() => _isPlaying;

    public bool gameWon()
    {
        return _gameWon;
    }
    
    public bool gameOver() => _gameOver;
}