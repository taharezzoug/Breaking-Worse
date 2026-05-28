using System;
using System.Collections.Generic;
using breaking_worse.Objects.Interaction;
using breaking_worse.Objects.Items;
using breaking_worse.Objects.Player;
using breaking_worse.Screens.Clickables;
using breaking_worse.Screens.ScreenTypes.MenuScreens;
using breaking_worse.State;
using breaking_worse.State.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace breaking_worse.Screens.ScreenTypes.InGameScreens;

public class DialogueScreen(GameManager gameManager, Vector2 position, List<PlayerCharacterId> correspondingPlayers, Npc npc = null, NpcBuyer npcBuyer = null, float areaPrice = 1) : AScreen(gameManager, true, true)
{
    public ButtonCollection ButtonCollection { get; private set; } = new(gameManager, position, correspondingPlayers, 550);
    
    public List<(Rectangle, (Texture2D, Color))> Rectangles { get; private set; } = [];

    public List<TextBox> TextBoxes { get; private set;  } = [];
    
    private Npc _npc;
    private NpcBuyer _npcBuyer;
    private readonly DifficultySettings _settings = gameManager.SettingsManager.getDifficultySettings();
    private string _selectedRecipe;
    private string _message;
    private float _cooldownMessage;
    
    public override void update(GameTime gameTime)
    {
        _npc = npc;
        _npcBuyer = npcBuyer;
        
        ButtonCollection.update();
        
        CookingStation cookingStation = GameManager.ScreenManager.InGameScreen.GameObjectManager.CookingStation;
        
        // --------------------------------------------------------------------------------------------------------------------------
        // buying button actions ---------------------------------------------------------------------------------------------------
        // --------------------------------------------------------------------------------------------------------------------------
        
        foreach (ItemName itemName in Enum.GetValues(typeof(ItemName)))
        {
            var item = new Item(itemName);
            
            if (item.Type != ItemType.Buyable) continue;

            var buttonName = itemName + "_buy";
            var itemPrice = (int)(item.Price * _settings.NpcSellMultiplier * areaPrice); // the Npc sells, you buy
            if(ButtonCollection.isInCollection(buttonName))
                ButtonCollection[buttonName].Text = $"{_npc.Stock[itemName]}x {itemName} ({itemPrice}$)";
            
            if (ButtonCollection.isInCollection(buttonName) && ButtonCollection[buttonName].isClicked())
            {
                if (GameManager.ScreenManager.InGameScreen.PlayerState.Inventory.Money >= itemPrice && _npc.Stock[itemName] > 0)
                {
                    GameManager.ScreenManager.InGameScreen.PlayerState.Inventory.addAmount(item, 1);
                    GameManager.ScreenManager.InGameScreen.PlayerState.Inventory.Money -= itemPrice;
                    _npc.Stock[itemName]--;
                    _npc.startCooldown();
                    if (buttonName == "Hp_buy")
                        GameManager.ScreenManager.InGameScreen.PlayerState.Health += 1;
                    if (buttonName == "CancerTreatment_buy")
                    {
                        setMessage("You are healed!");
                
                        GameManager.ProgressManager.updateStatistics(Statistics.TimesWin, 1, 1);
                        GameManager.ProgressManager.updateAchievements(Statistics.TimesWin, Achievement.FirstWin);
                
                        GameManager.ScreenManager.addScreen(new BackgroundScreen(GameManager,
                            GameManager.AssetManager.Images["gameOverImage"]));
                        GameManager.ScreenManager.addScreen(new GameEndedScreen(GameManager, true));
                    }
                    setMessage($"Bought {itemName}");
                }
                else
                    setMessage(_npc.Stock[itemName] == 0 ? "Out of stock!" : "Not enough money");
            }
        }
        // --------------------------------------------------------------------------------------------------------------------------
        // Selling button actions ---------------------------------------------------------------------------------------------------
        // --------------------------------------------------------------------------------------------------------------------------
        
        foreach (ItemName itemName in Enum.GetValues(typeof(ItemName)))
        {
            var item = new Item(itemName);
            
            if (item.Type != ItemType.Sellable) continue;

            var buttonName = itemName + "_sell";
            var sellPrice = (int)(item.Price * _settings.NpcBuyMultiplier * areaPrice); // The Npc buys, you sell
            
            if(ButtonCollection.isInCollection(buttonName))
                ButtonCollection[buttonName].Text = $"{_npcBuyer.Needs[itemName]}x {itemName} ({sellPrice}$)";
            
            if (ButtonCollection.isInCollection(buttonName) && ButtonCollection[buttonName].isClicked())
            {
                if (GameManager.ScreenManager.InGameScreen.PlayerState.Inventory.hasAmount(item, 1) && _npcBuyer.Needs[itemName] > 0)
                {
                    GameManager.ScreenManager.InGameScreen.PlayerState.Inventory.removeAmount(item, 1);
                    GameManager.ScreenManager.InGameScreen.PlayerState.Inventory.Money += sellPrice;
                    _npcBuyer.Needs[itemName]--;
                    _npcBuyer.startCooldown();
                    GameManager.ProgressManager.updateStatistics(Statistics.DrugsSell, 1, 2);
                    GameManager.ProgressManager.updateStatistics(Statistics.ReceivedMoney, sellPrice, 1);
                    setMessage($"Sold {itemName}");
                }
                else
                    setMessage(_npcBuyer.Needs[itemName] == 0 ? "He had enough for now" : "Not enough items to sell");
            }
        }
        
        GameManager.ProgressManager.updateAchievements(Statistics.DrugsSell, Achievement.XDrugSell);
        GameManager.ProgressManager.updateAchievements(Statistics.ReceivedMoney, Achievement.FirstXMoney);
        
        // --------------------------------------------------------------------------------------------------------------------------
        // Cooking button actions ---------------------------------------------------------------------------------------------------
        // --------------------------------------------------------------------------------------------------------------------------
        
        if (ButtonCollection.isInCollection("return-1") && ButtonCollection["return-1"].isClicked())
        {
            cookingStation.quit();
        }
        
        if (ButtonCollection.isInCollection("return0") && ButtonCollection["return0"].isClicked())
        {
            ButtonCollection = cookingStation.updateButtons(ButtonCollection, 0);
            TextBoxes = cookingStation.updateTextBoxes(TextBoxes, 0);
            Rectangles = cookingStation.updateRectangles(Rectangles, 0);
        }

        if (ButtonCollection.isInCollection("return1") && ButtonCollection["return1"].isClicked())
        {
            ButtonCollection = cookingStation.updateButtons(ButtonCollection, 1);
            Rectangles = cookingStation.updateRectangles(Rectangles, 1);
            TextBoxes = cookingStation.updateTextBoxes(TextBoxes, 1);
        }
        
        if (ButtonCollection.isInCollection("return2") && ButtonCollection["return2"].isClicked())
        {
            ButtonCollection = cookingStation.updateButtons(ButtonCollection, 2);
            Rectangles = cookingStation.updateRectangles(Rectangles, 2);
            TextBoxes = cookingStation.updateTextBoxes(TextBoxes, 2);
        }
        
        // Phase 0 Buttons

        if (ButtonCollection.isInCollection("selectRecipe") && ButtonCollection["selectRecipe"].isClicked())
        {
            ButtonCollection = cookingStation.updateButtons(ButtonCollection, 1);
            TextBoxes = cookingStation.updateTextBoxes(TextBoxes, 1);
        }
        
        if (ButtonCollection.isInCollection("upgrade") && ButtonCollection["upgrade"].isClicked())
        {
            if (GameManager.ScreenManager.InGameScreen.PlayerState.Inventory.Money >=
                (15000 + 15000 * cookingStation.upgradeLevel))
            {
                GameManager.ScreenManager.InGameScreen.PlayerState.Inventory.Money -= 15000 + 15000 * cookingStation.upgradeLevel;
                cookingStation.upgradeLevel += 1;
                cookingStation.Texture = GameManager.AssetManager.Images[
                    "CookingStation" + cookingStation.upgradeLevel switch
                    {
                        2 => "2", 3 => "3", _ => "1"
                    }];
                ButtonCollection = cookingStation.updateButtons(ButtonCollection, 0);
                TextBoxes = cookingStation.updateTextBoxes(TextBoxes, 0);
            }
        }
        
        // phase 1.1 Select Recipe
        
        if (ButtonCollection.isInCollection("Aluminium Granules") && ButtonCollection["Aluminium Granules"].isClicked())
        {
            _selectedRecipe = "Aluminium Granules";
            Recipe recipe = new Recipe(cookingStation.matchNameToItem(_selectedRecipe));
            if (cookingStation.hasIngredients(recipe.RecipeItemList))
            {
                ButtonCollection = cookingStation.updateButtons(ButtonCollection, 2);
                Rectangles = cookingStation.updateRectangles(Rectangles, 2);
                TextBoxes = cookingStation.updateTextBoxes(TextBoxes, 2);
            }
        }
        
        if (ButtonCollection.isInCollection("Pseudoephedrine") && ButtonCollection["Pseudoephedrine"].isClicked())
        {
            _selectedRecipe = "Pseudoephedrine";
            Recipe recipe = new Recipe(cookingStation.matchNameToItem(_selectedRecipe));
            if (cookingStation.hasIngredients(recipe.RecipeItemList))
            {
                ButtonCollection = cookingStation.updateButtons(ButtonCollection, 2);
                Rectangles = cookingStation.updateRectangles(Rectangles, 2);
                TextBoxes = cookingStation.updateTextBoxes(TextBoxes, 2);
            }
        }
        
        if (ButtonCollection.isInCollection("Red Phosphorus") && ButtonCollection["Red Phosphorus"].isClicked())
        {
            _selectedRecipe = "Red Phosphorus";
            Recipe recipe = new Recipe(cookingStation.matchNameToItem(_selectedRecipe));
            if (cookingStation.hasIngredients(recipe.RecipeItemList))
            {
                ButtonCollection = cookingStation.updateButtons(ButtonCollection, 2);
                Rectangles = cookingStation.updateRectangles(Rectangles, 2);
                TextBoxes = cookingStation.updateTextBoxes(TextBoxes, 2);
            }
        }
        
        if (ButtonCollection.isInCollection("Meth Small") && ButtonCollection["Meth Small"].isClicked())
        {
            _selectedRecipe = "Meth Small";
            Recipe recipe = new Recipe(cookingStation.matchNameToItem(_selectedRecipe));
            if (cookingStation.hasIngredients(recipe.RecipeItemList))
            {
                ButtonCollection = cookingStation.updateButtons(ButtonCollection, 2);
                Rectangles = cookingStation.updateRectangles(Rectangles, 2);
                TextBoxes = cookingStation.updateTextBoxes(TextBoxes, 2);
            }
        }
        
        if (ButtonCollection.isInCollection("Meth") && ButtonCollection["Meth"].isClicked())
        {
            _selectedRecipe = "Meth";
            Recipe recipe = new Recipe(cookingStation.matchNameToItem(_selectedRecipe));
            if (cookingStation.hasIngredients(recipe.RecipeItemList))
            {
                ButtonCollection = cookingStation.updateButtons(ButtonCollection, 2);
                Rectangles = cookingStation.updateRectangles(Rectangles, 2);
                TextBoxes = cookingStation.updateTextBoxes(TextBoxes, 2);
            }
        }
        
        if (ButtonCollection.isInCollection("Cocaine") && ButtonCollection["Cocaine"].isClicked())
        {
            _selectedRecipe = "Cocaine";
            Recipe recipe = new Recipe(cookingStation.matchNameToItem(_selectedRecipe));
            if (cookingStation.hasIngredients(recipe.RecipeItemList))
            {
                ButtonCollection = cookingStation.updateButtons(ButtonCollection, 2);
                Rectangles = cookingStation.updateRectangles(Rectangles, 2);
                TextBoxes = cookingStation.updateTextBoxes(TextBoxes, 2);
            }
        }
        
        if (ButtonCollection.isInCollection("Crack") && ButtonCollection["Crack"].isClicked())
        {
            _selectedRecipe = "Crack";
            Recipe recipe = new Recipe(cookingStation.matchNameToItem(_selectedRecipe));
            if (cookingStation.hasIngredients(recipe.RecipeItemList))
            {
                ButtonCollection = cookingStation.updateButtons(ButtonCollection, 2);
                Rectangles = cookingStation.updateRectangles(Rectangles, 2);
                TextBoxes = cookingStation.updateTextBoxes(TextBoxes, 2);
            }
        }
        
        // Set up MiniGame
        
        if (ButtonCollection.isInCollection("cook") && ButtonCollection["cook"].isClicked())
        {
            ButtonCollection.inMiniGame();
            if (!cookingStation.MiniGame.IsCheating)
            {
                cookingStation.MiniGame.play();
                ButtonCollection = cookingStation.updateButtons(ButtonCollection, 3);
                TextBoxes = cookingStation.updateTextBoxes(TextBoxes, 3);
                Rectangles = cookingStation.updateRectangles(Rectangles, 3);
            }
        }
        
        if (ButtonCollection.isInCollection("cheat") && ButtonCollection["cheat"].isClicked())
        {
            cookingStation.MiniGame.IsCheating = !cookingStation.MiniGame.IsCheating;
            ButtonCollection = cookingStation.updateButtons(ButtonCollection, 2);
        }
        
        if (ButtonCollection.isInCollection("tutorial") && ButtonCollection["tutorial"].isClicked())
        {
            ButtonCollection = cookingStation.updateButtons(ButtonCollection, 4);
        }
        
        // Tutorial Screen
        
        
        
        // Play MiniGame
        
        
        if (ButtonCollection.isInMiniGame())
        {
            if (cookingStation.MiniGame.gameOver())
            { 
                ButtonCollection.inMiniGame();
                Rectangles = cookingStation.updateRectangles(Rectangles, 4);
                TextBoxes = cookingStation.updateTextBoxes(TextBoxes, 3, _selectedRecipe);
                Recipe selectedRecipe = new Recipe(cookingStation.matchNameToItem(_selectedRecipe));
                foreach (var items in selectedRecipe.RecipeItemList)
                {
                    cookingStation.Inventory.removeAmount(items.Item1, items.Item2);
                }
                cookingStation.Inventory.cleanInventory();
                if (cookingStation.MiniGame.gameWon())
                {
                    Item item =  new Item(cookingStation.matchNameToItem(_selectedRecipe));
                    cookingStation.Inventory.addAmount(item, 1);
                    GameManager.ProgressManager.updateStatistics(Statistics.SuccessfulCooking, 1, 1);
                    GameManager.ProgressManager.updateAchievements(Statistics.SuccessfulCooking, Achievement.FirstCraftAndSell);
                }
                else
                {
                    GameManager.ScreenManager.InGameScreen.PlayerState.Health -= 3;
                    GameManager.ScreenManager.InGameScreen.PlayerState.balanceHealth();
                    // implement feedback for health loss
                }
                bool isCheating = cookingStation.MiniGame.IsCheating;
                cookingStation.MiniGame = new MiniGame(GameManager, isCheating);
                return;
            }
            cookingStation.MiniGame.update(gameTime);
            var colorWalt = cookingStation.MiniGame.hasWaltHit() ? Color.DarkGreen : Color.DarkRed;
            var colorJesse = cookingStation.MiniGame.hasJesseHit() ? Color.DarkGreen : Color.DarkRed;
            ButtonCollection = cookingStation.updateButtons(ButtonCollection, 3);
            Rectangles = cookingStation.updateRectangles(Rectangles, 3, colorWalt, colorJesse);
            TextBoxes = cookingStation.updateTextBoxes(TextBoxes, 1);
        }
        
        if (_cooldownMessage > 0)
        {
            _cooldownMessage -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
    }

    public override void draw(GameTime gameTime)
    {
        foreach (var (rectangle, (texture, color)) in Rectangles)
        {
            GameManager.SpriteBatch.Draw(texture, rectangle, color);
        }

        foreach (var textBox in TextBoxes)
        {
            textBox.draw();
        }
        
        ButtonCollection.draw();
        if (!string.IsNullOrEmpty(_message)  && _cooldownMessage > 0)
        {
            var messageLength = GameManager.AssetManager.Fonts["numbers"].MeasureString(_message).X;
            var newPositionX = position.X - messageLength / 2;
            GameManager.SpriteBatch.DrawString(
                GameManager.AssetManager.Fonts["numbers"], _message, new Vector2(newPositionX < 0 ? 0: newPositionX, position.Y + ButtonCollection.Height + 50 * yScale), Color.White);
        }
    }
    
    private void setMessage(string message)
    {
        _message = message;
        _cooldownMessage =1;
    }

    public override void loadContent()
    {
        // nothing to load
    }

    public override void changeResolution()
    {
    }
}
