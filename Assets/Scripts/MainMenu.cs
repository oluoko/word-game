using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public GameObject statsPanel;
    public GameObject settingsPanel;
    public GameObject howToPlayPanel;
    public GameObject statsButton;
    public GameObject playButton;
    public Image gameLevelImage;
    public Slider difficultySlider;
    public TextMeshProUGUI difficultyText;
    
    [Header("Difficulty Level Sprites")]
    public Sprite easySprite;
    public Sprite mediumSprite;
    public Sprite hardSprite;
    
    [Header("Audio Controls")]
    public Toggle musicToggle;
    public Toggle soundToggle;

    Color easyColor = new Color(0.188f, 0.690f, 0.251f);
    Color mediumColor = new Color(0.984f, 0.745f, 0.231f);
    Color hardColor = new Color(0.890f, 0.290f, 0.290f);

    private void Start()
    {
        if (GameSettings.Instance == null)
        {
            GameObject settingsObj = new GameObject("GameSettings");
            settingsObj.AddComponent<GameSettings>();
        }

        if (GameStatistics.Instance == null)
        {
            GameObject statsObj = new GameObject("GameStatistics");
            statsObj.AddComponent<GameStatistics>();
        }

        if (difficultySlider != null)
        {
            difficultySlider.minValue = 0;
            difficultySlider.maxValue = 2;
            difficultySlider.wholeNumbers = true;
            difficultySlider.value = (int)GameSettings.Instance.CurrentDifficulty;
            difficultySlider.onValueChanged.AddListener(OnDifficultyChanged);
            UpdateDifficultyDisplay();
        }
        
        if (musicToggle != null)
        {
            musicToggle.isOn = GameSettings.Instance.IsMusicEnabled;
            musicToggle.onValueChanged.AddListener(OnMusicToggleChanged);
        }
        
        if (soundToggle != null)
        {
            soundToggle.isOn = GameSettings.Instance.IsSoundEnabled;
            soundToggle.onValueChanged.AddListener(OnSoundToggleChanged);
        }
    }

    public void OnDifficultyChanged(float value)
    {
        GameSettings.Difficulty newDifficulty = (GameSettings.Difficulty)(int)value;
        GameSettings.Instance.CurrentDifficulty = newDifficulty;
        UpdateDifficultyDisplay();
    }

    private void UpdateDifficultyDisplay()
    {
        GameSettings.Difficulty currentDiff = GameSettings.Instance.CurrentDifficulty;

        if (difficultyText != null)
        {
            switch (currentDiff)
            {
                case GameSettings.Difficulty.Easy:
                    difficultyText.text = "EASY";
                    difficultyText.color = easyColor;
                    difficultySlider.image.color = easyColor;
                    difficultySlider.fillRect.GetComponent<Image>().color = easyColor;
                    statsButton.GetComponent<Image>().color = easyColor;
                    playButton.GetComponent<Image>().color = easyColor;
                    if (gameLevelImage != null && easySprite != null)
                    {
                        gameLevelImage.sprite = easySprite;
                    }
                    break;
                case GameSettings.Difficulty.Medium:
                    difficultyText.text = "MEDIUM";
                    difficultyText.color = mediumColor;
                    difficultySlider.image.color = mediumColor;
                    difficultySlider.fillRect.GetComponent<Image>().color = mediumColor;
                    statsButton.GetComponent<Image>().color = mediumColor;
                    playButton.GetComponent<Image>().color = mediumColor;
                    if (gameLevelImage != null && mediumSprite != null)
                    {
                        gameLevelImage.sprite = mediumSprite;
                    }
                    break;
                case GameSettings.Difficulty.Hard:
                    difficultyText.text = "HARD";
                    difficultyText.color = hardColor;
                    difficultySlider.image.color = hardColor;
                    difficultySlider.fillRect.GetComponent<Image>().color = hardColor;
                    statsButton.GetComponent<Image>().color = hardColor;
                    playButton.GetComponent<Image>().color = hardColor;
                    if (gameLevelImage != null && hardSprite != null)
                    {
                        gameLevelImage.sprite = hardSprite;
                    }
                    break;
            }
        }
    }
    
    public void OnMusicToggleChanged(bool isOn)
    {
        GameSettings.Instance.IsMusicEnabled = isOn;
        AudioManager.Instance?.SetMusicEnabled(isOn);
    }
    
    public void OnSoundToggleChanged(bool isOn)
    {
        GameSettings.Instance.IsSoundEnabled = isOn;
        AudioManager.Instance?.SetSoundEnabled(isOn);
    }

    public void StartGame()
    {
        AudioManager.Instance?.PlayButtonClick();
        SceneManager.LoadScene("WordGame");
    }

    public void ViewStats()
    {
        AudioManager.Instance?.PlayButtonClick();
        statsPanel.SetActive(true);
        
    }

    public void CloseStats()
    {
        AudioManager.Instance?.PlayButtonClick();
        statsPanel.SetActive(false);
    }

    public void ViewSettings()
    {
        AudioManager.Instance?.PlayButtonClick();
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        AudioManager.Instance?.PlayButtonClick();
        settingsPanel.SetActive(false);
    }

    public void ViewHowToPlay()
    {
        AudioManager.Instance?.PlayButtonClick();
        howToPlayPanel.SetActive(true);
    }

    public void CloseHowToPlay()
    {
        AudioManager.Instance?.PlayButtonClick();
        howToPlayPanel.SetActive(false);
    }

    public void QuitGame()
    {
        AudioManager.Instance?.PlayButtonClick();
        Application.Quit();
    }

    public void BackToMainMenu()
    {
        AudioManager.Instance?.PlayButtonClick();
        SceneManager.LoadScene("MainMenu");
    }
}