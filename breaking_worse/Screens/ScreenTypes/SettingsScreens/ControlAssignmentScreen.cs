using System;
using System.Collections.Generic;
using breaking_worse.Input;
using breaking_worse.Input.Enums;
using breaking_worse.input.KeyTypes;
using breaking_worse.Objects.Player;
using breaking_worse.Screens.Clickables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace breaking_worse.Screens.ScreenTypes.SettingsScreens;

public class ControlAssignmentScreen : AMenuScreen
{
    /// <summary>
    /// Screen for the control settings for the players, if bool walt is true,
    /// controls for player one are assigned, else for player two.
    /// </summary>
    private readonly InputDevice _inputDevice;
    
    private int _buttonToBeAssigned = -1;
    private int _previousButtonToBeAssigned = -1;
    private bool _conflictDetected;
    private readonly string _warning = "Cannot save, return nor process if a key is already assigned by Walt or Jesse";
    private readonly Dictionary<UserAction, string> _actionDisplayNames = new()
    {
        { UserAction.Interact, "Interact" },
        { UserAction.Inventory, "Inventory" },
        { UserAction.PauseMenu, "Pause" },
        { UserAction.Up, "Up" },
        { UserAction.Down, "Down" },
        { UserAction.Left, "Left" },
        { UserAction.Right, "Right" },
        { UserAction.Execute, "Shoot/Punch" },
        { UserAction.SwitchWeapon, "Switch Weapon" }
    };
    
    private IKey _lastProcessedKey;


    public ControlAssignmentScreen(GameManager gameManager, InputDevice inputDevice) : base(gameManager)
    {
        _inputDevice = inputDevice;
        ButtonCollection = new ButtonCollection(GameManager, new Vector2(GameManager.SettingsManager.Resolution.Width * (1f/6f), 200), [PlayerCharacterId.Walt, PlayerCharacterId.Jesse], 500, 70);
        createMenuElements();
    }

    protected sealed override void createMenuElements()
    {
        createHeadline($"Control assignment: {_inputDevice}", Color.White);
        
        ButtonCollection.addButton("Interact", $"Interact: {GameManager.SettingsManager.Controls[(_inputDevice, UserAction.Interact)]}");
        ButtonCollection.addButton("Inventory", $"Inventory: {GameManager.SettingsManager.Controls[(_inputDevice, UserAction.Inventory)]}");
        ButtonCollection.addButton("PauseMenu", $"Pause: {GameManager.SettingsManager.Controls[(_inputDevice, UserAction.PauseMenu)]}");
        
        ButtonCollection.addButtonAtPosition("save", "save", new Vector2(GameManager.SettingsManager.Resolution.Width * (1f/6f), 600));
        ButtonCollection.addButton("reset", "reset");
        ButtonCollection.addButton("return", "return");
        
        ButtonCollection.addButtonArrow("Up", $"{GameManager.SettingsManager.Controls[(_inputDevice, UserAction.Up)]}", new Vector2(1275, 300),  0);
        ButtonCollection.addButtonArrow("Down", $"{GameManager.SettingsManager.Controls[(_inputDevice, UserAction.Down)]}", new Vector2(1275, 700), 180);
        ButtonCollection.addButtonArrow("Left", $"{GameManager.SettingsManager.Controls[(_inputDevice, UserAction.Left)]}", new Vector2(1075, 500), 270);
        ButtonCollection.addButtonArrow("Right", $"{GameManager.SettingsManager.Controls[(_inputDevice, UserAction.Right)]}", new Vector2(1475, 500),  90);
        ButtonCollection.addButtonAtPosition("Execute", $"Shoot/Punch: {GameManager.SettingsManager.Controls[(_inputDevice, UserAction.Execute)]}", new Vector2(GameManager.SettingsManager.Resolution.Width * (3f/6f), (600 * yScale + 200 * Scale) / yScale));
        ButtonCollection.addButtonAtPosition("SwitchWeapon", $"Switch Weapon: {GameManager.SettingsManager.Controls[(_inputDevice, UserAction.SwitchWeapon)]}", new Vector2(GameManager.SettingsManager.Resolution.Width * (5f/6f), (600 * yScale + 200 * Scale) / yScale));
    } 
    
    /// <summary>
    /// Checks if the newly pressed key conflicts with existing key assignments.
    /// </summary>
    /// <param name="pressedKeys">The array of currently pressed keys.</param>
    /// <returns>True if there is no conflict; otherwise, false.</returns>
    private bool checkLogic(KeyboardKey[] pressedKeys)
    {
        var newKey = pressedKeys[0];

        foreach (var control in GameManager.SettingsManager.Controls)
        {
            var (inputDevice, userAction) = control.Key;
            var key = control.Value;

            // Skip the current action being reassigned to avoid self-comparison.
            if (inputDevice == _inputDevice && userAction == (UserAction)_buttonToBeAssigned)
                continue;

            // Check for collisions by comparing the key values
            if (keysAreEqual(key, newKey))
            {
                _conflictDetected = true;
                return false;
            }
        }
        _conflictDetected = false;
        return true;
    }
    
