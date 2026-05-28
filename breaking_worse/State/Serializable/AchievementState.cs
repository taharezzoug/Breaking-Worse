using System.Text.Json.Serialization;

namespace breaking_worse.State.Serializable;

public class AchievementState
{
    [JsonPropertyName("achievementSteps")]
    public bool AchievementSteps { get; set; }
    
    [JsonPropertyName("achievementCraftAndSell")]
    public bool AchievementCraftAndSell { get; set; }
    
    [JsonPropertyName("achievementMoney")]
    public bool AchievementMoney { get; set; }
    
    [JsonPropertyName("achievementRecipes")]
    public bool AchievementRecipes { get; set; }
    
    [JsonPropertyName("achievementJail")]
    public bool AchievementJail { get; set; }
    
    [JsonPropertyName("achievementDrugs")]
    public bool AchievementDrugs { get; set; }
    
    [JsonPropertyName("achievementIngredients")]
    public bool AchievementIngredients { get; set; }
    
    [JsonPropertyName("achievementWanted")]
    public bool AchievementWanted { get; set; }
    
    [JsonPropertyName("achievementKills")]
    public bool AchievementKills { get; set; }
    
    [JsonPropertyName("achievementBushes")]
    public bool AchievementBushes { get; set; }
    
    [JsonPropertyName("achievementWin")]
    public bool AchievementWin { get; set; }
}
