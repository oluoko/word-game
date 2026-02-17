using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance { get; private set; }

    public enum Difficulty
    {
        Easy = 0,
        Medium = 1,
        Hard = 2
    }

    private Difficulty currentDifficulty = Difficulty.Medium;
    private bool isMusicEnabled = true;
    private bool isSoundEnabled = true;

    private class DifficultyGameState
    {
        public bool hasUnfinishedGame = false;
        public string savedWordToGuess = "";
        public int savedRowIndex = 0;
        public int savedColumnIndex = 0;
        public int savedGuessCount = 0;
        public string[] savedBoardState = new string[6];
        public string[] savedTileStates = new string[6];
        public string savedKeyboardState = "";
    }

    private DifficultyGameState easyGameState = new DifficultyGameState();
    private DifficultyGameState mediumGameState = new DifficultyGameState();
    private DifficultyGameState hardGameState = new DifficultyGameState();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Difficulty CurrentDifficulty
    {
        get { return currentDifficulty; }
        set
        {
            currentDifficulty = value;
            SaveSettings();
        }
    }

    public string GetCurrentDifficultyString()
    {
        switch (currentDifficulty)
        {
            case Difficulty.Easy:
                return "Easy";
            case Difficulty.Medium:
                return "Medium";
            case Difficulty.Hard:
                return "Hard";
            default:
                return "Medium";
        }
    }

    public bool IsMusicEnabled
    {
        get { return isMusicEnabled; }
        set
        {
            isMusicEnabled = value;
            SaveSettings();
        }
    }

    public bool IsSoundEnabled
    {
        get { return isSoundEnabled; }
        set
        {
            isSoundEnabled = value;
            SaveSettings();
        }
    }

    public bool HasUnfinishedGame
    {
        get 
        { 
            DifficultyGameState state = GetCurrentDifficultyState();
            return state.hasUnfinishedGame;
        }
    }

    private DifficultyGameState GetCurrentDifficultyState()
    {
        switch (currentDifficulty)
        {
            case Difficulty.Easy:
                return easyGameState;
            case Difficulty.Medium:
                return mediumGameState;
            case Difficulty.Hard:
                return hardGameState;
            default:
                return mediumGameState;
        }
    }

    public int GetIncorrectKeysCount()
    {
        switch (currentDifficulty)
        {
            case Difficulty.Easy:
                return 4; 
            case Difficulty.Medium:
                return 2; 
            case Difficulty.Hard:
                return 0; 
            default:
                return 2;
        }
    }

    public void SaveGameState(string wordToGuess, int rowIndex, int columnIndex, int guessCount,
                              string[] boardState, string[] tileStates, string keyboardState)
    {
        DifficultyGameState state = GetCurrentDifficultyState();
        
        state.hasUnfinishedGame = true;
        state.savedWordToGuess = wordToGuess;
        state.savedRowIndex = rowIndex;
        state.savedColumnIndex = columnIndex;
        state.savedGuessCount = guessCount;
        state.savedBoardState = boardState;
        state.savedTileStates = tileStates;
        state.savedKeyboardState = keyboardState;

        string prefix = currentDifficulty.ToString();
        PlayerPrefs.SetInt($"{prefix}_HasUnfinishedGame", 1);
        PlayerPrefs.SetString($"{prefix}_SavedWord", wordToGuess);
        PlayerPrefs.SetInt($"{prefix}_SavedRowIndex", rowIndex);
        PlayerPrefs.SetInt($"{prefix}_SavedColumnIndex", columnIndex);
        PlayerPrefs.SetInt($"{prefix}_SavedGuessCount", guessCount);
        PlayerPrefs.SetString($"{prefix}_SavedKeyboardState", keyboardState);
        
        for (int i = 0; i < boardState.Length; i++)
        {
            PlayerPrefs.SetString($"{prefix}_SavedBoard_{i}", boardState[i] ?? "");
            PlayerPrefs.SetString($"{prefix}_SavedStates_{i}", tileStates[i] ?? "");
        }
        
        PlayerPrefs.Save();
    }

    public void GetSavedGameState(out string wordToGuess, out int rowIndex, out int columnIndex, out int guessCount,
                                   out string[] boardState, out string[] tileStates, out string keyboardState)
    {
        DifficultyGameState state = GetCurrentDifficultyState();
        
        wordToGuess = state.savedWordToGuess;
        rowIndex = state.savedRowIndex;
        columnIndex = state.savedColumnIndex;
        guessCount = state.savedGuessCount;
        boardState = state.savedBoardState;
        tileStates = state.savedTileStates;
        keyboardState = state.savedKeyboardState;
    }

    public void ClearSavedGame()
    {
        DifficultyGameState state = GetCurrentDifficultyState();
        
        state.hasUnfinishedGame = false;
        state.savedWordToGuess = "";
        state.savedRowIndex = 0;
        state.savedColumnIndex = 0;
        state.savedGuessCount = 0;
        state.savedBoardState = new string[6];
        state.savedTileStates = new string[6];
        state.savedKeyboardState = "";

        string prefix = currentDifficulty.ToString();
        PlayerPrefs.SetInt($"{prefix}_HasUnfinishedGame", 0);
        PlayerPrefs.DeleteKey($"{prefix}_SavedWord");
        PlayerPrefs.DeleteKey($"{prefix}_SavedRowIndex");
        PlayerPrefs.DeleteKey($"{prefix}_SavedColumnIndex");
        PlayerPrefs.DeleteKey($"{prefix}_SavedGuessCount");
        PlayerPrefs.DeleteKey($"{prefix}_SavedKeyboardState");
        
        for (int i = 0; i < 6; i++)
        {
            PlayerPrefs.DeleteKey($"{prefix}_SavedBoard_{i}");
            PlayerPrefs.DeleteKey($"{prefix}_SavedStates_{i}");
        }
        
        PlayerPrefs.Save();
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetInt("GameDifficulty", (int)currentDifficulty);
        PlayerPrefs.SetInt("MusicEnabled", isMusicEnabled ? 1 : 0);
        PlayerPrefs.SetInt("SoundEnabled", isSoundEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        if (PlayerPrefs.HasKey("GameDifficulty"))
        {
            currentDifficulty = (Difficulty)PlayerPrefs.GetInt("GameDifficulty");
        }
        
        isMusicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        isSoundEnabled = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;

        LoadDifficultyGameState(Difficulty.Easy, easyGameState);
        LoadDifficultyGameState(Difficulty.Medium, mediumGameState);
        LoadDifficultyGameState(Difficulty.Hard, hardGameState);
    }

    private void LoadDifficultyGameState(Difficulty difficulty, DifficultyGameState state)
    {
        string prefix = difficulty.ToString();
        
        state.hasUnfinishedGame = PlayerPrefs.GetInt($"{prefix}_HasUnfinishedGame", 0) == 1;
        
        if (state.hasUnfinishedGame)
        {
            state.savedWordToGuess = PlayerPrefs.GetString($"{prefix}_SavedWord", "");
            state.savedRowIndex = PlayerPrefs.GetInt($"{prefix}_SavedRowIndex", 0);
            state.savedColumnIndex = PlayerPrefs.GetInt($"{prefix}_SavedColumnIndex", 0);
            state.savedGuessCount = PlayerPrefs.GetInt($"{prefix}_SavedGuessCount", 0);
            state.savedKeyboardState = PlayerPrefs.GetString($"{prefix}_SavedKeyboardState", "");
            
            for (int i = 0; i < 6; i++)
            {
                state.savedBoardState[i] = PlayerPrefs.GetString($"{prefix}_SavedBoard_{i}", "");
                state.savedTileStates[i] = PlayerPrefs.GetString($"{prefix}_SavedStates_{i}", "");
            }
        }
    }

    private void OnApplicationQuit()
    {
        SaveSettings();
    }
}