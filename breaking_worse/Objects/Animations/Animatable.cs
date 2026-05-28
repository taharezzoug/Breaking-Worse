using System.Collections.Generic;

namespace breaking_worse.Objects.Animations;

public class Animatable : IComponent
{
    private readonly Dictionary<string, Animation> _animations = new();
    public string CurrentAnimationName { get; set; }
    
    public Animation CurrentAnimation => _animations[CurrentAnimationName];

    public void addAnimation(string name, Animation animation)
    {
        _animations.TryAdd(name, animation);
    }

    public void startAnimation(string name, bool force = false)
    {
        if (!force && (_animations[CurrentAnimationName].IsActive || _animations[CurrentAnimationName].RepeatLastFrame) && _animations[CurrentAnimationName].Priority > _animations[name].Priority) return;
        
        CurrentAnimation.stop();
        CurrentAnimationName = name;
        CurrentAnimation.start();
    }
}
