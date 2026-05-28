using System;
using System.Collections.Generic;
using System.Linq;
using breaking_worse.State.Enums;
using breaking_worse.State.Serializable;

namespace breaking_worse.State;

public class ProgressManager
{
    /// <summary>
    /// Manager to track and save Statistics and Achievements
    /// Has Methods to update, get, save and load the values of the statistics and achievements
    /// </summary>
    
    private readonly Dictionary<Statistics, (int, int)> _statistics;
    // Global statistics dictionary, aggregates values across all game states
    private Dictionary<Statistics, int> _globalStatistics;
    private readonly Dictionary<Achievement, bool> _achievements;
    private int _previousValue;
    private readonly GameManager _gameManager;
    
    public ProgressManager(GameManager gameManager)
    {
        _gameManager = gameManager;
        _statistics = new Dictionary<Statistics, (int, int)>();
        _globalStatistics = new Dictionary<Statistics, int>();
        _achievements = new Dictionary<Achievement, bool>();
        AchievementState achievementState = gameManager.SaveManager.loadAchievemntsFromFile();
        
        initializeStatistics();
        loadAchievements(achievementState);
    }

    private void initializeStatistics()
    {
        _statistics.Add(Statistics.StepsTaken, (0, 100));//
        _statistics.Add(Statistics.RecipesUnlocked, (3, 7));//
        _statistics.Add(Statistics.DrugsSell, (0, 1000));//
        _statistics.Add(Statistics.SuccessfulCooking, (0, 1));//
        _statistics.Add(Statistics.DamageTaken, (0, 0)); // All taken damage
        _statistics.Add(Statistics.ReceivedMoney, (0, 1000000));//
        _statistics.Add(Statistics.LastArrest, (0, 0));
        _statistics.Add(Statistics.CartelKilled, (0, 100));//
        _statistics.Add(Statistics.HighestWantedLevel, (0, 0));//
        _statistics.Add(Statistics.HospitalVisits, (0, 0));//
        
        _statistics.Add(Statistics.IngredientsCollected, (0, 10));//
        _statistics.Add(Statistics.BushesHidden, (0, 10));//
        _statistics.Add(Statistics.TimesJail, (0, 1)); //
        _statistics.Add(Statistics.TimesWin, (0, 1));   // TODO: implement when cancer treatment is implemented
        
        _globalStatistics = _statistics.Keys.ToDictionary(key => key, key => 0);
    }

    public void resetStats()
    {
        foreach (var key in _globalStatistics.Keys.ToList())
        {
            _statistics[key] = (0, _statistics[key].Item2);
        }
        _statistics[Statistics.RecipesUnlocked] = (3, _statistics[Statistics.RecipesUnlocked].Item2);
        _previousValue = 0;
    }
    
    
    public void updateStatistics(Statistics statistics, int newValue, int _case)
    {
        int value = _statistics[statistics].Item1;
        switch(_case)
        {
            case 0:        // only add absolute value
                    value += newValue - _previousValue;
                    _statistics[statistics] = (value, _statistics[statistics].Item2);
                    _previousValue = newValue;
                break;
            case 1:     // add value
                value += newValue;
                _statistics[statistics] = (value, _statistics[statistics].Item2);
                break;
            case 2:    // replace value
                _statistics[statistics] = (newValue, _statistics[statistics].Item2);
                break;
        }
    }
    
    public void updateAchievements(Statistics statistics, Achievement achievement)
    {
        if (_statistics[statistics].Item1 >= _statistics[statistics].Item2)
        {
            _achievements[achievement] = true;
        }
    }
    
    // Gets the value of a statistic, either current or global
    public int getStatistics(Statistics statistics, bool isMenuStats = false)
    {
        if(!isMenuStats) return _statistics[statistics].Item1;
        return _globalStatistics[statistics];
    }

