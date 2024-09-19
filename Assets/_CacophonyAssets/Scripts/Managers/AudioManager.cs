using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Author: Liz
/// Editor(s): Trinity
/// Description: Contains specific information for each audio clip used.
/// </summary>
[System.Serializable]
public class AudioFile
{
    public string Name;
    [Space]
    public AudioClip Clip;
    [Range(0, 1)] public float Volume = 0.25f;
    public float Pitch = 1f;
    //The pitch (+ or -) that an audio file will be randomly deviated by. Great for SFX.
    [Range(0, 1)] public float PitchDeviation = 0;
    [Space]
    public bool Loop;

    [HideInInspector] public AudioSource Source;

    [HideInInspector] public Coroutine FadeCoroutine = null;
}

/// <summary>
/// Author: Liz
/// Editor(s): Trinity
/// Description: 
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Files")]
    [SerializeField] private List<AudioFile> _music = new List<AudioFile>();
    [SerializeField] private List<AudioFile> _soundEffects = new List<AudioFile>();
    [SerializeField] private List<AudioFile> _amplifiedEffects = new List<AudioFile>();
    public List<AudioFile> _currentMusic = new List<AudioFile>();

    public List<AudioFile> Music
    {
        get
        {
            return _music;
        }
    }
    public List<AudioFile> SoundEffects
    {
        get
        {
            return _music;
        }
    }
    public List<AudioFile> CurrentMusic
    {
        get
        {
            return _music;
        }
    }

    [Header("Mixing")]
    [SerializeField] private AudioMixerGroup _musicGroup;
    [SerializeField] private AudioMixerGroup _soundEffectGroup;
    [SerializeField] private AudioMixerGroup _amplifiedEffectGroup;

    // Resonator Types
    Dictionary<HarmonizationType, int> resonatorDict = new()
    {
        { HarmonizationType.A, 0 },
        { HarmonizationType.B, 0 },
        { HarmonizationType.C, 0 }
    };

    /// <summary>
    /// Adds each Source to the object.
    /// </summary>
    private void Awake()
    {
        if (Instance == null && Instance != this)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        SetUpAudio(_music, _musicGroup);
        SetUpAudio(_soundEffects, _soundEffectGroup);
        SetUpAudio(_amplifiedEffects, _amplifiedEffectGroup);
    }

    /// <summary>
    /// Sets up AudioFiles.
    /// </summary>
    /// <param name="audioList">Audio files are from this List.</param>
    /// <param name="mixerGroup">The mixer group to use.</param>
    private void SetUpAudio(List<AudioFile> audioList, AudioMixerGroup mixerGroup)
    {
        foreach (AudioFile af in audioList)
        {
            af.Source = gameObject.AddComponent<AudioSource>();
            af.Source.clip = af.Clip;

            af.Source.volume = af.Volume;
            af.Source.pitch = af.Pitch;
            af.Source.loop = af.Loop;
            af.Source.playOnAwake = false;

            af.Source.outputAudioMixerGroup = mixerGroup;
        }
    }

    /// <summary>
    /// Plays a sound effect.
    /// </summary>
    /// <param name="name">Its name</param>
    public void PlaySoundEffect(string name)
    {
        AudioFile a = GetAudioFile(_soundEffects, name);

        if (a == null)
        {
            a = GetAudioFile(_amplifiedEffects, name);

            if(a == null)
            {
                Debug.LogWarning($"No sound found with name {name}");
                return;
            }
        }

        float newPitch = Random.Range(1 - a.PitchDeviation, 1 + a.PitchDeviation);

        if (a.Source.isActiveAndEnabled)
        {
            a.Source.pitch = newPitch;
            a.Source.Play();
        }
        else
        {
            Debug.LogWarning($"{a.Source.name} is not enabled! Cannot play audio file.");
        }
    }

    /// <summary>
    /// Switches current music to a new track.
    /// </summary>
    /// <param name="name">Music name</param>
    public void PlayMusic(string name)
    {
        AudioFile a = GetAudioFile(_music, name);

        if (a == null)
        {
            Debug.LogWarning($"No sound found with name {name}");
            return;
        }

        //Debug.Log("Attempting to play " + a.Name);
        if (a.Source.isActiveAndEnabled)
        {
            //Debug.Log("Playing " + a.Name);
            a.Source.Play();
            a.Source.playOnAwake = true;
        }
        else
        {
            Debug.LogWarning($"{a.Source.name} is not enabled! Cannot play audio file.");
        }

        //Debug.Log("Playing " + a.Name);
        _currentMusic.Add(a);
    }

    public void StopMusic(string name)
    {
        AudioFile a = GetAudioFile(_music, name);

        if (a == null)
        {
            Debug.LogWarning($"No sound found with name {name}");
            return;
        }

        a.Source.Stop();
        a.Source.playOnAwake = false;

        _currentMusic.Remove(a);
    }

    public bool IsMusicPlaying(string name)
    {
        AudioFile a = GetAudioFile(_music, name);

        if (a == null)
        {
            Debug.LogWarning($"No sound found with name {name}");
            return false;
        }

        return _currentMusic.Contains(a);
    }

    public void AdjustMusicVolume(string name, float volume)
    {
        AudioFile a = GetAudioFile(_music, name);

        if (a == null)
        {
            Debug.LogWarning($"No sound found with name {name}");
            return;
        }

        a.Source.volume = volume;
        // Remove this v
        a.Volume = volume;
    }

    /// <summary>
    /// Gradually adjusts the volume of a track over time to reach the target volume
    /// </summary>
    /// <param name="name">Audio File Name</param>
    /// <param name="targetVolume">Volume to reach</param>
    /// <param name="time">Time it takes to reach target volume</param>
    public void AdjustVolumeOverTime(string name, float targetVolume, float time)
    {
        AudioFile af = GetAudioFile(_music, name);

        if (af == null)
            return;

        float startVolume = af.Source.volume;
        InstantiateFadeCoroutine(af, startVolume, targetVolume, time);
    }

    /// <summary>
    /// Gradually adjusts the volume of a track over time to reach the target volume
    /// </summary>
    /// <param name="name">Audio File Name</param>
    /// <param name="startVolume">Volume to start at. Sets current volume to this immediately</param>
    /// <param name="targetVolume">Volume to reach</param>
    /// <param name="time">Time it takes to reach target volume</param>
    public void AdjustVolumeOverTime(string name, float startVolume, float targetVolume, float time)
    {
        AudioFile af = GetAudioFile(_music, name);
        if (af == null)
            return;

        af.Source.volume = startVolume;
        
        af.Volume = startVolume;
        InstantiateFadeCoroutine(af, startVolume, targetVolume, time);
    }

    private void InstantiateFadeCoroutine(AudioFile af, float startVolume, float targetVolume, float time)
    {
        if(af.FadeCoroutine != null)
            StopCoroutine(af.FadeCoroutine);

        
        af.FadeCoroutine = StartCoroutine(AdjustVolumeOverTime(af, startVolume, targetVolume, time));
    }

    private IEnumerator AdjustVolumeOverTime(AudioFile af, float startVolume, float targetVolume, float time)
    {
        float timer = 0;

        if (!_currentMusic.Contains(af))
        {
            //Debug.Log(af.Name + " not currently playing");
        }
            

        while(timer < time)
        {
            if (af.Source.volume == targetVolume)
                break;

            yield return new WaitForFixedUpdate();
            timer += Time.fixedDeltaTime;

            float newVolume = (timer / time) * (targetVolume - startVolume) + startVolume;
            af.Source.volume = newVolume;
            // Remove this v
            af.Volume = newVolume;
            //Debug.Log("Adjusting " + af.Name + " from " + startVolume + " to " + newVolume + "db over " + time + " seconds.");
        }

        af.FadeCoroutine = null;
    }

    public AudioFile GetAudioFile(List<AudioFile> audio, string name)
    {
        foreach (AudioFile af in audio)
        {
            if (af.Name == name)
            {
                return af;
            }
        }

        Debug.Log(name + " not found!");
        return null;
    }

    public void SetMusicVolume(float newVolume)
    {
        _musicGroup.audioMixer.SetFloat("M_Volume", Mathf.Log10(newVolume) * 20);
    }

    public void SetSFXVolume(float newVolume)
    {
        _soundEffectGroup.audioMixer.SetFloat("S_Volume", Mathf.Log10(newVolume) * 20);
    }

    // Resonator SFX
    public void PlayResonatorSoundEffect(HarmonizationType type)
    {
        resonatorDict[type]++;
        //Debug.Log("Play Resonator Sound: " + type);
        UpdateResonatorSounds();
    }

    public void StopResonatorSoundEffect(HarmonizationType type)
    {
        resonatorDict[type]--;
        //Debug.Log("Stop Resonator Sound: " + type);
        UpdateResonatorSounds();
    }

    private void UpdateResonatorSounds()
    {
        foreach(HarmonizationType type in resonatorDict.Keys)
        {
            string songName = GameplayManagers.Instance.Harmonizer.GetHarmonyType(type)._audioName;
            if (resonatorDict[type] > 0 && !IsMusicPlaying(songName))
            {
                PlayMusic(songName);
            }
            else if (IsMusicPlaying(songName))
            {
                StopMusic(songName);
            }
        }
    }
}