    /// <summary>
    /// Compares two IKey objects for equality based on their specific type and value.
    /// </summary>
    /// <param name="key1">The first key to compare.</param>
    /// <param name="key2">The second key to compare.</param>
    /// <returns>True if the keys are equal; otherwise, false.</returns>
    private bool keysAreEqual(IKey key1, IKey key2)
    {
        if (key1 is KeyboardKey keyboardKey1 && key2 is KeyboardKey keyboardKey2)
        {
            return keyboardKey1.Key == keyboardKey2.Key;
        }
        if (key1 is GamePadButton gamePadKey1 && key2 is GamePadButton gamePadKey2)
        {
            return gamePadKey1.Button == gamePadKey2.Button && gamePadKey1.PlayerIndex == gamePadKey2.PlayerIndex;
        }
        return false;
    }
    
public override void update(GameTime gameTime)
{
    base.update(gameTime);

    // Get the Execute key (whatever it is currently assigned to)
    var executeKey = GameManager.SettingsManager.Controls[(_inputDevice, UserAction.Execute)];

    // Handle button selection (Ignore Execute key for selection)
    for (var i = 0; i < 3; i++)
    {
        if (ButtonCollection[i].isClicked())
        {
            _buttonToBeAssigned = i;
            _lastProcessedKey = executeKey;  // Store Execute key as last processed to prevent instant assignment
            ButtonCollection["save"].HasUnsavedChanges = true;
            return;
        }
    }

    for (var i = 6; i < 12; i++)
    {
        if (ButtonCollection[i].isClicked())
        {
            _buttonToBeAssigned = i - 3;
            _lastProcessedKey = executeKey;
            ButtonCollection["save"].HasUnsavedChanges = true;
            return;
        }
    }

    // Handle save, reset, and return buttons
    if (ButtonCollection["save"].isClicked() && !_conflictDetected)
    {
        GameManager.SaveManager.saveSettingsToFile(GameManager.SettingsManager.buildSettingsState());
        ButtonCollection["save"].HasUnsavedChanges = false;
    }

    if (ButtonCollection["reset"].isClicked())
    {
        GameManager.SettingsManager.loadSettings(GameManager.SettingsManager.createDefaultPlayerSettings(_inputDevice));
        ButtonCollection["save"].HasUnsavedChanges = false;
        changeResolution();
    }


    if (ButtonCollection["return"].isClicked() && !_conflictDetected)
        GameManager.ScreenManager.removeFromStack(this);

    // Disable gamepad assignments for now (as per TODO)
    if (_inputDevice is InputDevice.GamePadOne or InputDevice.GamePadTwo)
        _buttonToBeAssigned = -1;

    // Handle key assignment
    if (_buttonToBeAssigned != -1)
    {
        var action = (UserAction)_buttonToBeAssigned;
        bool isArrow = action == UserAction.Up || action == UserAction.Left || action == UserAction.Right || action == UserAction.Down;

        // Update the previous button's text if a new button is selected
        if (_previousButtonToBeAssigned != -1 && _previousButtonToBeAssigned != _buttonToBeAssigned)
        {
            var prevAction = (UserAction)_previousButtonToBeAssigned;
            if (ButtonCollection[prevAction.ToString()] is { } prevButton)
            {
                string prevDisplayName = _actionDisplayNames.TryGetValue(prevAction, out var prevLabel) ? prevLabel : prevAction.ToString();
                prevButton.Text = isArrow ? $"{GameManager.SettingsManager.Controls[(_inputDevice, prevAction)]}"
                    : $"{prevDisplayName}: {GameManager.SettingsManager.Controls[(_inputDevice, prevAction)]}";
            }
        }

        // Handle key assignment for the selected button
        if (ButtonCollection[action.ToString()] is { } button)
        {
            string displayName = _actionDisplayNames.TryGetValue(action, out var label) ? label : action.ToString();
            button.Text = isArrow ? "?" : $"{displayName}: ?";

            // Get the currently pressed keys
            var pressedKeys = GameManager.InputHandler.getPressedKeyboardKeys();

            // Ignore Execute key if it was the last processed key
            if (pressedKeys.Length == 1 && !keysAreEqual(pressedKeys[0], _lastProcessedKey))
            {
                if (checkLogic(pressedKeys))
                {
                    var newKey = pressedKeys[0];
                    GameManager.SettingsManager.Controls[(_inputDevice, action)] = newKey;
                    button.Text = isArrow ? $"{newKey}" : $"{displayName}: {newKey}";
                    _buttonToBeAssigned = -1; // Reset the button to be assigned
                    _lastProcessedKey = null;  // Clear last processed key to allow Execute reassignment later
                }
            }
        }
        _previousButtonToBeAssigned = _buttonToBeAssigned;
    }
}

    public override void draw(GameTime gameTime)
    {
        base.draw(gameTime);

        if (!_conflictDetected) return;
        
        var font = GameManager.AssetManager.Fonts["gabriele"];
        var textSize = font.MeasureString(_warning);
        
        var centeredX = GameManager.SettingsManager.Resolution.Width / 2f - (textSize.X * Scale);
        var position = new Vector2(centeredX, GameManager.SettingsManager.Resolution.Height * (5f/6f));
        
        GameManager.SpriteBatch.DrawString(font, _warning, position, Color.Red, 0f, Vector2.Zero, Scale*2f, SpriteEffects.None, 0f);
    }

    public override void changeResolution()
    {
        yScale = GameManager.SettingsManager.Resolution.Width / 1920f;
        yScale = GameManager.SettingsManager.Resolution.Height / 1080f;
        Scale = Math.Min(xScale, yScale);
        ButtonCollection = new ButtonCollection(GameManager, new Vector2(GameManager.SettingsManager.Resolution.Width * (1f/6f), 200), [PlayerCharacterId.Walt, PlayerCharacterId.Jesse], 500, 70);
        createMenuElements();
    }
}
