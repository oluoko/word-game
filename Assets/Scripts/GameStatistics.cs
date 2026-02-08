using UnityEngine;

[System.Serializable]
public class DifficultyStats
{
    public int gamesPlayed;
    public int gamesWon;
    public int currentStreak;
    public int maxStreak;
    public int[] guessDistribution = new int[7]; 

    public float WinPercentage
    {
        get
        {
            if (gamesPlayed == 0) return 0f;
            return (float)gamesWon / gamesPlayed * 100f;
        }
    }

    public void RecordWin(int guessCount)
    {
        gamesPlayed++;
        gamesWon++;
        currentStreak++;
        
        if (currentStreak > maxStreak)
        {
            maxStreak = currentStreak;
        }

        if (guessCount >= 1 && guessCount <= 6)
        {
            guessDistribution[guessCount - 1]++;
        }
    }

    public void RecordLoss()
    {
        gamesPlayed++;
        currentStreak = 0;
        guessDistribution[6]++; 
    }
}

public class GameStatistics : MonoBehaviour
{
    public static GameStatistics Instance { get; private set; }

    private DifficultyStats easyStats = new DifficultyStats();
    private DifficultyStats mediumStats = new DifficultyStats();
    private DifficultyStats hardStats = new DifficultyStats();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadStatistics();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public DifficultyStats GetStats(GameSettings.Difficulty difficulty)
    {
        switch (difficulty)
        {
            case GameSettings.Difficulty.Easy:
                return easyStats;
            case GameSettings.Difficulty.Medium:
                return mediumStats;
            case GameSettings.Difficulty.Hard:
                return hardStats;
            default:
                return mediumStats;
        }
    }

    public void RecordGameWin(GameSettings.Difficulty difficulty, int guessCount)
    {
        GetStats(difficulty).RecordWin(guessCount);
        SaveStatistics();
    }

    public void RecordGameLoss(GameSettings.Difficulty difficulty)
    {
        GetStats(difficulty).RecordLoss();
        SaveStatistics();
    }

    private void SaveStatistics()
    {
        SaveDifficultyStats("Easy", easyStats);
        SaveDifficultyStats("Medium", mediumStats);
        SaveDifficultyStats("Hard", hardStats);
        PlayerPrefs.Save();
    }

    private void LoadStatistics()
    {
        LoadDifficultyStats("Easy", easyStats);
        LoadDifficultyStats("Medium", mediumStats);
        LoadDifficultyStats("Hard", hardStats);
    }

    private void SaveDifficultyStats(string prefix, DifficultyStats stats)
    {
        PlayerPrefs.SetInt(prefix + "_GamesPlayed", stats.gamesPlayed);
        PlayerPrefs.SetInt(prefix + "_GamesWon", stats.gamesWon);
        PlayerPrefs.SetInt(prefix + "_CurrentStreak", stats.currentStreak);
        PlayerPrefs.SetInt(prefix + "_MaxStreak", stats.maxStreak);
        
        for (int i = 0; i < stats.guessDistribution.Length; i++)
        {
            PlayerPrefs.SetInt(prefix + "_Guess_" + i, stats.guessDistribution[i]);
        }
    }

    private void LoadDifficultyStats(string prefix, DifficultyStats stats)
    {
        stats.gamesPlayed = PlayerPrefs.GetInt(prefix + "_GamesPlayed", 0);
        stats.gamesWon = PlayerPrefs.GetInt(prefix + "_GamesWon", 0);
        stats.currentStreak = PlayerPrefs.GetInt(prefix + "_CurrentStreak", 0);
        stats.maxStreak = PlayerPrefs.GetInt(prefix + "_MaxStreak", 0);
        
        for (int i = 0; i < stats.guessDistribution.Length; i++)
        {
            stats.guessDistribution[i] = PlayerPrefs.GetInt(prefix + "_Guess_" + i, 0);
        }
    }

    public void ResetAllStats()
    {
        easyStats = new DifficultyStats();
        mediumStats = new DifficultyStats();
        hardStats = new DifficultyStats();
        SaveStatistics();
    }

    public void ResetStats(GameSettings.Difficulty difficulty)
    {
        switch (difficulty)
        {
            case GameSettings.Difficulty.Easy:
                easyStats = new DifficultyStats();
                SaveDifficultyStats("Easy", easyStats);
                break;
            case GameSettings.Difficulty.Medium:
                mediumStats = new DifficultyStats();
                SaveDifficultyStats("Medium", mediumStats);
                break;
            case GameSettings.Difficulty.Hard:
                hardStats = new DifficultyStats();
                SaveDifficultyStats("Hard", hardStats);
                break;
        }
        PlayerPrefs.Save();
    }
}