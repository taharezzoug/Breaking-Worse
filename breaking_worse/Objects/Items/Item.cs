using Microsoft.Xna.Framework.Graphics;

namespace breaking_worse.Objects.Items;

public class Item
{
    public ItemName Name;
    public int Price { get; }
    public Texture2D Texture {get; set;}
    public ItemType Type { get; }
    
    public Item(ItemName name)
    {
        Name = name;
        (Price, Type) = name switch
        {
            ItemName.Acetone => (10, ItemType.Buyable),
            ItemName.Ammo => (50, ItemType.Buyable),
            ItemName.Ammonia => (20, ItemType.Buyable),
            ItemName.Battery => (15, ItemType.Buyable),
            ItemName.Matches => (10, ItemType.Buyable),
            ItemName.Hp => (100, ItemType.Buyable),
            ItemName.Methylamine => (300, ItemType.Buyable),
            ItemName.Petrol => (20, ItemType.Buyable),
            ItemName.Phenylacetone => (200, ItemType.Buyable),
            ItemName.AluminumFoil => (10, ItemType.Buyable),
            ItemName.CocainePlant => (400, ItemType.Buyable),
            ItemName.CancerTreatment => (20000, ItemType.Buyable),
            ItemName.ErlenmeyerFlask => (25, ItemType.Buyable),
            ItemName.ColdMedicine => (10, ItemType.Buyable),
            
            ItemName.MethSmall => (450, ItemType.Sellable),
            ItemName.Meth => (2000, ItemType.Sellable),
            ItemName.Cocaine => (2300, ItemType.Sellable),
            ItemName.Crack => (3250, ItemType.Sellable),
            
            _ => (0, ItemType.Neither)
        };

    }
}
