using System;
using System.Collections.Generic;
using breaking_worse.State.Enums;
using breaking_worse.State.Serializable;
using Microsoft.Xna.Framework;

namespace breaking_worse.Objects;

public abstract class AGameObject(GameManager gameManager)
{
    protected readonly GameManager GameManager = gameManager;
    
    // maps a type (should only be classes that implement IComponent) to an object of IComponent
    // -> not a List here, so that each Component can be accessed more efficiently
    private readonly Dictionary<Type, IComponent> _components = new();

    public int ObjectId;
    public NpcType Type;
    
    public Vector2 Position { get;  set; }
    
    public bool CanBeTalkedToWalt { get; set; }
    public bool CanBeTalkedToJesse { get; set; }

    // add component to gameObject
    protected void addComponent(IComponent component)
    { 
        _components.TryAdd(component.GetType(), component);
    }

    // true if gameObject has component of given type
    public bool hasComponent<T>()
    {
        return _components.ContainsKey(typeof(T));
    }
    
    // returns component of given type if present
    public T getComponent<T>() where T : class, IComponent
    {
        _components.TryGetValue(typeof(T), out var component);
        return component as T;
    }
    
    public abstract void update(GameTime gameTime);
    public abstract void render(GameTime gameTime);
    public abstract void draw(GameTime gameTime);
    
    // saves current state of gameObject in gameState 
    public abstract void saveState(GameState gameState);
    
    // loads gameState in gameObject
    public abstract void loadState(SavedGameObject gameObject);
    
    // load assets and initialize some stuff that depends on them
    public abstract void loadContent();
}