    // Loads and aggregates statistics from all saved game states into global statistics
    public void loadGlobalStatistics()
    {
        // Reset all global statistics to 0 for recomputation
        foreach (var key in _globalStatistics.Keys.ToList())
        {
            _globalStatistics[key] = 0;
        }
        // Aggregate statistics from saved game states
        for (int i = 1; i < 5; i++)
        {
            var gameState = _gameManager.SaveManager.readGameStateFromFile(i);
            
            if (gameState == null) continue;
            
            _globalStatistics[Statistics.HighestWantedLevel] = Math.Max(gameState.StatisticsWantedLevel, _globalStatistics[Statistics.HighestWantedLevel]);
            _globalStatistics[Statistics.RecipesUnlocked] = Math.Max(gameState.StatisticsRecipes, _globalStatistics[Statistics.RecipesUnlocked]);
            _globalStatistics[Statistics.LastArrest] = Math.Max(gameState.StatisticsArrest, _globalStatistics[Statistics.LastArrest]);
            _globalStatistics[Statistics.StepsTaken] += gameState.StatisticsSteps;
            _globalStatistics[Statistics.DrugsSell] += gameState.StatisticsDrugs;
            _globalStatistics[Statistics.SuccessfulCooking] += gameState.StatisticsCooking;
            _globalStatistics[Statistics.DamageTaken] += gameState.StatisticsDamage;
            _globalStatistics[Statistics.ReceivedMoney] += gameState.StatisticsMoney;
            _globalStatistics[Statistics.CartelKilled] += gameState.StatisticsKills;
            _globalStatistics[Statistics.HospitalVisits] += gameState.StatisticsHospital;
            _globalStatistics[Statistics.IngredientsCollected] += gameState.StatisticsIngredients;
            _globalStatistics[Statistics.BushesHidden] += gameState.StatisticsBushes;
            _globalStatistics[Statistics.TimesJail] += gameState.StatisticsJail;
            _globalStatistics[Statistics.TimesWin] += gameState.StatisticsWin; 
        }
    } 

    
    public bool getAchievement(Achievement achievement)
    {
        return _achievements[achievement];
    }
    
    public void saveState(GameState gameState, AchievementState achievementState)
    {
        // Statistics
        gameState.StatisticsSteps = _statistics[Statistics.StepsTaken].Item1;
        gameState.StatisticsRecipes = _statistics[Statistics.RecipesUnlocked].Item1;
        gameState.StatisticsDrugs = _statistics[Statistics.DrugsSell].Item1;
        gameState.StatisticsCooking = _statistics[Statistics.SuccessfulCooking].Item1;
        gameState.StatisticsDamage = _statistics[Statistics.DamageTaken].Item1;
        gameState.StatisticsMoney = _statistics[Statistics.ReceivedMoney].Item1;
        gameState.StatisticsArrest = _statistics[Statistics.LastArrest].Item1;
        gameState.StatisticsKills = _statistics[Statistics.CartelKilled].Item1;
        gameState.StatisticsWantedLevel = _statistics[Statistics.HighestWantedLevel].Item1;
        gameState.StatisticsHospital = _statistics[Statistics.HospitalVisits].Item1;
        gameState.StatisticsIngredients = _statistics[Statistics.IngredientsCollected].Item1;
        gameState.StatisticsBushes = _statistics[Statistics.BushesHidden].Item1;
        gameState.StatisticsJail = _statistics[Statistics.TimesJail].Item1;
        gameState.StatisticsWin = _statistics[Statistics.TimesWin].Item1;
        
        // Achievements
        achievementState.AchievementSteps = _achievements[Achievement.FirstSteps];
        achievementState.AchievementCraftAndSell = _achievements[Achievement.FirstCraftAndSell];
        achievementState.AchievementMoney = _achievements[Achievement.FirstXMoney];
        achievementState.AchievementRecipes = _achievements[Achievement.AllRecipes];
        achievementState.AchievementJail = _achievements[Achievement.FirstJail];
        achievementState.AchievementDrugs = _achievements[Achievement.XDrugSell];
        achievementState.AchievementIngredients = _achievements[Achievement.XIngredients];
        achievementState.AchievementWanted = _achievements[Achievement.MostWanted];
        achievementState.AchievementKills = _achievements[Achievement.KillXCartel];
        achievementState.AchievementBushes = _achievements[Achievement.HideXBushes];
        achievementState.AchievementWin = _achievements[Achievement.FirstWin];
    }

