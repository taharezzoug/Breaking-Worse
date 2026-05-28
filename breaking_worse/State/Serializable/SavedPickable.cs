using System.Collections.Generic;
using breaking_worse.Objects.Items;

namespace breaking_worse.State.Serializable;

public struct SavedPickable
{
    public int PositionX { get; set; }
    public int PositionY { get; set; }

    public List<(PossiblePickables, int)> Drops = [];

    public SavedPickable(float positionX, float positionY, List<Drop> drops)
    {
        PositionX = (int)positionX;
        PositionY = (int)positionY;
        foreach (var drop in drops)
        {
            Drops.Add((drop.Pickable, drop.Quantity));
        }
    }
}