using System;
using System.Collections.Generic;
using breaking_worse.Objects.Items;
using breaking_worse.Objects.Player;
using breaking_worse.Screens.Clickables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace breaking_worse.Screens.ScreenTypes.InGameScreens;

public class InventoryScreen: AScreen
{
    private GameManager _gameManager;
    private Inventory _inventory;
    private readonly Texture2D _texture;
    private ButtonCollection _recipes;
    
    private int _originalScreenWidth;
    
    public InventoryScreen(GameManager gameManager, Inventory inventory, List<PlayerCharacterId> correspondingPlayers) : base(gameManager, true, true)
    {
        _gameManager = gameManager;
        _inventory = inventory;
        _texture = _gameManager.AssetManager.Images["DialogBackground"];
        _originalScreenWidth = GameManager.SettingsManager.Resolution.Width;
        
        _recipes = new ButtonCollection(_gameManager, new Vector2((int)(_gameManager.SettingsManager.Resolution.Width / 2f - 400 * xScale), 275), correspondingPlayers,250, 60, 70);

        GameManager.ScreenManager.InGameScreen.PlayerState.Inventory.changeRecipe(GameManager.AssetManager.Images["recipeMethSmall"]);

        initializeRecipes();
    }

    private void initializeRecipes()
    {
        _recipes.addButton("MethSmall", "Meth Small", 0.6f);
        _recipes.addButton("Meth", "Meth", 0.6f);
        _recipes.addButton("Cocaine", "Cocaine", 0.6f);
        _recipes.addButton("Crack", "Crack", 0.6f);
        _recipes.addButton("Others", "Others", 0.6f);
    }
    
    public override void loadContent()
    {
        //nothing to load
    }
    public override void draw(GameTime gameTime)
    {
        int iconSize = (int)(32 * Scale);
        int rowHeight = (int)(iconSize * 1.4);
        int startX = (int)(GameManager.SettingsManager.Resolution.Width / 2f - 200 * Scale);
        int startY = (int)(100 * yScale);

        int currentRow = 0;

        foreach (ItemName itemName in Enum.GetValues(typeof(ItemName)))
        {
            if (itemName == ItemName.Hp || itemName == ItemName.CancerTreatment) break;

            int x = startX;
            int y = startY + rowHeight * currentRow;
            int boxWidth = (int)(400 * xScale);
            int boxHeight = (int)(iconSize + 10 * yScale);
            
            var amount = 0;
            var isInInventory = false;
            if (_inventory.Items.Count > 0 && _inventory.Items.ContainsKey(itemName))
            {
                amount = _inventory.Items[itemName];
                isInInventory = _inventory.Items[itemName] > 0;
            }

            var icon = GameManager.AssetManager.Images[itemName.ToString()];
            
            Color itemColor = isInInventory ? Color.White : new Color(100, 100, 100, 150);
            Color boxColor = isInInventory ? new Color(50, 50, 50, 200) : new Color(30, 30, 30, 150);
            Color borderColor = isInInventory ? Color.Gray : new Color(80, 80, 80, 150);
            
            if (isInInventory && (itemName == ItemName.MethSmall || itemName == ItemName.Meth || itemName == ItemName.Crack || itemName == ItemName.Cocaine))
            {
                borderColor = Color.Gold;
                itemColor = Color.Gold;
            }
            
            GameManager.SpriteBatch.Draw(_texture, new Rectangle(x, y, boxWidth, boxHeight), boxColor);
            
            DrawRectangle(x, y, boxWidth, boxHeight, borderColor);
            
            GameManager.SpriteBatch.Draw(icon, new Rectangle(x + 10, y + 5, iconSize, iconSize), itemColor);
            
            GameManager.SpriteBatch.DrawString(
                GameManager.AssetManager.Fonts["numbers"], 
                itemName.ToString(), 
                new Vector2(x + iconSize + 20 * xScale, y + iconSize / 4f), 
                isInInventory ? Color.White : Color.Gray, 0f, Vector2.Zero, 0.5f * Scale, SpriteEffects.None, 0f
            );
            
            GameManager.SpriteBatch.DrawString(
                GameManager.AssetManager.Fonts["numbers"], 
                amount.ToString(), 
                new Vector2(x + iconSize + 300 * xScale, y + iconSize / 4f), 
                isInInventory ? Color.LightGreen : Color.Gray, 
                0f, Vector2.Zero, 0.5f * Scale, SpriteEffects.None, 0f
            );

            currentRow++;
        }
        _recipes.draw();
    }
    
private void DrawRectangle(int x, int y, int width, int height, Color color)
    {
        Texture2D rect = new Texture2D(GameManager.GraphicsDeviceManager.GraphicsDevice, 1, 1);
        rect.SetData(new[] { color });

        int thickness = 3;
        
        GameManager.SpriteBatch.Draw(rect, new Rectangle(x, y, width, thickness), color);
        GameManager.SpriteBatch.Draw(rect, new Rectangle(x, y + height - thickness, width, thickness), color);
        GameManager.SpriteBatch.Draw(rect, new Rectangle(x, y, thickness, height), color);
        GameManager.SpriteBatch.Draw(rect, new Rectangle(x + width - thickness, y, thickness, height), color);
    }
    public override void update(GameTime gameTime)
    {
        _recipes.update();
        
        if (_originalScreenWidth != GameManager.SettingsManager.Resolution.Width)
        {
            xScale = GameManager.GraphicsDeviceManager.GraphicsDevice.Viewport.Width / 1920f;
            yScale = GameManager.GraphicsDeviceManager.GraphicsDevice.Viewport.Height / 1080f;
            Scale = Math.Min(xScale, yScale);
            
            _originalScreenWidth = GameManager.SettingsManager.Resolution.Width;
            _recipes = new ButtonCollection(_gameManager, new Vector2(_gameManager.SettingsManager.Resolution.Width / 2f - 400 * xScale, 275), [PlayerCharacterId.Walt, PlayerCharacterId.Jesse],250, 60, 70);
            initializeRecipes();
        }
        
        if (_recipes["MethSmall"].isClicked())
            GameManager.ScreenManager.InGameScreen.PlayerState.Inventory.changeRecipe(GameManager.AssetManager.Images["recipeMethSmall"]);
        if (_recipes["Meth"].isClicked())
            GameManager.ScreenManager.InGameScreen.PlayerState.Inventory.changeRecipe(GameManager.AssetManager.Images["recipeMeth"]);
        if (_recipes["Cocaine"].isClicked())
            GameManager.ScreenManager.InGameScreen.PlayerState.Inventory.changeRecipe(GameManager.AssetManager.Images["recipeCocaine"]);
        if (_recipes["Crack"].isClicked())
            GameManager.ScreenManager.InGameScreen.PlayerState.Inventory.changeRecipe(GameManager.AssetManager.Images["recipeCrack"]);
        if (_recipes["Others"].isClicked())
            GameManager.ScreenManager.InGameScreen.PlayerState.Inventory.changeRecipe(GameManager.AssetManager.Images["recipeOther"]);

    }
    
    public override void changeResolution() { }
}
