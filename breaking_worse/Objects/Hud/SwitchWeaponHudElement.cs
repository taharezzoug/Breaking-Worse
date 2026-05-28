using System;
using breaking_worse.Objects.Player;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace breaking_worse.Objects.Hud;

public class SwitchWeaponHudElement(GameManager gameManager, PlayerCharacter playerCharacter, Func<string> text)
    : HudElement(gameManager, playerCharacter.PlayerCharacterId == PlayerCharacterId.Walt ? new Vector2(550, 20) : new Vector2(620, 20))
{
    private float _xScale = gameManager.SettingsManager.Resolution.Width / 1920f;
    private float _yScale = gameManager.SettingsManager.Resolution.Height / 1080f;
    private float _scale;
    private const float IconScale = 0.1f;
    private readonly Texture2D _fist = gameManager.AssetManager.Images["fist"];
    private readonly Texture2D _gun = gameManager.AssetManager.Images["gun"];
    private readonly Vector2 _textOffSet = new(25, 30);
    private int _originalScreenWidth;


    public override void draw()
    {
        _scale = Math.Min(_xScale, _yScale);
        var icon = playerCharacter.EquippedWeapon == Weapon.Fist ? _fist : _gun;
        GameManager.SpriteBatch.Draw(icon, new Rectangle(new Point((int)(Position.X * _scale), (int)(Position.Y * _scale)), new Point((int)(icon.Width * IconScale * _scale), (int)(icon.Height * IconScale * _scale))), Color.White);
        if(playerCharacter.EquippedWeapon == Weapon.Gun) GameManager.SpriteBatch.DrawString(GameManager.AssetManager.Fonts["numbersSmall"], text(), new Vector2((Position.X + _textOffSet.X) * _xScale, (Position.Y + _textOffSet.Y) * _yScale), Color.White);
    }

    public override void update()
    {
        if (_originalScreenWidth != GameManager.SettingsManager.Resolution.Width)
        {
            _xScale = GameManager.GraphicsDeviceManager.GraphicsDevice.Viewport.Width / 1920f;
            _yScale = GameManager.GraphicsDeviceManager.GraphicsDevice.Viewport.Height / 1080f;
            _scale = Math.Min(_xScale, _yScale);
            _originalScreenWidth = GameManager.SettingsManager.Resolution.Width;
        }
    }
}
