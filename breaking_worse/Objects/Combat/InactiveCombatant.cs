using System;
using breaking_worse.Objects.Animations;
using breaking_worse.Sound;
using breaking_worse.State.Enums;

namespace breaking_worse.Objects.Combat;

public class InactiveCombatant(GameManager gameManager, int initialHealth = 2, Animatable animatable = null) : IComponent
{
    protected readonly GameManager GameManager = gameManager;
    protected readonly Animatable Animatable = animatable;
    
    public int Health { get; set; } = initialHealth;
    public bool IsTrash { get; set; } = false;
    private Random _random = new Random();
    private Sfx[] _sfxTrash = [Sfx.TrashCan1, Sfx.TrashCan2];
    
    // animations
    private const string HurtAnimation = "hurt";
    private const string DieAnimation = "die";
    
    // time until object despawns after death
    public DateTime? DeathTime { get; set; }
    private readonly TimeSpan _deathTimeSpan = TimeSpan.FromSeconds(6);
    
    public bool shouldDespawn()
    {
        return Health <= 0 && DateTime.Now - DeathTime >= _deathTimeSpan;
    }
    
    // decreases health by given amount, returns true if combatant dies
    public bool receiveDamage(int amount)
    {
        Health -= amount;
        var dead = false;
        
        if (Health <= 0)
        {
            if (DeathTime == null)
            {
                DeathTime = DateTime.Now;
                GameManager.SoundManager.playSoundEffect(IsTrash ? _sfxTrash[_random.Next(_sfxTrash.Length)] : Sfx.Death);
                Animatable?.startAnimation(DieAnimation);
            }
            dead = true;
        }
        else
        {
            if(!IsTrash) GameManager.SoundManager.playSoundEffect(Sfx.Hurt);
            Animatable?.startAnimation(HurtAnimation);
        } 
        
        GameManager.ScreenManager.InGameScreen.PlayerState.balanceHealth();
        GameManager.ProgressManager.updateStatistics(Statistics.DamageTaken, amount, 1);
        
        return dead;
    }
}
