using System.Collections.Generic;
using breaking_worse.Objects.Items;
using Microsoft.Xna.Framework.Graphics;

namespace breaking_worse.Objects.Player;

public class Inventory
{
    /// <summary>
    /// Both players share an instance of this class for management of items/money/bullets
    /// currently in possession of both Players.
    /// </summary>
    public Dictionary<ItemName, int> Items { get; set; }
    public int Money {get; set;}
    public Texture2D SelectedRecipe;
    public Inventory(Dictionary<ItemName, int> items, int money, int bulletAmount)
    {
        Items = items;
        Items.Add(ItemName.Matches, 25);
        Items.Add(ItemName.Acetone, 10);
        Items.Add(ItemName.ColdMedicine, 5);
        Items.Add(ItemName.ErlenmeyerFlask, 2);
        
        addAmount(new Item(ItemName.Ammo), bulletAmount);
        
        Money = money;
    }

    public void addAmount(Item item, int amount)
    {
        bool inSet = false;
        foreach (var itemPair in Items)
        {
             if (itemPair.Key == item.Name)
             {
                 inSet = true;
                 Items[itemPair.Key] += amount;
                 break;
             }
        }
        if (!inSet)
        { 
             Items.Add(item.Name, amount);
        }
    }

    public bool removeAmount(Item item, int amount)
    {
        Items[item.Name] -= Items[item.Name] >= amount ? amount : 0;
        return hasAmount(item, amount);
    }

    public bool hasAmount(Item item, int amount)
    {
        if (Items.ContainsKey(item.Name) && Items[item.Name] >= amount) { return true; }
        return false;
    }

    public string getBulletAmount()
    {
        return Items[ItemName.Ammo].ToString();
    }

    public bool hasEnoughAmmo()
    {
        if (Items[ItemName.Ammo] > 0)
        {
            Items[ItemName.Ammo]--;
            return true;
        }
        return false;
    }

    public void cleanInventory()
    {
        foreach (var itemPair in Items)
        {
            if (itemPair.Value <= 0)
            {
                Items.Remove(itemPair.Key);
            }
        }
    }

    public void changeRecipe(Texture2D newRecipe)
    {
        SelectedRecipe = newRecipe;
    }
    
    // TODO: Integration into saving process
}
