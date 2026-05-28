using System.Collections.Generic;
using System.Linq.Expressions;

namespace breaking_worse.Objects.Items;

public class Recipe
{
    public ItemName Name { get; set; }
    
    public int UpgradeLevel { get; set; }

    public List<(Item, int)> RecipeItemList;

    public Recipe(ItemName itemName)
    {
         Name = itemName;
         setRecipeList();
         setUpgradeLevel();
    }
    
    private void setRecipeList()
    {
        switch (Name)
        {
            case ItemName.AluminumGranules:
                RecipeItemList = [(new Item(ItemName.AluminumFoil), 10)];
                return;  
            case ItemName.Pseudoephedrine:
                RecipeItemList = [(new Item(ItemName.ColdMedicine), 3)];
                return;
            case ItemName.RedPhosphorus:
                RecipeItemList =  [(new Item(ItemName.Matches), 15)];
                return;
            
            case ItemName.MethSmall:
                RecipeItemList = [
                    (new Item(ItemName.Pseudoephedrine),1),
                    (new Item(ItemName.RedPhosphorus),2),
                    (new Item(ItemName.Acetone),5),
                    (new Item(ItemName.ErlenmeyerFlask), 1)
                ];
                return;
            case ItemName.Meth:
                RecipeItemList = [
                    (new Item(ItemName.Methylamine), 1),
                    (new Item(ItemName.Phenylacetone), 1),
                    (new Item(ItemName.AluminumGranules), 5),
                    (new Item(ItemName.ErlenmeyerFlask), 1)
                ];
                return;
            case ItemName.Cocaine:
                RecipeItemList = [
                    (new Item(ItemName.CocainePlant), 1),
                    (new Item(ItemName.Acetone), 10),
                    (new Item(ItemName.Battery),3),
                    (new Item(ItemName.Petrol),10),
                    (new Item(ItemName.ErlenmeyerFlask), 1)
                ];
                return;
            case ItemName.Crack:
                RecipeItemList = [
                    (new Item(ItemName.Cocaine), 1),
                    (new Item(ItemName.Ammonia), 15),
                    (new Item(ItemName.ErlenmeyerFlask), 1)
                ];
                return;
            default:
                RecipeItemList = [];
                return;
        }
    }

    private void setUpgradeLevel()
    {
        switch (Name)
        {
            case ItemName.Pseudoephedrine:
                UpgradeLevel = 0;
                return;
            case ItemName.RedPhosphorus:
                UpgradeLevel = 0;
                return;
            case ItemName.MethSmall:
                UpgradeLevel = 0;
                return;
            
            case ItemName.AluminumGranules:
                UpgradeLevel = 1;
                return;
            case ItemName.Meth:
                UpgradeLevel = 2;
                return;
            
            case ItemName.Cocaine:
                UpgradeLevel = 3;
                return;
            
            case ItemName.Crack:
                UpgradeLevel = 3;
                return;
            
            default:
                UpgradeLevel = 0;
                return;
        }
    }
}