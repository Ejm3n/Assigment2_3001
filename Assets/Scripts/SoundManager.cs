using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public struct AudioTrack
{
    public SoundType Type;
    public AudioClip Clip;
    public string Name;
}

public enum SoundType
{
    SOUND_SFX,
    SOUND_MUSIC
}
public class SoundManager : MonoBehaviour
{
    [Header("Put all sounds and sfx here")]
    [SerializeField] private AudioTrack[] audioTracks;
    public static SoundManager Instance;
    private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> musicDictionary = new Dictionary<string, AudioClip>();

    private AudioSource sfxSource;
    private AudioSource musicSource;

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }




    } // Static constructor. Gets called the first time the class is accessed.

    private void Start()
    {
        Initialize();
    }
    // Initialize the SoundManager. I just put this functionality here instead of in the static constructor.
    private void Initialize()
    {
        // Create a new GameObject to hold the AudioSource
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.volume = 1.0f;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.volume = 0.25f;
        musicSource.loop = true;
        foreach (AudioTrack track in audioTracks)
        {
            AddSound(track.Name, track.Clip, track.Type);
        }
        PlayLoopedMusic("bg");
        
    }

    // Add a sound to the dictionary.
    public void AddSound(string soundKey, AudioClip audioClip, SoundType soundType)
    {
        Dictionary<string, AudioClip> targetDictionary = GetDictionaryByType(soundType);

        if (!targetDictionary.ContainsKey(soundKey))
        {
            targetDictionary.Add(soundKey, audioClip);
        }
        else
        {
            Debug.LogWarning("Sound key " + soundKey + " already exists in the " + soundType + " dictionary.");
        }
    }

    // Play a sound by key interface.
    public void PlaySound(string soundKey)
    {
        Play(soundKey, SoundType.SOUND_SFX);
    }
    public void PlayRandomSound(string[] soundKeys)
    {
        PlaySound(soundKeys[Random.Range(0, soundKeys.Length)]);
    }
    // Play music by key interface.
    public void PlayMusic(string soundKey)
    {
        musicSource.Stop();
        musicSource.loop = false;
        musicSource.clip = null;
        Play(soundKey, SoundType.SOUND_MUSIC);
    }
    public void PlayLoopedMusic(string soundKey)
    {
        musicSource.Stop();
        if (musicDictionary.ContainsKey(soundKey))
        {
            musicSource.loop = true;
            musicSource.clip = musicDictionary[soundKey];
            musicSource.Play();
        }

    }

    // Play utility.
    private void Play(string soundKey, SoundType soundType)
    {
        Dictionary<string, AudioClip> targetDictionary;
        AudioSource targetSource;

        SetTargetsByType(soundType, out targetDictionary, out targetSource);

        if (targetDictionary.ContainsKey(soundKey))
        {
            targetSource.PlayOneShot(targetDictionary[soundKey]);
        }
        else
        {
            Debug.LogWarning("Sound key " + soundKey + " not found in the " + soundType + " dictionary.");
        }
    }

    private void SetTargetsByType(SoundType soundType, out Dictionary<string, AudioClip> targetDictionary, out AudioSource targetSource)
    {
        switch (soundType)
        {
            case SoundType.SOUND_SFX:
                targetDictionary = sfxDictionary;
                targetSource = sfxSource;
                break;
            case SoundType.SOUND_MUSIC:
                targetDictionary = musicDictionary;
                targetSource = musicSource;
                break;
            default:
                Debug.LogError("Unknown sound type: " + soundType);
                targetDictionary = null;
                targetSource = null;
                break;
        }
    }
    private Dictionary<string, AudioClip> GetDictionaryByType(SoundType soundType)
    {
        switch (soundType)
        {
            case SoundType.SOUND_SFX:
                return sfxDictionary;
            case SoundType.SOUND_MUSIC:
                return musicDictionary;
            default:
                Debug.LogError("Unknown sound type: " + soundType);
                return null;
        }
    }
   

    //Debug.Log("music = " + MainController.Instance.DataSaver.GetSoundVolume());

 
}