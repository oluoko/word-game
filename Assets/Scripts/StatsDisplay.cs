using UnityEngine;
using TMPro;

public class StatsDisplay : MonoBehaviour
{
    [Header("Statistics Display")]
    public TextMeshProUGUI wordsSolvedText;
    public TextMeshProUGUI percentageSuccessText;
    public TextMeshProUGUI averageSuccessText;
    public TextMeshProUGUI bestGuessText;
    public TextMeshProUGUI currentStreakText;
    public TextMeshProUGUI bestStreakText;

    private void OnEnable()
    {
        RefreshStats();
    }

    public void RefreshStats()
    {
        if (GameStatistics.Instance == null || GameSettings.Instance == null) return;

        
        GameSettings.Difficulty currentDifficulty = GameSettings.Instance.CurrentDifficulty;
        DifficultyStats stats = GameStatistics.Instance.GetStats(currentDifficulty);

        
        if (wordsSolvedText != null)
        {
            wordsSolvedText.text = stats.gamesWon.ToString();
        }

        
        if (percentageSuccessText != null)
        {
            percentageSuccessText.text = stats.WinPercentage.ToString("F0");
        }

        
        if (averageSuccessText != null)
        {
            float averageGuesses = CalculateAverageGuesses(stats);
            averageSuccessText.text = averageGuesses > 0 ? averageGuesses.ToString("F1") : "0";
        }

        
        if (bestGuessText != null)
        {
            int bestGuess = CalculateBestGuess(stats);
            bestGuessText.text = bestGuess > 0 ? bestGuess.ToString() : "0";
        }

        
        if (currentStreakText != null)
        {
            currentStreakText.text = stats.currentStreak.ToString();
        }

        
        if (bestStreakText != null)
        {
            bestStreakText.text = stats.maxStreak.ToString();
        }
    }

    private float CalculateAverageGuesses(DifficultyStats stats)
    {
        if (stats.gamesWon == 0) return 0f;

        int totalGuesses = 0;
        int totalWins = 0;

        
        for (int i = 0; i < 6; i++)
        {
            int guessCount = i + 1; 
            int gamesWonInThisManyGuesses = stats.guessDistribution[i];
            
            totalGuesses += guessCount * gamesWonInThisManyGuesses;
            totalWins += gamesWonInThisManyGuesses;
        }

        return totalWins > 0 ? (float)totalGuesses / totalWins : 0f;
    }

    private int CalculateBestGuess(DifficultyStats stats)
    {
        
        for (int i = 0; i < 6; i++)
        {
            if (stats.guessDistribution[i] > 0)
            {
                return i + 1; 
            }
        }
        return 0; 
    }

    public void ResetCurrentDifficultyStats()
    {
        if (GameStatistics.Instance != null && GameSettings.Instance != null)
        {
            GameSettings.Difficulty currentDifficulty = GameSettings.Instance.CurrentDifficulty;
            GameStatistics.Instance.ResetStats(currentDifficulty);
            RefreshStats();
        }
    }

    public void ResetAllStats()
    {
        if (GameStatistics.Instance != null)
        {
            GameStatistics.Instance.ResetAllStats();
            RefreshStats();
        }
    }
}