    private void loadAchievements(AchievementState achievementState)
    {
        _achievements[Achievement.FirstSteps] = achievementState.AchievementSteps;
        _achievements[Achievement.FirstCraftAndSell] = achievementState.AchievementCraftAndSell;
        _achievements[Achievement.FirstXMoney] = achievementState.AchievementMoney;
        _achievements[Achievement.AllRecipes] = achievementState.AchievementRecipes;
        _achievements[Achievement.FirstJail] = achievementState.AchievementJail;
        _achievements[Achievement.XDrugSell] = achievementState.AchievementDrugs;
        _achievements[Achievement.XIngredients] = achievementState.AchievementIngredients;
        _achievements[Achievement.MostWanted] = achievementState.AchievementWanted;
        _achievements[Achievement.KillXCartel] = achievementState.AchievementKills;
        _achievements[Achievement.HideXBushes] = achievementState.AchievementBushes;
        _achievements[Achievement.FirstWin] = achievementState.AchievementWin;
    }
    
    public void loadStatistics(GameState gameState)
    {
        _statistics[Statistics.StepsTaken] = (gameState.StatisticsSteps, _statistics[Statistics.StepsTaken].Item2);
        _statistics[Statistics.RecipesUnlocked] = (gameState.StatisticsRecipes, _statistics[Statistics.RecipesUnlocked].Item2);
        _statistics[Statistics.DrugsSell] = (gameState.StatisticsDrugs, _statistics[Statistics.DrugsSell].Item2);
        _statistics[Statistics.SuccessfulCooking] = (gameState.StatisticsCooking, _statistics[Statistics.SuccessfulCooking].Item2);
        _statistics[Statistics.DamageTaken] = (gameState.StatisticsDamage, _statistics[Statistics.DamageTaken].Item2);
        _statistics[Statistics.ReceivedMoney] = (gameState.StatisticsMoney, _statistics[Statistics.ReceivedMoney].Item2);
        _statistics[Statistics.LastArrest] = (gameState.StatisticsArrest, _statistics[Statistics.LastArrest].Item2);
        _statistics[Statistics.CartelKilled] = (gameState.StatisticsKills, _statistics[Statistics.CartelKilled].Item2);
        _statistics[Statistics.HighestWantedLevel] = (gameState.StatisticsWantedLevel, _statistics[Statistics.HighestWantedLevel].Item2);
        _statistics[Statistics.HospitalVisits] = (gameState.StatisticsHospital, _statistics[Statistics.HospitalVisits].Item2);
        _statistics[Statistics.IngredientsCollected] = (gameState.StatisticsIngredients, _statistics[Statistics.IngredientsCollected].Item2);
        _statistics[Statistics.BushesHidden] = (gameState.StatisticsBushes, _statistics[Statistics.BushesHidden].Item2);
        _statistics[Statistics.TimesJail] = (gameState.StatisticsJail, _statistics[Statistics.TimesJail].Item2);
        _statistics[Statistics.TimesWin] = (gameState.StatisticsWin, _statistics[Statistics.TimesWin].Item2);
    }
    
    public static AchievementState createDefaultAchievementState()
    {
        AchievementState achievementState = new AchievementState()
        {
            AchievementSteps = false,
            AchievementCraftAndSell = false,
            AchievementMoney = false,
            AchievementRecipes = false,
            AchievementJail = false,
            AchievementDrugs = false,
            AchievementIngredients = false,
            AchievementWanted = false,
            AchievementKills = false,
            AchievementBushes = false,
            AchievementWin = false
        };
        return achievementState;
    }

    public Dictionary<Achievement, bool> Achievments => _achievements;
}
