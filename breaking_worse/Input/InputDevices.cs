using System;
using System.Collections.Generic;
using breaking_worse.Input.Enums;
using breaking_worse.Objects.Player;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace breaking_worse.Input;

public class InputDevices
{
    /// <summary>
    /// struct to store which input devices are currently in use
    /// </summary>

    public List<InputDevice> AvailableInputDevices { get; private set; }
    
    private InputDevice _waltDevice;
    private InputDevice _jesseDevice;

    public InputDevices()
    {
        checkForInputDevices();
        setDefaultInputDevices();
    }

    // get current input device of given player
    public InputDevice getInputDevice(PlayerCharacterId playerCharacterId)
    {
        return playerCharacterId switch
        {
            PlayerCharacterId.Walt => _waltDevice,
            PlayerCharacterId.Jesse => _jesseDevice,
            _ => throw new ArgumentOutOfRangeException(nameof(playerCharacterId), playerCharacterId, null)
        };
    }

    // set input device of given player
    public void setInputDevice(PlayerCharacterId playerCharacterId, InputDevice inputDevice)
    {
        if (!AvailableInputDevices.Contains(inputDevice))
            return;
        
        switch (playerCharacterId)
        {
            case PlayerCharacterId.Walt:
                _waltDevice = inputDevice;
                break;
            case PlayerCharacterId.Jesse:
                _jesseDevice = inputDevice;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(playerCharacterId), playerCharacterId, null);
        }
    }
    
    /// <summary>
    /// check which input devices are connected
    /// </summary>
    public void checkForInputDevices()
    {
        AvailableInputDevices = [InputDevice.KeyboardLeft, InputDevice.KeyboardRight];
        if (GamePad.GetState(PlayerIndex.One).IsConnected)
            AvailableInputDevices.Add(InputDevice.GamePadOne);
        if (GamePad.GetState(PlayerIndex.Two).IsConnected) 
            AvailableInputDevices.Add(InputDevice.GamePadTwo);
    }
    
    /// <summary>
    /// based on available input devices, decide which to use
    /// </summary>
    private void setDefaultInputDevices()
    {
        switch (AvailableInputDevices.Contains(InputDevice.GamePadOne))
        {
            case true when AvailableInputDevices.Contains(InputDevice.GamePadTwo):
                setInputDevice(PlayerCharacterId.Walt, InputDevice.GamePadOne);
                setInputDevice(PlayerCharacterId.Jesse, InputDevice.GamePadTwo);
                break;
            case true:
                setInputDevice(PlayerCharacterId.Walt, InputDevice.GamePadOne);
                setInputDevice(PlayerCharacterId.Jesse, InputDevice.KeyboardLeft);
                break;
            default:
                setInputDevice(PlayerCharacterId.Walt, InputDevice.KeyboardLeft);
                setInputDevice(PlayerCharacterId.Jesse, InputDevice.KeyboardRight);
                break;
        }
    }
}
