using System;
using System.Collections.Generic;
using System.Linq;
using breaking_worse.Objects.Player;
using breaking_worse.Screens.Clickables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace breaking_worse.Screens.ScreenTypes.MenuScreens;

public class TutorialScreen : AMenuScreen
{
    
    private readonly List<Texture2D> _backgroundImages;
    private string[] _texts; // Holds the tutorial text for each slide
    private readonly List<List<Texture2D>> _icons;
    private readonly GameManager _gameManager;
    private readonly BackgroundScreen _backgroundScreen;
    
    private int _screenIndex;
    private readonly Texture2D _solidTexture; // Texture for semi-transparent background behind text


    
    private readonly SpriteFont _font;
    private readonly float _scale;
    public TutorialScreen(GameManager gameManager) : base(gameManager)
    {
        _gameManager = gameManager;

        _icons = new List<List<Texture2D>>();
        _backgroundImages = new List<Texture2D>();
        _solidTexture = createSolidColorTexture(_gameManager.SpriteBatch.GraphicsDevice, new Color(50, 50, 50, 200));
        
        _backgroundScreen = new BackgroundScreen(_gameManager, _gameManager.AssetManager.Images["waltAndJesse"]);
        _gameManager.ScreenManager.addScreen(_backgroundScreen);
        
        _font = _gameManager.AssetManager.Fonts["gabriele"];
        _font = _gameManager.AssetManager.Fonts["gabriele"];
        _scale = 3f * Scale;
        
        //ButtonCollection.Position = new Vector2(_screenWidth - ButtonCollection.ButtonWidth, _screenHeight - 50 - ButtonCollection.ButtonHeight);
        ButtonCollection = new ButtonCollection(GameManager, new Vector2(GameManager.SettingsManager.Resolution.Width * (1f/6f) - 30 * xScale, 950), [PlayerCharacterId.Walt, PlayerCharacterId.Jesse]);
        createMenuElements();
        
        loadContent();
    }

    protected sealed override void createMenuElements()
    {
        ButtonCollection.addButton("enter", "Continue");
        ButtonCollection.addButtonAtPosition("skip", "skip", new Vector2(GameManager.SettingsManager.Resolution.Width * (5f/6f) + 30 * xScale, 950));
    }

    public override void update(GameTime gameTime)
    {
        base.update(gameTime);
        
        // Close the tutorial when all slides are viewed
        if (_screenIndex > _texts.Length - 1)
        {
            _gameManager.ScreenManager.removeFromStack(this, 1, true, false);
        }

        if (ButtonCollection["enter"].isClicked())
        {
            _screenIndex++;
            _backgroundScreen.BackGroundTexture = _backgroundImages[_screenIndex];
        }
        if (ButtonCollection["skip"].isClicked())
        {
            _gameManager.ScreenManager.removeFromStack(this, 1, true, false);
        }
    }


    public override void draw(GameTime gameTime)
    {
        if (_screenIndex >= _texts.Length) return;
        ButtonCollection.draw();
        
        float boxWidth = _gameManager.SettingsManager.Resolution.Width * 0.8f; 
        float boxHeight = _gameManager.SettingsManager.Resolution.Height * 0.8f;
        Vector2 position = new Vector2((_gameManager.SettingsManager.Resolution.Width - boxWidth) / 2, (_gameManager.SettingsManager.Resolution.Height - boxHeight) / 2);

        string wrappedText = wrapText(_font, _texts[_screenIndex], boxWidth);

        float textHeight = _font.MeasureString(wrappedText).Y * _scale;

        float padding = 20f;

        Rectangle textBackground = new Rectangle(
            (int)(position.X - padding),
            (int)(position.Y - padding),
            (int)(boxWidth + 2 * padding),
            (int)(textHeight + 2 * padding)
        );
        
        _gameManager.SpriteBatch.Draw(_solidTexture, textBackground, Color.White);
        _gameManager.SpriteBatch.DrawString(_font, wrappedText, position, Color.White, 0f, Vector2.Zero, _scale, SpriteEffects.None, 0f);

        drawIcons(_icons[_screenIndex], (int)(200 * Scale), (int)(20 * Scale), 0.5f);
    }


