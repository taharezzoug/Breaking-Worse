using System;
using System.Collections.Generic;
using System.Linq;
using breaking_worse.Input.Enums;
using breaking_worse.Objects.Player;
using breaking_worse.Utility;
using Microsoft.Xna.Framework;

namespace breaking_worse.Screens.Clickables;

public sealed class ButtonCollection
{
    /// <summary>
    /// A collection/list of all buttons with a method to addButtons easily
    /// </summary>

    private readonly GameManager _gameManager;

    // maps button name to index in _buttons
    private readonly Dictionary<string, int> _ids = new();
    private readonly List<AMenuElement> _buttons = [];

    private Vector2 _position;
    private int _buttonWidth;
    private int _buttonHeight;
    private readonly int _distance;
    private readonly float _scale;
    private readonly float _xScale;
    private readonly float _yScale;
    private int _maxButtonWidth;

    private bool _isInSelectionMode;
    private bool _isInMiniGame;
    private bool _isDialog;

    private int _selectedButtonId = -1;
    private int _buttonCount = -1;

    public AMenuElement this[string name] => _buttons[_ids[name]];
    public AMenuElement this[int idx] => _buttons[idx];

    private List<PlayerCharacterId> _correspondingPlayers;

    private const int SliderHeight = 60;
    private const int SliderWidth = 600;

    public ButtonCollection(GameManager gameManager,
        Vector2 position,
        List<PlayerCharacterId> correspondingPlayers,
        int buttonWidth = 350,
        int buttonHeight = 80,
        int distance = 100)
    {
        _gameManager = gameManager;
        _xScale = gameManager.SettingsManager.Resolution.Width / 1920f;
        _yScale = gameManager.SettingsManager.Resolution.Height / 1080f;
        _scale = Math.Min(_xScale, _yScale);


        _position = new Vector2(position.X, position.Y * _yScale);
        _buttonWidth = (int)(buttonWidth * _scale);
        _buttonHeight = (int)(buttonHeight * _scale);
        _distance = (int)(distance * _scale);
        _maxButtonWidth = _buttonWidth;
        _correspondingPlayers = correspondingPlayers;
    }

    public void addButton(string name, string text, float textSize = 0.7f, string texture = null)
    {
        _buttonCount += 1;
        var correctedPosition =
            new Vector2(_position.X - _buttonWidth / 2f,
                _position.Y + _distance * _buttonCount); // button not yet added to list, so count is one smaller
        var hitBox = new Rectangle(correctedPosition.ToPoint(), new Point(_buttonWidth, _buttonHeight));
        _buttons.Add(new Button(_gameManager, this, text, hitBox, textSize, texture));
        _ids[name] = _buttons.Count - 1;
    }

    public void addButtonAtPosition(string name,
        string text,
        Vector2 position,
        string texture = null,
        float textOffSet = 0)
    {
        _buttonCount -= _buttons.Count - 1;
        _position = new Vector2(position.X, position.Y * _yScale);
        var correctedPosition = new Vector2(_position.X - _buttonWidth / 2f, _position.Y);
        var hitBox = new Rectangle(correctedPosition.ToPoint(), new Point(_buttonWidth, _buttonHeight));
        _buttons.Add(new Button(_gameManager, this, text, hitBox, 0.7f, texture, textOffSet));
        _ids[name] = _buttons.Count - 1;
    }

    public void addTextElement(string name, string text, int width, float size = 0.7f)
    {
        _buttonCount += 1;
        var scaledWidth = width * _scale;
        var correctedPosition =
            new Vector2(_position.X - scaledWidth / 2f,
                _position.Y + _distance * _buttonCount); // button not yet added to list, so count is one smaller
        var hitBox = new Rectangle(correctedPosition.ToPoint(), new Point((int)(scaledWidth), _buttonHeight));
        _buttons.Add(new TextElement(_gameManager, this, text, hitBox, size));
        _ids[name] = _buttons.Count - 1;

        if (scaledWidth > _maxButtonWidth)
            _maxButtonWidth = (int)scaledWidth;
    }

