using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class Board : MonoBehaviour
{
    private static readonly Key[] SUPPORTED_KEYS = new Key[]
    {
        Key.A, Key.B, Key.C, Key.D, Key.E, Key.F, Key.G, 
        Key.H, Key.I, Key.J, Key.K, Key.L, Key.M, Key.N, 
        Key.O, Key.P, Key.Q, Key.R, Key.S, Key.T, Key.U, 
        Key.V, Key.W, Key.X, Key.Y, Key.Z,
    };

    private Row[] rows;

    private string[] solutions;
    private string[] validWords;
    private string wordToGuess;

    public Color correctColor = new Color(0.188f, 0.690f, 0.251f);
    public Color wrongColor = new Color(0.329f, 0.361f, 0.329f); 

    private int rowIndex;
    private int columnIndex;
    private int currentGuessCount;

    [Header("TileStates")]
    public Tile.State correctState;
    public Tile.State emptyState;
    public Tile.State incorrectState;
    public Tile.State misplacedState;
    public Tile.State occupiedState;

    [Header("UI")]
    public GameObject levelFailedPanel;
    public GameObject gameWonPanel;
    public GameObject infoPanel;
    public GameObject submitButton; 
    public Row correctWordRow; 
    public VirtualKeyboard virtualKeyboard;

    private Keyboard keyboard;
    private GameSettings.Difficulty currentDifficulty;
    
    
    private bool gameEndedNaturally = false;

    private void Start()
    {
        LoadWords();
        NewGame();
    }

    public void NewGame()
    {
        
        if (GameSettings.Instance != null && GameSettings.Instance.HasUnfinishedGame)
        {
            RestoreSavedGame();
        }
        else
        {
            StartFreshGame();
        }
    }

    private void StartFreshGame()
    {
        ClearBoard();
        correctWordRow?.gameObject.SetActive(false);
        SetRandomWordToGuess();
        
        if (GameSettings.Instance != null)
        {
            currentDifficulty = GameSettings.Instance.CurrentDifficulty;
        }
        
        MarkRandomIncorrectKeys();
        
        currentGuessCount = 0;
        gameEndedNaturally = false;
        enabled = true;
    }

    private void RestoreSavedGame()
    {
        string savedWord;
        int savedRow, savedColumn, savedGuessCount;
        string[] boardState, tileStates;
        string keyboardState;
        
        GameSettings.Instance.GetSavedGameState(out savedWord, out savedRow, out savedColumn, out savedGuessCount,
                                                 out boardState, out tileStates, out keyboardState);
        
        wordToGuess = savedWord;
        rowIndex = savedRow;
        columnIndex = savedColumn;
        currentGuessCount = savedGuessCount;
        
        if (GameSettings.Instance != null)
        {
            currentDifficulty = GameSettings.Instance.CurrentDifficulty;
        }
        
        for (int r = 0; r < rows.Length && r < boardState.Length; r++)
        {
            if (!string.IsNullOrEmpty(boardState[r]))
            {
                string word = boardState[r];
                string states = tileStates[r];
                
                for (int c = 0; c < rows[r].tiles.Length && c < word.Length; c++)
                {
                    if (word[c] != '\0' && word[c] != ' ')
                    {
                        rows[r].tiles[c].SetLetter(word[c]);
                        
                        if (c < states.Length)
                        {
                            Tile.State state = CharToTileState(states[c]);
                            rows[r].tiles[c].SetState(state);
                        }
                    }
                }
            }
        }
        
        
        if (virtualKeyboard != null && !string.IsNullOrEmpty(keyboardState))
        {
            virtualKeyboard.RestoreKeyboardState(keyboardState);
        }
        
        gameEndedNaturally = false;
        enabled = true;
    }

    public void TryAgain()
    {
        
        GameSettings.Instance?.ClearSavedGame();
        
        ClearBoard();
        correctWordRow?.gameObject.SetActive(false);
        SetRandomWordToGuess();
        MarkRandomIncorrectKeys();
        
        currentGuessCount = 0;
        gameEndedNaturally = false;
        enabled = true;
    }

    private void LoadWords()
    {
        TextAsset validWordsTextAsset = Resources.Load<TextAsset>("official_wordle_all") as TextAsset;
        TextAsset solutionsTextAsset = Resources.Load<TextAsset>("official_wordle_common") as TextAsset;

        validWords = validWordsTextAsset.text.Split('\n');
        solutions = solutionsTextAsset.text.Split('\n');
    }

    private void SetRandomWordToGuess()
    {
        wordToGuess = solutions[Random.Range(0, solutions.Length)];
        wordToGuess = wordToGuess.ToLower().Trim();
    }

    private void Awake()
    {
        rows = GetComponentsInChildren<Row>();
        ResetSubmitButton();
    }

    private void Update()
    {
        keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        Row currentRow = rows[rowIndex];
        
        if (keyboard.backspaceKey.wasPressedThisFrame)
        {
            AudioManager.Instance?.PlayButtonClick();
            HandleBackspace();
        } 
        else if (columnIndex >= currentRow.tiles.Length)
        {
            if(keyboard.enterKey.wasPressedThisFrame)
            {
                AudioManager.Instance?.PlayButtonClick();
                SubmitRow(currentRow);
            }
        } 
        else if (keyboard.enterKey.wasPressedThisFrame && columnIndex <= currentRow.tiles.Length)
        {
            StartCoroutine(ShowInfoPanel(5f, "You need to enter a full five-letter word"));
        }
        else
        {
            for (int i = 0; i < SUPPORTED_KEYS.Length; i++)
            {
                if (keyboard[SUPPORTED_KEYS[i]].wasPressedThisFrame)
                {
                    AudioManager.Instance?.PlayButtonClick();
                    HandleLetterInput((char)('A' + i));
                    break;
                }
            }
        }
    }

    public void OnVirtualKeyPressed(VirtualKey key)
    {
        Row currentRow = rows[rowIndex];

        switch (key.keyType)
        {
            case VirtualKey.KeyType.Letter:
                if (columnIndex < currentRow.tiles.Length)
                {
                    HandleLetterInput(key.letter);
                }
                break;

            case VirtualKey.KeyType.Backspace:
                HandleBackspace();
                break;

            case VirtualKey.KeyType.Enter:
                if (columnIndex >= currentRow.tiles.Length)
                {
                    SubmitRow(currentRow);
                } else
                {
                    StartCoroutine(ShowInfoPanel(5f, "You need to enter a full five-letter word"));
                }
                break;
        }
    }

    private void HandleLetterInput(char letter)
    {
        Row currentRow = rows[rowIndex];
        
        if (columnIndex < currentRow.tiles.Length)
        {
            currentRow.tiles[columnIndex].SetLetter(char.ToUpper(letter));
            currentRow.tiles[columnIndex].SetState(occupiedState);
            columnIndex++;
        }
    }

    private void HandleBackspace()
    {
        Row currentRow = rows[rowIndex];
        
        if (columnIndex > 0)
        {
            columnIndex--;
            currentRow.tiles[columnIndex].SetLetter('\0');
            currentRow.tiles[columnIndex].SetState(emptyState);
        }
        
        ResetSubmitButton();
    }

    private void SubmitRow(Row row)
    {      
        if(!isValid(row.word))
        {
            submitButton.GetComponentInChildren<TextMeshProUGUI>().text = "Not a word";
            submitButton.GetComponentInChildren<Image>().color = wrongColor;
            
            return;
        }

        currentGuessCount++;

        string remaining = wordToGuess;

        for (int i = 0; i < row.tiles.Length; i++)
        {
            Tile tile = row.tiles[i];

            if(char.ToLower(tile.letter) == wordToGuess[i])
            {
                tile.SetState(correctState);
            
                virtualKeyboard?.UpdateKeyState(tile.letter, "correct");     
            
                remaining = remaining.Remove(i, 1);
                remaining = remaining.Insert(i, " ");
            } 
            else if (!wordToGuess.Contains(char.ToLower(tile.letter)))
            {
                tile.SetState(incorrectState);
            
                virtualKeyboard?.UpdateKeyState(tile.letter, "incorrect");
            }
        }

        for (int i = 0; i < row.tiles.Length; i++)
        {
            Tile tile = row.tiles[i];

            if (tile.tileState != correctState && tile.tileState != incorrectState)
            {
                if (remaining.Contains(char.ToLower(tile.letter)))
                {
                    tile.SetState(misplacedState);
                
                    virtualKeyboard?.UpdateKeyState(tile.letter, "misplaced");
                
                    int index = remaining.IndexOf(char.ToLower(tile.letter));
                    remaining = remaining.Remove(index, 1);
                    remaining = remaining.Insert(index, " ");
                } 
                else 
                {
                    tile.SetState(incorrectState);
                
                    virtualKeyboard?.UpdateKeyState(tile.letter, "incorrect");
                }
            } 
        }

        if(HasWon(row))
        {
            gameEndedNaturally = true;
            
            if (GameStatistics.Instance != null)
            {
                GameStatistics.Instance.RecordGameWin(currentDifficulty, currentGuessCount);
            }
            
            
            GameSettings.Instance?.ClearSavedGame();
            
            enabled = false;
            return;
        }

        rowIndex++;
        columnIndex = 0;

        if(rowIndex >= rows.Length)
        {
            gameEndedNaturally = true;
            
            if (GameStatistics.Instance != null)
            {
                GameStatistics.Instance.RecordGameLoss(currentDifficulty);
            }
            
            
            GameSettings.Instance?.ClearSavedGame();
            
            enabled = false;
        }
        else
        {
            
            SaveCurrentGameState();
        }
    }

    private void SaveCurrentGameState()
    {
        if (GameSettings.Instance == null) return;
        
        string[] boardState = new string[rows.Length];
        string[] tileStates = new string[rows.Length];
        
        for (int r = 0; r < rows.Length; r++)
        {
            string word = "";
            string states = "";
            
            for (int c = 0; c < rows[r].tiles.Length; c++)
            {
                char letter = rows[r].tiles[c].letter;
                word += (letter == '\0') ? ' ' : letter;
                states += TileStateToChar(rows[r].tiles[c].tileState);
            }
            
            boardState[r] = word;
            tileStates[r] = states;
        }
        
        string keyboardState = virtualKeyboard?.GetKeyboardState() ?? "";
        
        GameSettings.Instance.SaveGameState(wordToGuess, rowIndex, columnIndex, currentGuessCount,
                                             boardState, tileStates, keyboardState);
    }

    private char TileStateToChar(Tile.State state)
    {
        if (state == correctState) return 'C';
        if (state == incorrectState) return 'I';
        if (state == misplacedState) return 'M';
        if (state == occupiedState) return 'O';
        return 'E'; 
    }

    private Tile.State CharToTileState(char c)
    {
        switch (c)
        {
            case 'C': return correctState;
            case 'I': return incorrectState;
            case 'M': return misplacedState;
            case 'O': return occupiedState;
            default: return emptyState;
        }
    }

    private void MarkRandomIncorrectKeys()
    {
        virtualKeyboard?.ResetKeyboard();
        
        int incorrectKeysCount = GameSettings.Instance != null ? 
            GameSettings.Instance.GetIncorrectKeysCount() : 0;
        
        if (incorrectKeysCount == 0 || virtualKeyboard == null)
        {
            return;
        }

        System.Collections.Generic.List<char> availableIncorrectLetters = new System.Collections.Generic.List<char>();
        
        for (char c = 'A'; c <= 'Z'; c++)
        {
            if (!wordToGuess.Contains(char.ToLower(c)))
            {
                availableIncorrectLetters.Add(c);
            }
        }

        if (availableIncorrectLetters.Count >= incorrectKeysCount)
        {
            for (int i = availableIncorrectLetters.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                char temp = availableIncorrectLetters[i];
                availableIncorrectLetters[i] = availableIncorrectLetters[randomIndex];
                availableIncorrectLetters[randomIndex] = temp;
            }

            for (int i = 0; i < incorrectKeysCount; i++)
            {
                virtualKeyboard.UpdateKeyState(availableIncorrectLetters[i], "incorrect");
            }
        }
    }

    private void ClearBoard()
    {
        for(int row = 0; row < rows.Length; row++)
        {
           for(int column = 0; column < rows[row].tiles.Length; column++)
           {
                rows[row].tiles[column].SetLetter('\0');
                rows[row].tiles[column].SetState(emptyState);
           }
        }
        rowIndex = 0;
        columnIndex = 0;
    }

    private bool isValid(string word)
    {
        foreach (string validWord in validWords)
        {
            if (validWord.Trim().ToLower() == word.ToLower())
            {
                return true;
            }
        }
        return false;
    }

    private bool HasWon(Row row) 
    {
        for (int i = 0; i < row.tiles.Length; i++) 
        {
            if(row.tiles[i].tileState != correctState) 
            {
                return false;
            }
        }
        return true;
    }

    public void ShowCorrectWord()
    {
        if (string.IsNullOrEmpty(wordToGuess) || correctWordRow == null)
        {
            return;
        }

        correctWordRow.gameObject.SetActive(true);

        for (int i = 0; i < correctWordRow.tiles.Length; i++)
        {
            correctWordRow.tiles[i].SetLetter('\0');
            correctWordRow.tiles[i].SetState(emptyState);
        }

        for (int i = 0; i < wordToGuess.Length && i < correctWordRow.tiles.Length; i++)
        {
            correctWordRow.tiles[i].SetLetter(char.ToUpper(wordToGuess[i]));
            correctWordRow.tiles[i].SetState(correctState);
        }

        AudioManager.Instance?.PlayButtonClick();
    }
    
    private void OnEnable()
    {
        gameWonPanel.SetActive(false);
        levelFailedPanel.SetActive(false);
        gameEndedNaturally = false;
    }

    private void OnDisable()
    {
        
        if (!gameEndedNaturally)
        {
            
            if (enabled && rowIndex < rows.Length)
            {
                SaveCurrentGameState();
            }
            return;
        }
        
        int lastRowIndex = Mathf.Min(rowIndex, rows.Length - 1);
        
        if(lastRowIndex >= 0 && HasWon(rows[lastRowIndex]))
        {
            AudioManager.Instance?.PlayGameWon();
            gameWonPanel.SetActive(true);
        } 
        else if(lastRowIndex >= 0 && !HasWon(rows[lastRowIndex]))
        {
            AudioManager.Instance?.PlayGameLost();
            levelFailedPanel.SetActive(true);
        }
    }

    private void ResetSubmitButton()
    {
        submitButton.GetComponentInChildren<TextMeshProUGUI>().text = "Submit";
        submitButton.GetComponentInChildren<Image>().color = correctColor;
    }

    private IEnumerator ShowInfoPanel(float delay, string infoText)
    {
        infoPanel.GetComponentInChildren<TextMeshProUGUI>().text = infoText;
        infoPanel.SetActive(true);
        yield return new WaitForSeconds(delay);
        infoPanel.SetActive(false);
    }

    public void UseHint()
    {
        if (virtualKeyboard == null || string.IsNullOrEmpty(wordToGuess))
        {
            return;
        }

        System.Collections.Generic.List<char> availableHints = new System.Collections.Generic.List<char>();
        
        foreach (char letter in wordToGuess)
        {
            char upperLetter = char.ToUpper(letter);
            
            if (!virtualKeyboard.IsKeyInState(upperLetter, "correct") && 
                !virtualKeyboard.IsKeyInState(upperLetter, "misplaced"))
            {
                if (!availableHints.Contains(upperLetter))
                {
                    availableHints.Add(upperLetter);
                }
            }
        }

        if (availableHints.Count > 0)
        {
            AudioManager.Instance?.PlayButtonClick();
            int randomIndex = Random.Range(0, availableHints.Count);
            char hintLetter = availableHints[randomIndex];
            virtualKeyboard.UpdateKeyState(hintLetter, "misplaced");
        }
        else
        {
            StartCoroutine(ShowInfoPanel(5f, "No hints available - all letters already discovered"));
        }
    }
}