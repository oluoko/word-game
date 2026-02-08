using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource soundSource;

    [Header("Audio Clips")]
    public AudioClip backgroundMusic;
    public AudioClip buttonClickSound;
    public AudioClip gameOverWonSound;
    public AudioClip gameOverLostSound;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudio();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudio()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = true;
        }

        if (soundSource == null)
        {
            soundSource = gameObject.AddComponent<AudioSource>();
            soundSource.loop = false;
            soundSource.playOnAwake = false;
        }

        if (GameSettings.Instance != null)
        {
            SetMusicEnabled(GameSettings.Instance.IsMusicEnabled);
            SetSoundEnabled(GameSettings.Instance.IsSoundEnabled);
        }

        if (backgroundMusic != null && musicSource.enabled)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }

    public void SetMusicEnabled(bool enabled)
    {
        if (musicSource != null)
        {
            musicSource.mute = !enabled;
        }
    }

    public void SetSoundEnabled(bool enabled)
    {
        if (soundSource != null)
        {
            soundSource.mute = !enabled;
        }
    }

    public void PlaySound(AudioClip clip)
    {
        if (soundSource != null && clip != null && !soundSource.mute)
        {
            soundSource.PlayOneShot(clip);
        }
    }

    public void PlayButtonClick()
    {
        PlaySound(buttonClickSound);
    }

    public void PlayGameWon()
    {
        PlaySound(gameOverWonSound);
    }

    public void PlayGameLost()
    {
        PlaySound(gameOverLostSound);
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void StartMusic()
    {
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }      
    }
}