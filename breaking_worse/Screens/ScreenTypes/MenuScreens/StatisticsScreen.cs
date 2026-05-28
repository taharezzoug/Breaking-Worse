using System.Collections.Generic;
using breaking_worse.Objects.Player;
using breaking_worse.Screens.Clickables;
using breaking_worse.State.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace breaking_worse.Screens.ScreenTypes.MenuScreens;

public class StatisticsScreen : AMenuScreen
{
    /// <summary>
    ///     Screen for the statistics screen (shows all statistics)
    ///     Creates the layout for the screen objects (button, texBoxes, headline)
    ///     Definition of actions if a button in the screen is clicked
    /// </summary>
    /// 

    private readonly Dictionary<string, string> _statisticsData;
    private readonly SpriteFont _font;

    public StatisticsScreen(GameManager gameManager, bool isMenuStats = false) : base(gameManager)
    {
        ButtonCollection = new ButtonCollection(GameManager, new Vector2(GameManager.SettingsManager.Resolution.Width / 2f, 800), [PlayerCharacterId.Walt, PlayerCharacterId.Jesse]);
        createMenuElements();
        GameManager.ProgressManager.loadGlobalStatistics();
        _statisticsData = new Dictionary<string, string>
        {
            { "Total of taken steps:", GameManager.ProgressManager.getStatistics(Statistics.StepsTaken, isMenuStats).ToString() },
            { "Total of recipes unlocked:", GameManager.ProgressManager.getStatistics(Statistics.RecipesUnlocked, isMenuStats).ToString() },
            { "Total of drugs sold:", GameManager.ProgressManager.getStatistics(Statistics.DrugsSell, isMenuStats).ToString() },
            { "Total of successful cooking:", GameManager.ProgressManager.getStatistics(Statistics.SuccessfulCooking, isMenuStats).ToString() },
            { "Total of damage taken:", GameManager.ProgressManager.getStatistics(Statistics.DamageTaken, isMenuStats).ToString() },
            { "Total of received money:", GameManager.ProgressManager.getStatistics(Statistics.ReceivedMoney, isMenuStats).ToString() },
            { "Time since last arrest:", GameManager.ProgressManager.getStatistics(Statistics.LastArrest,isMenuStats).ToString() },
            { "Total of cartel members killed:", GameManager.ProgressManager.getStatistics(Statistics.CartelKilled, isMenuStats).ToString() },
            { "Highest wanted level:", GameManager.ProgressManager.getStatistics(Statistics.HighestWantedLevel, isMenuStats).ToString() },
            { "Total of hospital visits:", GameManager.ProgressManager.getStatistics(Statistics.HospitalVisits,isMenuStats).ToString() }
        };
        _font = GameManager.AssetManager.Fonts["gabriele"];
    }

    protected sealed override void createMenuElements()
    {
        createHeadline("Statistics");
        ButtonCollection.addButton("return", "return");
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

    var scale = GameManager.SettingsManager.Resolution.Height * 2f / 1040;

    // Define the positions
    var startX = GameManager.SettingsManager.Resolution.Width * 0.25f; // Left margin
    float endX = GameManager.SettingsManager.Resolution.Width * 0.75f; // Right margin
    float startY = GameManager.SettingsManager.Resolution.Height * 0.2f; // Top margin
    float lineHeight = _font.MeasureString("Test").Y * scale; // Line height with spacing
    
    drawBackgroundImage((int)startX - 100, (int)(startY - 2 * lineHeight), (int)(endX - startX) + 150, (int)lineHeight * 14);
    
    int lineIndex = 0;
    foreach (var entry in _statisticsData)
    {
        Vector2 leftPosition = new Vector2(startX, startY + lineHeight * lineIndex);
        
        float numberWidth = _font.MeasureString(entry.Value).X * scale;
        Vector2 rightPosition = new Vector2(endX - numberWidth, startY + lineHeight * lineIndex);

        GameManager.SpriteBatch.DrawString(_font, entry.Key, leftPosition, Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f); // Left-aligned text, scaled
        GameManager.SpriteBatch.DrawString(_font, entry.Value, rightPosition, Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f); // Right-aligned text, scaled
        
        lineIndex++;
    }
}

    private void drawBackgroundImage(int startX, int startY, int width, int height)
    {
        Rectangle backgroundRectangle = new Rectangle(startX, startY, width, height + 10);
        GameManager.SpriteBatch.Draw(GameManager.AssetManager.Images["BackgroundPaper"], backgroundRectangle, Color.White);
    }


    public override void changeResolution()
    {
        yScale = GameManager.SettingsManager.Resolution.Height / 1080f;
        ButtonCollection = new ButtonCollection(GameManager, new Vector2(GameManager.SettingsManager.Resolution.Width / 2f, 800), [PlayerCharacterId.Walt, PlayerCharacterId.Jesse]);
        createMenuElements();
    }
}