    public sealed override void loadContent()
    {
        loadTexts();
        loadIcons();
        loadBackground();
    }

    // Splits the tutorial text into slides based on the "+" delimiter
    private void loadTexts()
    {
        string dialogs =
            "White: Alright, Jesse. Listen up. We've got no time for screw-ups. This isn't just about making some cash. This is survival. You learn to work - my way.\n\nJesse: Yo, I didn't sign up for another lecture. Just tell me what to do, Mr. White!" +
            "+White: First things first - move. You're not some slacker standing around in the corner.\n\nJesse: Yeah, yeah. Look at me. I'm dancing, yo. A bit like in a video-game where we move with the keys or with a joystick!" +
            "+White: This lab isn't just for show. We gotta walk up to the workstation and press the interact button. That's how you interact. Use it to cook, to buy, to sell. It's not just a button. Keep an eye on your inventory aswell! - it's the key to making this work.\n\nJesse: Yes! Science Bitch!\n\nWhite: Keep up the jokes, Jesse. I'll find another partner." +
            "+White: Cooking isn't easy, remember the chemistry class?\n\nJesse: Hell yeah! Things go in stuff...\n\nWhite: *Sighs* It's a process and see it a bit like a MiniGame, the execution must be perfect...\n\nJesse: Got it!" +
            "+Jesse: What are these papers about?\n\nWalter: Those are the recipes for the good shit we'll make. It shows the ingredients. You can get those by the shady sellers all around.\n\nJesse: Last time I also found some weird chems in the trash cans... Had to to hit it by accident though."+
            "+White: When you've got a batch ready, it's time to sell. Find a buyer in town. Approach them and press the interaction key. They'll make an offer. You decide if it's worth it - but remember, every deal carries risk.\n\nJesse: Sweet. More cash, less problems.\n\nWhite: No, Jesse. More cash means more problems. The police, the cartel - they don't care about your aspirations." +
            "+Jesse: Yo, who's that? The cops?!\n\nWhite: Calm down! You have a gun and your fists to deal with that. You can switch between them but be careful who you try to get rid of...\n\nJesse: What the hell, Mr. White?! This ain't a war zone!\n\nWhite: It is now. Welcome to the big leagues." +
            "+White: If the heat's too much, you hide. Find a bush or an alley. Stand near it and interact with it. But remember, if they see you go in, it's game over.\n\nJesse: Hiding in bushes? What is this, a nature hike?\n\nWhite: You want to survive, Jesse? Then adapt." +
            "+White: This isn't just a game, Jesse. Every minute we waste, I lose time. We need the money for the cure - before the cancer wins.\n\nJesse: Yeah, yeah. Let's cook, sell, and save your sorry ass." +
            "+You've entered a world of crime, desperation, and survival. Time is your greatest enemy. Will you build an empire... or watch it crumble? The choice is yours.";
        _texts = dialogs.Split("+");
    }

    private void loadIcons()
    {
        _icons.Add([]);
        _icons.Add([_gameManager.AssetManager.Images["Walt"],
                    _gameManager.AssetManager.Images["Jesse"]]);
        _icons.Add([_gameManager.AssetManager.Images["Crack"],
                    _gameManager.AssetManager.Images["CookingStation1"],
                    _gameManager.AssetManager.Images["Cocaine"]]);
        _icons.Add([]);
        _icons.Add([_gameManager.AssetManager.Images["Trader"],
                    _gameManager.AssetManager.Images["recipeCocaine"],
                    _gameManager.AssetManager.Images["recipeCrack"],
                    _gameManager.AssetManager.Images["Trash"]]);
        _icons.Add([_gameManager.AssetManager.Images["Addict1"],
                    _gameManager.AssetManager.Images["Addict2"],
                    _gameManager.AssetManager.Images["Addict3"],
                    _gameManager.AssetManager.Images["Addict4"]]);
        _icons.Add([_gameManager.AssetManager.Images["Police"],
                    _gameManager.AssetManager.Images["Cartel1"],]);
        _icons.Add([_gameManager.AssetManager.Images["Bush"]]);
        _icons.Add([_gameManager.AssetManager.Images["Hospital"]]);
        _icons.Add([]);
        _icons.Add([]);
        _icons.Add([]);
    }

