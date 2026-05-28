using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace breaking_worse.AssetDistribution;

public class AssetManager(GameManager gameManager)
{
    /// <summary>
    /// Loads all content from the Content folder
    /// Saves it in Dictionaries that are similar to the folder structure
    /// </summary>
    public Dictionary<string, Texture2D> Images { get; } = new();
    public Dictionary<string, SpriteFont> Fonts { get; } = new();
    public Dictionary<string, SoundEffect> SoundEffects { get; } = new();
    public Dictionary<string, Song> Music { get; } = new();
    public Dictionary<string, Effect> Effects { get; } = new();
    
    public void loadContent()
    {
        string contentPath;
        if (Directory.Exists(Directory.GetCurrentDirectory() + "/Content/bin/DesktopGL/Content"))
            contentPath = gameManager.Content.RootDirectory + "/bin/DesktopGL/Content";
        else
            contentPath = gameManager.Content.RootDirectory;
        
        foreach (var fileWithPath in Directory.GetFiles(contentPath, "*.*", SearchOption.AllDirectories))
        {
            var relativePath = fileWithPath.Substring(contentPath.Length + 1).Replace("\\", "/");    // relative path with Content as root (not included)
            var folderName = relativePath.Split("/")[0];
            
            switch (folderName)
            {
                case "Images":
                    addTexture(Path.ChangeExtension(relativePath, null));
                    break;
                case "Fonts":
                    addFont(Path.ChangeExtension(relativePath, null));
                    break;
                case "SoundEffects":
                    addSoundEffect(Path.ChangeExtension(relativePath, null));
                    break;
                case "Songs":
                    addMusic(Path.ChangeExtension(relativePath, null));
                    break;
                case "Effects":
                    addEffect(Path.ChangeExtension(relativePath, null));
                    break;
            }
        }
    }

    private void addTexture(string path)
    {
        string key = Path.GetFileName(path);
        Images[key] = gameManager.Content.Load<Texture2D>(path);
    }

    private void addFont(string path)
    {
        string key = Path.GetFileName(path);
        Fonts[key] = gameManager.Content.Load<SpriteFont>(path);
    }

    private void addSoundEffect(string path)
    {
        string key = Path.GetFileName(path);
        SoundEffects[key] = gameManager.Content.Load<SoundEffect>(path);
    }

    private void addMusic(string path)
    {
        string key = Path.GetFileName(path);
        Music[key] = gameManager.Content.Load<Song>(path);
    }

    private void addEffect(string path)
    {
        string key = Path.GetFileName(path);
        Effects[key] = gameManager.Content.Load<Effect>(path);
    }
}
