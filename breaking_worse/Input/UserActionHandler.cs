using System;
using System.Collections.Generic;
using breaking_worse.Input.Enums;
using breaking_worse.Objects.Player;
using Microsoft.Xna.Framework;

namespace breaking_worse.Input;

public class UserActionHandler(GameManager gameManager)
{
    /// <summary>
    /// class to offer an interface that connects MonoGame inputs to IKeys
    /// </summary>

    private readonly Dictionary<IKey, bool> _userActionPressAndReleaseStates = new();

    public void update()
    {
        checkForUserActionsPressAndRelease();
    }

    private void checkForUserActionsPressAndRelease()
    {
        foreach (var inputDevice in gameManager.InputHandler.InputDevices.AvailableInputDevices)
            foreach (UserAction userAction in Enum.GetValues(typeof(UserAction)))
            {
                var key = gameManager.SettingsManager.Controls[(inputDevice, userAction)];
                _userActionPressAndReleaseStates[key] = gameManager.InputHandler.isPressed(key, PressType.PressAndRelease);
            }
    }
    
    public bool isUserAction(PlayerCharacterId playerCharacterId, UserAction userAction, PressType pressType)
    {
        var inputDevice = gameManager.InputHandler.InputDevices.getInputDevice(playerCharacterId);
        var key = gameManager.SettingsManager.Controls[(inputDevice, userAction)];
        return pressType == PressType.PressAndRelease ? _userActionPressAndReleaseStates[key] : gameManager.InputHandler.isPressed(key, pressType);
    }
    
    /// <summary>
    /// calculates direction the player should move based on input combinations
    /// </summary>
    public Vector2 getMovement(PlayerCharacterId playerCharacterId)
    { 
        var inputDevice = gameManager.InputHandler.InputDevices.getInputDevice(playerCharacterId);
        var correction = new Vector2(1, -1);

        return inputDevice switch
        {
            InputDevice.GamePadOne => gameManager.InputHandler.getLeftThumbStick(PlayerIndex.One) * correction,
            InputDevice.GamePadTwo => gameManager.InputHandler.getLeftThumbStick(PlayerIndex.Two) * correction,
            InputDevice.KeyboardLeft => getDirection(InputDevice.KeyboardLeft),
            InputDevice.KeyboardRight => getDirection(InputDevice.KeyboardRight),
            _ => Vector2.Zero
        };
    }

    private Vector2 getDirection(InputDevice inputDevice)
    {
        const int degree180 = 180;
        const int degree45 = 45;
        const int degree135 = 135;
        
        var upKey = gameManager.SettingsManager.Controls[(inputDevice, UserAction.Up)];
        var downKey = gameManager.SettingsManager.Controls[(inputDevice, UserAction.Down)];
        var leftKey = gameManager.SettingsManager.Controls[(inputDevice, UserAction.Left)];
        var rightKey = gameManager.SettingsManager.Controls[(inputDevice, UserAction.Right)];
        
        if (isKeyCombination(upKey, rightKey))
            return new Vector2((float)Math.Cos(-degree45 * Math.PI / degree180), (float)Math.Sin(-degree45 * Math.PI / degree180));
        if (isKeyCombination(rightKey, downKey))
            return new Vector2((float)Math.Cos(degree45 * Math.PI / degree180), (float)Math.Sin(degree45 * Math.PI / degree180));
        if (isKeyCombination(downKey, leftKey))
            return new Vector2((float)Math.Cos(degree135 * Math.PI / degree180), (float)Math.Sin(degree135 * Math.PI / degree180));
        if (isKeyCombination(leftKey, upKey))
            return new Vector2((float)Math.Cos(-degree135 * Math.PI / degree180), (float)Math.Sin(-degree135 * Math.PI / degree180));
        if (isKeyCombination(upKey))
            return new Vector2(0, -1);
        if (isKeyCombination(leftKey))
            return new Vector2(-1, 0);
        if (isKeyCombination(downKey))
            return new Vector2(0, 1);
        if (isKeyCombination(rightKey))
            return new Vector2(1, 0);
        return Vector2.Zero;
    }
    
    private bool isKeyCombination(IKey key1, IKey key2 = null)
    {
        if (key2 != null)
        {
            return gameManager.InputHandler.isPressed(key1, PressType.HoldWithoutRelease) &&
                   gameManager.InputHandler.isPressed(key2, PressType.HoldWithoutRelease);
        }
        return gameManager.InputHandler.isPressed(key1, PressType.HoldWithoutRelease);
    }

    public bool isLeftMouseButtonClicked()
    {
        return gameManager.InputHandler.isPressed(gameManager.SettingsManager.LeftMouseButton, PressType.PressAndRelease);
    }
    
    public bool isLeftMouseButtonPressed()
    {
        return gameManager.InputHandler.isPressed(gameManager.SettingsManager.LeftMouseButton, PressType.HoldWithoutRelease);
    }
}