    private void loadBackground()
    {
        _backgroundImages.Add(_gameManager.AssetManager.Images["waltAndJesse"]);
        _backgroundImages.Add(_gameManager.AssetManager.Images["waltAndJesse"]);
        _backgroundImages.Add(_gameManager.AssetManager.Images["CookingStationBackground"]);
        _backgroundImages.Add(_gameManager.AssetManager.Images["CookingStationBackground"]);
        _backgroundImages.Add(_gameManager.AssetManager.Images["CookingStationBackground"]);
        _backgroundImages.Add(_gameManager.AssetManager.Images["waltAndJesse"]);
        _backgroundImages.Add(_gameManager.AssetManager.Images["hank"]);
        _backgroundImages.Add(_gameManager.AssetManager.Images["hank"]);
        _backgroundImages.Add(_gameManager.AssetManager.Images["waltAndJesse"]);
        _backgroundImages.Add(_gameManager.AssetManager.Images["waltAndJesse"]);
        _backgroundImages.Add(_gameManager.AssetManager.Images["waltAndJesse"]);
        _backgroundImages.Add(_gameManager.AssetManager.Images["waltAndJesse"]);
        _backgroundImages.Add(_gameManager.AssetManager.Images["waltAndJesse"]);
    }
    
    // Wraps text to fit within a specified width
    private string wrapText(SpriteFont font, string text, float maxLineWidth)
    {
        string[] words = text.Split(' '); 
        string wrappedText = "";
        string line = "";

        foreach (string word in words)
        {
            string testLine = line.Length == 0 ? word : line + " " + word;
            float lineWidth = font.MeasureString(testLine).X * _scale;

            if (lineWidth <= maxLineWidth)
            {
                line = testLine;
            }
            else
            {
                wrappedText += line + "\n";
                line = word;
            }
        }

        if (line.Length > 0)
        {
            wrappedText += line;
        }

        return wrappedText;
    }
    
    private void drawIcons(List<Texture2D> icons, int targetHeight, int spacing, float bottomPaddingFactor)
    {
        if (icons.Count <= 0) return;
        
        int totalWidth = 0;

        var iconWidths = new List<int>();
        foreach (var scaledWidth in icons.Select(icon => (float)icon.Width / icon.Height).Select(aspectRatio => (int)(targetHeight * aspectRatio)))
        {
            iconWidths.Add((int)(scaledWidth));
            totalWidth += scaledWidth;
        }

        totalWidth += (icons.Count - 1) * spacing; 
        
        int startX = (_gameManager.SettingsManager.Resolution.Width - totalWidth) / 2;

        int yPosition = _gameManager.SettingsManager.Resolution.Height - targetHeight - (int)(targetHeight * bottomPaddingFactor);

        for (int i = 0; i < icons.Count; i++)
        {
            int xPosition = startX;

            _gameManager.SpriteBatch.Draw(icons[i], 
                new Rectangle(xPosition, yPosition, iconWidths[i], targetHeight), 
                Color.White);

            startX += iconWidths[i] + spacing;
        }
    }
    
    
    // Creates a single-pixel solid texture of a given color
    private Texture2D createSolidColorTexture(GraphicsDevice graphicsDevice, Color color)
    {
        var texture = new Texture2D(graphicsDevice, 1, 1); 
        texture.SetData(new[] { color });
        return texture;
    }
    
    public override void changeResolution()
    {        
        xScale = GameManager.GraphicsDeviceManager.GraphicsDevice.Viewport.Width / 1920f;
        yScale = GameManager.SettingsManager.Resolution.Height / 1080f;
        Scale = Math.Min(xScale, yScale);
        ButtonCollection = new ButtonCollection(GameManager, new Vector2(GameManager.SettingsManager.Resolution.Width * (5f/6f) - 30, 900), [PlayerCharacterId.Walt, PlayerCharacterId.Jesse]);
        createMenuElements();
    }

}