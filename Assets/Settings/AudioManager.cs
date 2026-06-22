using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("--- MIXER ---")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string musicParam = "MusicVolume";
    [SerializeField] private string sfxParam   = "SFXVolume";

    [Header("--- MUSIK BACKGROUND ---")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip   musikUtama;

    private const string KEY_MUSIC = "VolumeMusik";
    private const string KEY_SFX   = "VolumeSFX";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetMusicVolume(PlayerPrefs.GetFloat(KEY_MUSIC, 0.75f));
        SetSFXVolume(PlayerPrefs.GetFloat(KEY_SFX, 0.75f));

        if (musicSource != null && musikUtama != null && !musicSource.isPlaying)
        {
            musicSource.clip = musikUtama;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public float GetMusicVolume() => PlayerPrefs.GetFloat(KEY_MUSIC, 0.75f);
    public float GetSFXVolume()   => PlayerPrefs.GetFloat(KEY_SFX, 0.75f);

    public void SetMusicVolume(float nilai01)
    {
        nilai01 = Mathf.Clamp(nilai01, 0.0001f, 1f);
        audioMixer.SetFloat(musicParam, Mathf.Log10(nilai01) * 20f);
        PlayerPrefs.SetFloat(KEY_MUSIC, nilai01);
    }

    public void SetSFXVolume(float nilai01)
    {
        nilai01 = Mathf.Clamp(nilai01, 0.0001f, 1f);
        audioMixer.SetFloat(sfxParam, Mathf.Log10(nilai01) * 20f);
        PlayerPrefs.SetFloat(KEY_SFX, nilai01);
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    public void PlayMusic()
    {
        if (musicSource != null && !musicSource.isPlaying)
            musicSource.Play();
    }
}