    public void removeButton(string name)
    {
        _buttonCount -= 1;
        _buttons.Remove(_buttons[_ids[name]]);
        _ids.Remove(name);
    }

    public void setButtonSize(int width = 350, int height = 80)
    {
        _buttonWidth = (int)(width * _scale);
        _buttonHeight = (int)(height * _scale);
    }

    public void addAchievement(string name, string text, int width)
    {
        _buttonCount += 1;
        var correctedPosition =
            new Vector2(_position.X - width * _scale / 2f,
                _position.Y + _distance * _buttonCount); // button not yet added to list, so count is one smaller
        var hitBox = new Rectangle(correctedPosition.ToPoint(), new Point((int)(width * _scale), _buttonHeight));
        _buttons.Add(new AchievementClickable(_gameManager, this, text, hitBox));
        _ids[name] = _buttons.Count - 1;
    }

    public void addAchievementAtPosition(string name, string text, int width, Vector2 position)
    {
        _buttonCount -= _buttons.Count - 1;
        _position = new Vector2(position.X, position.Y * _yScale);
        var correctedPosition = new Vector2(_position.X - width * _scale / 2f, _position.Y);
        var hitBox = new Rectangle(correctedPosition.ToPoint(), new Point((int)(width * _scale), _buttonHeight));
        _buttons.Add(new AchievementClickable(_gameManager, this, text, hitBox));
        _ids[name] = _buttons.Count - 1;
    }

    public Slider addSlider(string name, float value, string displayName = "")
    {
        _buttonCount += 1;
        var position1 =
            new Vector2(_position.X - SliderWidth * _scale / 2f,
                _position.Y + _distance * _buttonCount); // button not yet added to list, so count is one smaller
        var hitBox = new Rectangle(position1.ToPoint(),
            new Point((int)(SliderWidth * _scale), (int)(SliderHeight * _scale)));
        var newSlider = new Slider(_gameManager, this, hitBox, value, displayName);
        _buttons.Add(newSlider);
        _ids[name] = _buttons.Count - 1;
        return newSlider;
    }

    public void addButtonArrow(string name, string text, Vector2 position, int rotation)
    {
        _position = new Vector2(position.X * _xScale, position.Y * _yScale);
        _buttonCount += _buttons.Count;
        var correctedPosition =
            new Vector2(_position.X, _position.Y); // button not yet added to list, so count is one smaller
        var hitBox = new Rectangle(correctedPosition.ToPoint(), new Point((int)(150 * _scale), (int)(150 * _scale)));
        _buttons.Add(new ButtonArrow(_gameManager, this, text, hitBox, rotation));
        _ids[name] = _buttons.Count - 1;
    }

