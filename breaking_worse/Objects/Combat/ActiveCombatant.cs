using System;
using System.Collections.Generic;
using System.Linq;
using breaking_worse.Objects.Animations;
using breaking_worse.Objects.Collisions;
using breaking_worse.Objects.Player;
using breaking_worse.Sound;
using Microsoft.Xna.Framework;

namespace breaking_worse.Objects.Combat;

public class ActiveCombatant(GameManager gameManager, int range, int fov, Func<Weapon> equippedWeapon, Animatable animatable, int initialHealth = 10, bool isPlayer = false) : InactiveCombatant(gameManager, initialHealth, animatable)
{
    // animations
    private const string PunchAnimation = "punch";
    private const string ShootAnimation = "shoot";
    
    private string _usedAnimation;
    private Sfx _usedSoundEffect;
    
    private PlayerCharacterId _player;

    public void attack(Collider collider, Vector2 facing, PlayerCharacterId id = 0)
    {
        _player = id;
        
        if (isPlayer && equippedWeapon() == Weapon.Gun && !GameManager.ScreenManager.InGameScreen.PlayerState.Inventory.hasEnoughAmmo())
            return;
        
        var usedRange = equippedWeapon() == Weapon.Fist ? 2 : range;
        var usedFov = equippedWeapon() == Weapon.Fist ? 270 : fov;
        var neighborsInFov = GameManager.ScreenManager.InGameScreen.CollisionManager.getNeighborsInFov(collider, usedRange, facing, usedFov, true);
        _usedAnimation = equippedWeapon() == Weapon.Fist ? PunchAnimation : ShootAnimation;
        _usedSoundEffect = equippedWeapon() == Weapon.Fist ? Sfx.Punch : Sfx.GunShot;
        
        if (!isPlayer) hitClosestPlayer(neighborsInFov);
        else hitClosest(neighborsInFov);
    }

    // Takes a list of neighbours and decides hits the closest entity to the player
    // The Line Of Sight is handled directyly by the getNeighborsInFov in Collision Manager
    private void hitClosest(List<Collider> neighborsInFov)
    {
        Collider nearest = null;
        float nearestDistance = 1000000;
        Animatable?.startAnimation(_usedAnimation);
        GameManager.SoundManager.playSoundEffect(_usedSoundEffect);
        foreach (var neighbor in neighborsInFov)
        {
            if ( neighbor.InactiveCombatant.Health <= 0) continue;
            if (distanceToPlayer(neighbor) >= nearestDistance) continue;
            nearest = neighbor;
            nearestDistance = distanceToPlayer(neighbor);
        }
        if (nearest != null)
        {
            bool dead;
            if (nearest.IsPlayer)
            {
                dead = nearest.InactiveCombatant.receiveDamage(equippedWeapon() == Weapon.Fist ? 1 : 3);
                GameManager.ScreenManager.InGameScreen.PlayerState.TimeSinceLastDamage = 0;
                if (dead)
                    GameManager.ScreenManager.InGameScreen.PlayerState.SuspiciousActivityCounter = 0;
            }
            else dead = nearest.InactiveCombatant.receiveDamage(equippedWeapon() == Weapon.Fist ? 1 : 10);
            if (nearest.IsCartel)
            {
                GameManager.ScreenManager.InGameScreen.PlayerState.WillShoot = true;
                GameManager.ScreenManager.InGameScreen.PlayerState.AggroCooldown = 100f;
            }
            if (!nearest.IsTrash && isPlayer) handleSuspiciousActivityCounter(dead);
        }

    }
    private void hitClosestPlayer(List<Collider> neighborsInFov)
    {
        Collider player = null;
        float nearestDistance = 1000000;
        
        foreach (var neighbor in from neighbor in neighborsInFov where neighbor.InactiveCombatant.Health > 0 where !(distanceToPlayer(neighbor) >= nearestDistance) where neighbor.IsPlayer select neighbor)
        {
            player = neighbor;
            nearestDistance = distanceToPlayer(neighbor);
        }
        if (player == null) return;
        Animatable?.startAnimation(_usedAnimation);
        GameManager.SoundManager.playSoundEffect(_usedSoundEffect);
        var dead = player.InactiveCombatant.receiveDamage(equippedWeapon() == Weapon.Fist ? 1 : 3);
        GameManager.ScreenManager.InGameScreen.PlayerState.TimeSinceLastDamage = 0;
        if (dead)
            GameManager.ScreenManager.InGameScreen.PlayerState.SuspiciousActivityCounter = 0;
    }
    
    private float distanceToPlayer(Collider neighbor)
    {
        if (neighbor == null) return 1000000;
        
        Vector2 position;
        
        switch (_player)
        {
            case PlayerCharacterId.Walt:
                position = GameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.Position;
                return Vector2.Distance(neighbor.DynamicHitBox.Position, position);
            case PlayerCharacterId.Jesse:
                position = GameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.Position;
                return Vector2.Distance(neighbor.DynamicHitBox.Position, position);
            default:
                return 1000000;
        }
    }
    
    private void handleSuspiciousActivityCounter(bool dead)
    {
        var increase = 0;
        if (dead) increase += 10;
        increase += equippedWeapon() switch
        {
            Weapon.Fist => 1,
            Weapon.Gun => 5,
            _ => 0
        };
        GameManager.ScreenManager.InGameScreen.PlayerState.SuspiciousActivityCounter += increase;
        GameManager.ScreenManager.InGameScreen.PlayerState.LastTimeSeen = DateTime.Now;
    }
}
