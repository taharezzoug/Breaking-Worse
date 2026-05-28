using System;
using breaking_worse.Objects.Player;
using breaking_worse.Screens.Clickables;
using breaking_worse.State.Enums;
using Microsoft.Xna.Framework;
namespace breaking_worse.Screens.ScreenTypes.MenuScreens;

public class AchievementsScreen : AMenuScreen
{
    /// <summary>
    /// Screen for Achievements (shows achievements)
    /// Creates the layout for the screen objects (button, texBoxes, headline)
    /// Definition of actions if a button in the screen is clicked
    /// </summary>

    public AchievementsScreen(GameManager gameManager) : base(gameManager)
    {
        ButtonCollection = new ButtonCollection(GameManager, new Vector2(GameManager.SettingsManager.Resolution.Width * (2f/6f), 200), [PlayerCharacterId.Walt, PlayerCharacterId.Jesse]);
        createMenuElements();
    }

    protected sealed override void createMenuElements()
    {
        createHeadline("achievements");

        ButtonCollection.addAchievement("FirstSteps", "Baby Steps:\n Take your first 100 Steps ", 600);
        ButtonCollection.addAchievement("FirstCraftAndSell","New Sprout in Town:\n Craft and sell for the first ", 600);
        ButtonCollection.addAchievement("FirstXMoney","We Banking:\n Make your first 1,000,000 Dollar ", 600);
        ButtonCollection.addAchievement("AllRecipes","Walter Who?:\n Unlock all the recipes ", 600);
        ButtonCollection.addAchievement("FirstJail","Don't Drop the Soap:\n Land for the first time in jail ", 600);
        ButtonCollection.addAchievement("XDrugSell","So Blue. . .:\n Deliver 1,000kg of pure meth ", 600);
        ButtonCollection.addAchievementAtPosition("XIngredients","Scavenger:\n Find 10 different ingredients ", 600, new Vector2(GameManager.SettingsManager.Resolution.Width * (4f/6f) + ButtonCollection.ButtonHeight, 200));
        ButtonCollection.addAchievement("MostWanted","Most Wanted:\n Reach a 100% wanted level ", 600);
        ButtonCollection.addAchievement("KillXCartel","Sharp Shooter:\n Take down 100 cartel members ", 600); 
        ButtonCollection.addAchievement("HideXBushes","Chill Spot:\n Successfully hide in 5 bushes ", 600);
        ButtonCollection.addAchievement("FirstWin","Cancer Free:\n Complete the game the first time ", 600); 
        ButtonCollection.addButtonAtPosition("return", "return", new Vector2(GameManager.SettingsManager.Resolution.Width / 2f, 900));
    }

    public override void update(GameTime gameTime)
    {
        base.update(gameTime);
        if (ButtonCollection["return"].isClicked())
            GameManager.ScreenManager.removeFromStack(this);
    }

    public override void draw(GameTime gameTime)
    {
        base.draw(gameTime);
        
        
        foreach (Achievement achievement in Enum.GetValues(typeof(Achievement)))
        {
            string achievementName = Enum.GetName(typeof(Achievement), achievement);
            var button = ButtonCollection[achievementName];
            if (achievementName != null)
            {
                var texture =  GameManager.ProgressManager.Achievments[achievement] ? GameManager.AssetManager.Images[achievementName] : GameManager.AssetManager.Images["Default"];
                GameManager.SpriteBatch.Draw(texture,
                    new Rectangle(button.HitBox.X - ButtonCollection.ButtonHeight - 10, button.HitBox.Y, ButtonCollection.ButtonHeight, ButtonCollection.ButtonHeight), Color.White);
                if (GameManager.ProgressManager.Achievments[achievement])
                {
                    button.IsUnlocked = true;
                }
            }
        }
    }

    public override void changeResolution()
    {
        yScale = GameManager.SettingsManager.Resolution.Height / 1080f;
        ButtonCollection = new ButtonCollection(GameManager, new Vector2(GameManager.SettingsManager.Resolution.Width * (2f/6f), 200), [PlayerCharacterId.Walt, PlayerCharacterId.Jesse]);
        createMenuElements();
    }
}