    public void update()
    {
        if (_isInMiniGame) return;
        if (!_isInSelectionMode)
        {
            if (_isDialog)
                _selectedButtonId = 1;
            else
                _selectedButtonId = 0;

            _buttons[_selectedButtonId].IsSelectedByButtonCollection = true;
            _isInSelectionMode = true;
        }
        else
        {
            _buttons[_selectedButtonId].IsSelectedByButtonCollection = true;
            if (isUserActionForCorrespondingPlayers(UserAction.Up))
                incrementSelectedButtonId(-1);
            else if (isUserActionForCorrespondingPlayers(UserAction.Down))
                incrementSelectedButtonId(1);
            if (isUserActionForCorrespondingPlayers(UserAction.Right))
            {
                // searches for the nearest button on the right side and goes there if it exists
                var buttonOnRight = getButtonOnSide(_buttons[_selectedButtonId], false);

                if (buttonOnRight != null && (_buttons[_selectedButtonId].GetType() != typeof(Slider) &&
                                              buttonOnRight.GetType() != typeof(Slider)))
                {
                    var index = _buttons.IndexOf(buttonOnRight);
                    if (_buttons[_selectedButtonId].GetType() == typeof(AchievementClickable) ||
                        buttonOnRight.GetType() == typeof(AchievementClickable))
                        incrementSelectedButtonId(6);
                    else if (index != -1)
                    {
                        incrementSelectedButtonId(index - _selectedButtonId);
                    }
                }
            }
            else if (isUserActionForCorrespondingPlayers(UserAction.Left))
            {
                // searches for the nearest button on the left side and goes there if it exists
                var buttonOnLeft = getButtonOnSide(_buttons[_selectedButtonId], true);

                if (buttonOnLeft != null && (_buttons[_selectedButtonId].GetType() != typeof(Slider) &&
                                             buttonOnLeft.GetType() != typeof(Slider)))
                {
                    var index = _buttons.IndexOf(buttonOnLeft);
                    if (_buttons[_selectedButtonId].GetType() == typeof(AchievementClickable) ||
                        buttonOnLeft.GetType() == typeof(AchievementClickable))
                        incrementSelectedButtonId(-6);
                    else if (index != -1)
                    {
                        incrementSelectedButtonId(index - _selectedButtonId);
                    }
                }
            }
        }

        if (_buttons[_selectedButtonId] is Slider slider)
        {
            slider.updateWithKeys();
        }

        foreach (var button in _buttons)
            button.updateShade();
    }

    public bool isUserActionForCorrespondingPlayers(UserAction userAction)
    {
        return _correspondingPlayers.Aggregate(false,
            (current, playerCharacterId) => current ||
                                            _gameManager.UserActionHandler.isUserAction(playerCharacterId,
                                                userAction,
                                                PressType.PressAndRelease));
    }

    private void incrementSelectedButtonId(int offSet)
    {
        _buttons[_selectedButtonId].IsSelectedByButtonCollection = false;
        _selectedButtonId = OwnMath.mod(_selectedButtonId + offSet, _buttons.Count);
        if (_isDialog && _selectedButtonId == 0)
            _selectedButtonId = OwnMath.mod(_selectedButtonId + offSet, _buttons.Count);
        _buttons[_selectedButtonId].IsSelectedByButtonCollection = true;
    }

    // checks if there are buttons in the button collection that are on the left or right side of the given button
    private AMenuElement getButtonOnSide(AMenuElement button, bool left)
    {
        AMenuElement closestButton = null;
        foreach (var element in _buttons)
        {
            if (left)
            {
                if (button.HitBox.Center.X > element.HitBox.Center.X && (closestButton == null ||
                                                                         element.HitBox.Center.X >
                                                                         closestButton.HitBox.Center.X))
                {
                    closestButton = element;
                }
            }
            else
            {
                if (button.HitBox.Center.X < element.HitBox.Center.X && (closestButton == null ||
                                                                         element.HitBox.Center.X <
                                                                         closestButton.HitBox.Center.X))
                {
                    closestButton = element;
                }
            }
        }

        return closestButton;
    }

    public void draw()
    {
        // if (_isDialog)
        //     _gameManager.SpriteBatch.Draw(_gameManager.AssetManager.Images["DialogBackground"], new Rectangle((int)(_position.X - _maxButtonWidth  / 2f), (int)(_position.Y), (int)(_maxButtonWidth), (int)(_buttonHeight + _distance * _buttons.Count)), null, Color.White * 0.6f, 0f, Vector2.Zero, SpriteEffects.None, 0f);

        foreach (var button in _buttons)
            button.draw();
    }

    public bool isInCollection(string buttonName) => _ids.ContainsKey(buttonName);

    public void inMiniGame()
    {
        _isInMiniGame = !_isInMiniGame;
    }

    public void isDialog(bool isDialog)
    {
        _isDialog = isDialog;
    }

    public bool isInMiniGame() => _isInMiniGame;

    public int ButtonHeight => _buttonHeight;

    public List<AMenuElement> Buttons => _buttons;

    public int Height => _buttonHeight + _distance * (_buttons.Count - 1);
}