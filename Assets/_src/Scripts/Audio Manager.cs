using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    [Header("Music")]
    public AudioClip backgroundMusic;
    public float musicVolume = 0.5f;
    public bool playMusicOnStart = true;

    [Header("Sound Effects")]
    public AudioClip interactSound;
    public AudioClip puzzleCompleteSound;
    public AudioClip puzzleFailSound;
    public AudioClip catMeowSound;
    public AudioClip buttonClickSound;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    private static AudioManager instance;
    private float originalMusicVolume;

    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AudioManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("AudioManager");
                    instance = obj.AddComponent<AudioManager>();
                }
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }


        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = musicVolume;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.volume = 0.7f;
        }

        originalMusicVolume = musicVolume;
    }

    void Start()
    {
        if (playMusicOnStart && backgroundMusic != null)
        {
            PlayBackgroundMusic();
        }
    }

    public void PlayBackgroundMusic()
    {
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }

    public void StopBackgroundMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    public void PlayInteractSound()
    {
        if (interactSound != null)
            PlaySound(interactSound);
    }

    public void PlayPuzzleCompleteSound()
    {
        if (puzzleCompleteSound != null)
            PlaySound(puzzleCompleteSound);
    }

    public void PlayPuzzleFailSound()
    {
        if (puzzleFailSound != null)
            PlaySound(puzzleFailSound);
    }

    public void PlayCatMeowSound()
    {
        if (catMeowSound != null)
            PlaySound(catMeowSound, 0.8f);
    }

    public void PlayButtonClickSound()
    {
        if (buttonClickSound != null)
            PlaySound(buttonClickSound, 0.5f);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = Mathf.Clamp01(volume);
        }
    }

    public void FadeOutMusic(float duration)
    {
        StartCoroutine(FadeMusic(0f, duration));
    }

    public void FadeInMusic(float duration)
    {
        StartCoroutine(FadeMusic(originalMusicVolume, duration));
    }

    private IEnumerator FadeMusic(float targetVolume, float duration)
    {
        float startVolume = musicSource.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, targetVolume, timer / duration);
            yield return null;
        }

        musicSource.volume = targetVolume;
    }
}