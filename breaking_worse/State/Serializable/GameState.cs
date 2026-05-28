using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace breaking_worse.State.Serializable;

public class GameState
{
    /// <summary>
    /// Contains all Variables which are saved. Class is serialized 
    /// into a .json file by SaveManager. This file will then again
    /// be deserialized by SaveManager.
    /// </summary>
    
    // GameObjects -----------------------------------------------------

    [JsonPropertyName("SavedGameObjects")]
    public List<SavedGameObject> SavedGameObjects { get; set; }
    
    [JsonPropertyName("ReSpawnTimes")]
    public Dictionary<int, double> ReSpawnTimes { get; set; }
    
    [JsonPropertyName("SavedPickables")]
    public List<SavedPickable> SavedPickables { get; set; }
    
    [JsonPropertyName("SavedPlayerState")]
    public SavedPlayerState SavedPlayerState { get; set; }
    
    // Statistics------------------------------------------------
    [JsonPropertyName("statisticsSteps")]
    public int StatisticsSteps { get; set; }
    
    [JsonPropertyName("statisticsRecipes")]
    public int StatisticsRecipes { get; set; }
    
    [JsonPropertyName("statisticsDrugs")]
    public int StatisticsDrugs { get; set; }
    
    [JsonPropertyName("statisticsCooking")]
    public int StatisticsCooking { get; set; }
    
    [JsonPropertyName("statisticsDamage")]
    public int StatisticsDamage { get; set; }
    
    [JsonPropertyName("statisticsMoney")]
    public int StatisticsMoney { get; set; }
    
    [JsonPropertyName("statisticsArrest")]
    public int StatisticsArrest { get; set; }
    
    [JsonPropertyName("statisticsKills")]
    public int StatisticsKills { get; set; }
    
    [JsonPropertyName("statisticsWantedLevel")]
    public int StatisticsWantedLevel { get; set; }
    
    [JsonPropertyName("statisticsHospital")]
    public int StatisticsHospital { get; set; }
    
    [JsonPropertyName("statisticsIngredients")]
    public int StatisticsIngredients { get; set; }
    
    [JsonPropertyName("statisticsBushes")]
    public int StatisticsBushes { get; set; }
    
    [JsonPropertyName("statisticsJail")]
    public int StatisticsJail { get; set; }
    
    [JsonPropertyName("statisticsWin")]
    public int StatisticsWin { get; set; }
}
