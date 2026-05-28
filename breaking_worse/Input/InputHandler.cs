using System.Collections.Generic;
using System.Linq;
using breaking_worse.Input.Enums;
using breaking_worse.input.KeyTypes;
using breaking_worse.Objects.Player;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace breaking_worse.Input;

public class InputHandler
{
    // duration a key needs to be pressed to count as held
    private const int HoldThresholdInMicroSeconds = 1000000;
    // tolerance for holding in microseconds
    private const int HoldToleranceInMicroSeconds = 50;
    
    // contains information about which playerCharacter is controlled by which input device
    public InputDevices InputDevices { get; private init; } = new();

    // maps key to tuple of (bool, double)
    // - bool stands for if key was pressed last update
    // - double stands for timeStamp when key was pressed
    private readonly Dictionary<IKey, (bool, double)> _keyStates = new();
    private GameTime _gameTime;

    public void update(GameTime gameTime)
    {
        _gameTime = gameTime;
    }
    
    /// <summary>
    /// Checks if given key is pressed according to pressType
    /// PressAndRelease:
    ///     return true one time per press/release cycle, instantly when pressed
    /// OnlyRelease:
    ///     return true one time per press/release cycle, when released
    /// PressHoldAndRelease:
    ///     return true one time per press/release cycle but only if key is held down for HoldThresholdInMicroSeconds
    /// HoldWithoutRelease:
    ///     return true every frame if the key is held down
    /// </summary>
    public bool isPressed(IKey key, PressType pressType)
    {
        var isCurrentlyPressed = key.isPressed();
        // check if key exists in _keyStates
        var hasKeyState = _keyStates.TryGetValue(key, out var lastKeyState);
        
        // return true if not pressed, OnlyRelease and wasPressedLastUpdate
        if (!isCurrentlyPressed)
        {
            var result = pressType == PressType.OnlyRelease && lastKeyState.Item1;
            _keyStates.Remove(key);
            return result;
        }
        
        // if key is pressed and pressType is HoldWithoutRelease, return true
        if (pressType == PressType.HoldWithoutRelease)
            return true;
        
        // get current timeStamp
        var nowInMicroseconds = _gameTime.TotalGameTime.TotalMicroseconds;

        if (hasKeyState)
        {
            // get time difference between now and when key was pressed
            var timeSpan = nowInMicroseconds - lastKeyState.Item2;

            // returns true and sets wasPressedLastUpdate to false if wasPressedLastUpdate is true and
            // - pressType is PressAndRelease
            // - pressType is HoldPressAndRelease and the button was held down for holdThreshold
            if (lastKeyState.Item1 && (pressType == PressType.PressAndRelease ||
                                       (pressType == PressType.HoldPressAndRelease &&
                                        timeSpan > HoldThresholdInMicroSeconds + HoldToleranceInMicroSeconds)))
            {
                _keyStates[key] = (false, lastKeyState.Item2);
                return true;
            }
        } else
            _keyStates[key] = (true, nowInMicroseconds);
        
        return false;
    }

    public KeyboardKey[] getPressedKeyboardKeys()
    {
        return Keyboard.GetState().GetPressedKeys().Select(key => new KeyboardKey(key)).ToArray();
    }
    
    public Point MousePosition => Mouse.GetState().Position;

    public int getScrollWheelValue(bool horizontal = false)
    {
        return horizontal ? Mouse.GetState().ScrollWheelValue : Mouse.GetState().HorizontalScrollWheelValue;
    }

    public Vector2 getLeftThumbStick(PlayerIndex playerIndex)
    {
        return GamePad.GetState(playerIndex).ThumbSticks.Left;
    }
    
    public Vector2 getRightThumbStick(PlayerIndex playerIndex)
    {
        return GamePad.GetState(playerIndex).ThumbSticks.Right;
    }

    public float getLeftTrigger(PlayerIndex playerIndex)
    {
        return GamePad.GetState(playerIndex).Triggers.Left;
    }
    
    public float getRightTrigger(PlayerIndex playerIndex)
    {
        return GamePad.GetState(playerIndex).Triggers.Right;
    }
}
