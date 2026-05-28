using System.Collections.Generic;
using System.Text.Json.Serialization;
using breaking_worse.Objects.Items;

namespace breaking_worse.State.Serializable;

public struct SavedPlayerState
{

    /// <summary>
    /// Makes PlayerState class serializable for saving Process
    /// </summary>

    [JsonPropertyName("Timer")]
    public int Timer { get; set; } = 0;
    
    [JsonPropertyName("Health")]
    public int Health { get; set; } = 0;
    
    [JsonPropertyName("SuspiciousActivityCounter")]
    public int SuspiciousActivityCounter { get; set; } = 0;
    
    [JsonPropertyName("Arrests")]
    public int Arrests { get; set; } = 0;
    
    [JsonPropertyName("Deaths")]
    public int Deaths { get; set; } = 0;
    
    [JsonPropertyName("Items")]
    public Dictionary<ItemName, int> Items { get; set; }
    
    [JsonPropertyName("Money")]
    public int Money { get; set; } = 0;

    [JsonPropertyName("CartelAggro")]
    public bool WillShoot { get; set; } = false;
    
    [JsonPropertyName("CartelAggroCooldown")]
    public float AggroCooldown { get; set; } = 0;
    
    [JsonPropertyName("RecipeInHud")]
    public string RecipeInHud { get; set; } = "";
    

    public SavedPlayerState(int timer, int health, int suspiciousActivityCounter, int arrests, int deaths, Dictionary<ItemName, int> items, int money, bool cartelAggro, float aggroCooldown, string recipeInHud)
    {
        Timer = timer;
        Health = health;
        SuspiciousActivityCounter = suspiciousActivityCounter;
        Arrests = arrests;
        Deaths = deaths;
        Items = items;
        Money = money;
        WillShoot = cartelAggro;
        AggroCooldown = aggroCooldown;
        RecipeInHud = recipeInHud;
    }
}