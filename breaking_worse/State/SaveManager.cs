using System.IO;
using System.Text.Json;
using breaking_worse.Input.Enums;
using breaking_worse.Objects.Items;
using breaking_worse.State.CustomJsonConverters;
using breaking_worse.State.Enums;
using breaking_worse.State.Serializable;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace breaking_worse.State;

public class SaveManager
{
    /// <summary>
    /// Provides framework for saving and loading content by JSON serialization and deserialization
    /// </summary>
    
    private const string SaveFilePath = "/PersistentData/saves/SaveFile_";
    
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        IncludeFields = true,
        WriteIndented = true,
        Converters =
        {
            new DictionaryTKeyEnumTValueConverter(),
            new InputDeviceUserActionTupleConverter(), 
            new KeyConverter(),
            new EnumConverter<Keys>(),
            new EnumConverter<Buttons>(),
            new EnumConverter<MouseButtons>(),
            new EnumConverter<PlayerIndex>(),
            new EnumConverter<Difficulty>(),
            new EnumConverter<NpcType>(),
            new EnumConverter<ItemName>()
        }
    };

    // saves SaveFile at SaveFilePath as serialized numerically ascending .JSON files
    public void writeGameStateToFile(GameState gameState, int id) {
        var serializedText = JsonSerializer.Serialize(gameState, _jsonOptions);
        if (!Directory.Exists(Path.GetFullPath(string.Concat(Directory.GetCurrentDirectory(), SaveFilePath))))
        {
            Directory.CreateDirectory(Path.GetFullPath(string.Concat(Directory.GetCurrentDirectory(), "/PersistentData/saves/")));
        }
        File.WriteAllText(Path.GetFullPath(string.Concat(Directory.GetCurrentDirectory(), SaveFilePath + id + ".json")), serializedText);
    }

    // reads GameState from corresponding save slot (that matches id)
    public GameState readGameStateFromFile(int id)
    {
        var files = readSaveFiles();
        foreach (var fileName in files)
        {
            if (!fileName.EndsWith($"SaveFile_{id}.json")) continue;
            
            var data = File.ReadAllText(fileName);
            return JsonSerializer.Deserialize<GameState>(data, _jsonOptions);
        }
        return null;
    }

    // Reads all files in local save folder in as system paths and passes them on as unsorted list of strings
    // returned field CAN BE EMPTY if no save files are present
    private string[] readSaveFiles() {
        if (!Directory.Exists(Directory.GetCurrentDirectory() + "/PersistentData/saves/"))
        {
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/PersistentData/saves/");
            return [];
        }
        var files = Directory.GetFiles(Directory.GetCurrentDirectory() + "/PersistentData/saves/");
        return files;
    }
    
    // Saves Settings at PersistentData/settings/settings.json 
    public void saveSettingsToFile(SettingsState settingsState) {
        string serializedText = JsonSerializer.Serialize(settingsState, _jsonOptions);
        if (!Directory.Exists(Path.GetFullPath(string.Concat(Directory.GetCurrentDirectory(), "/PersistentData/settings/"))))
        {
            Directory.CreateDirectory(Path.GetFullPath(string.Concat(Directory.GetCurrentDirectory(), "/PersistentData/settings/")));
        }

        if (File.Exists(
                Path.GetFullPath(string.Concat(Directory.GetCurrentDirectory(), "/PersistentData/settings/settings.json"))))
        {
            File.Delete(Path.GetFullPath(string.Concat(Directory.GetCurrentDirectory(), "/PersistentData/settings/settings.json")));
        }
        File.WriteAllText(Path.GetFullPath(string.Concat(Directory.GetCurrentDirectory(), "/PersistentData/settings/settings.json")),
            serializedText);
    }

    // Loads Settings file from local settings folder, parses settings and provides them for further integration
    public SettingsState loadSettingsFromFile() {
        if (!File.Exists(Path.GetFullPath(string.Concat(Directory.GetCurrentDirectory(), "/PersistentData/settings/settings.json"))))
        {
            saveSettingsToFile(SettingsManager.createDefaultSettingsState());
        }
        var savedSettings = File.ReadAllText(Path.GetFullPath(string.Concat(Directory.GetCurrentDirectory(), "/PersistentData/settings/settings.json")));
        return JsonSerializer.Deserialize<SettingsState>(savedSettings, _jsonOptions);
    }
    
    
    // Saves Achievements at PersistentData/achievements/achievements.json 
    public void saveAchievementsToFile(AchievementState achievementState) {
        string serializedText = JsonSerializer.Serialize(achievementState, _jsonOptions);
        if (!Directory.Exists(Path.GetFullPath(string.Concat(Directory.GetCurrentDirectory(), "/PersistentData/achievements/"))))
        {
            Directory.CreateDirectory(Path.GetFullPath(string.Concat(Directory.GetCurrentDirectory(), "/PersistentData/achievements/")));
        }

        if (File.Exists(
                Path.GetFullPath(string.Concat(Directory.GetCurrentDirectory(), "/PersistentData/achievements/achievements.json"))))
        {
            File.Delete(Path.GetFullPath(string.Concat(Directory.GetCurrentDirectory(), "/PersistentData/achievements/achievements.json")));
        }
        File.WriteAllText(Path.GetFullPath(string.Concat(Directory.GetCurrentDirectory(), "/PersistentData/achievements/achievements.json")),
            serializedText);
    }
    
    // Loads Achievements file from local achievements folder
    public AchievementState loadAchievemntsFromFile() {
        if (!File.Exists(Path.GetFullPath(string.Concat(Directory.GetCurrentDirectory(), "/PersistentData/achievements/achievements.json"))))
        {
            saveAchievementsToFile(ProgressManager.createDefaultAchievementState());
        }
        var savedAchievements = File.ReadAllText(Path.GetFullPath(string.Concat(Directory.GetCurrentDirectory(), "/PersistentData/achievements/achievements.json")));
        return JsonSerializer.Deserialize<AchievementState>(savedAchievements, _jsonOptions);
    }
}
