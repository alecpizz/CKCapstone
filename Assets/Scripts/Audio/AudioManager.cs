/******************************************************************
 *    Author: Claire Noto
 *    Contributors: Claire Noto, Alec Pizziferro
 *    Date Created: 09/19/2024
 *    Description: Audio Manager using FMOD. See FMOD documentation
 *    for more info
 *******************************************************************/

using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using STOP_MODE = FMOD.Studio.STOP_MODE;
using System;
using System.Collections;
using UnityEngine.SceneManagement;


public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private EventReference _music = default;
    [SerializeField] [Range(0, 4)] private int _musicLayering = 0;

    private Dictionary<EventReference, EventInstance> AudioInstances;

    private EventInstance _key;
    private static EventReference _nextMusic;
    private static int _nextLayering;

    private void Awake()
    {
        //total hack right here guys
        _nextMusic = _music;
        _nextLayering = _nextLayering;
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        transform.parent = null;
        DontDestroyOnLoad(gameObject);

        Instance = this;
        AudioInstances = new Dictionary<EventReference, EventInstance>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        Debug.Log("Scene loaded, changing audio.");
        if (_key.isValid())
        {
            _key.getDescription(out var description);
            description.getID(out var id);
            if (id != _nextMusic.Guid)
            {
                _key.stop(STOP_MODE.ALLOWFADEOUT);
                _key = PlaySound(_nextMusic);
            }

            _key.setParameterByName("MusicLayering", _nextLayering);
        }
        else
        {
            _key = PlaySound(_nextMusic);
            _key.setParameterByName("MusicLayering", _nextLayering);
        }
    }

    private void Start()
    {
        _key = PlaySound(_nextMusic);
        _key.setParameterByName("MusicLayering", _nextLayering);
    }

    public EventInstance PlaySound(EventReference reference, params ParamRef[] parameters)
    {
        if (reference.IsNull)
        {
            Debug.LogWarning("NO REFERENCE SOUND!");
            return default;
        }

        EventInstance audioEvent;

        if (AudioInstances.ContainsKey(reference))
            AudioInstances.TryGetValue(reference, out audioEvent);
        else
        {
            audioEvent = RuntimeManager.CreateInstance(reference);
            AudioInstances.Add(reference, audioEvent);
        }

        foreach (var paramRef in parameters)
        {
            audioEvent.setParameterByName(paramRef.Name, paramRef.Value);
        }

        audioEvent.start();
        return audioEvent;
    }

    /// <summary>
    /// Plays an FMOD sound using a reference. Just add an eventreference and setup the sound
    /// in the inspector. See the FMOD Guide in resources for more info.
    /// </summary>
    /// <param name="reference">the desired sound reference</param>
    /// <returns>an EventInstance, save it if you need to use a parameter</returns>
    public EventInstance PlaySound(EventReference reference)
    {
        if (reference.IsNull)
        {
            Debug.LogWarning("NO REFERENCE SOUND!");
            return default;
        }

        EventInstance audioEvent;

        if (AudioInstances.ContainsKey(reference))
            AudioInstances.TryGetValue(reference, out audioEvent);
        else
        {
            audioEvent = RuntimeManager.CreateInstance(reference);
            AudioInstances.Add(reference, audioEvent);
        }

        audioEvent.start();
        return audioEvent;
    }

    /// <summary>
    /// Plays an FMOD sound using a reference. Just add an EventReference and setup the sound
    /// in the inspector. See the FMOD Guide in resources for more info.
    /// </summary>
    /// <param name="reference">the desired sound reference</param>
    /// <param name="target">gameObject the sound plays from</param>
    /// <returns>an EventInstance, save it if the sound needs to be stopped or has a parameter</returns>
    public EventInstance PlaySound(EventReference reference, GameObject target)
    {
        if (reference.IsNull)
        {
            Debug.LogWarning("NO REFERENCE SOUND!");
            return default;
        }

        EventInstance audioEvent = RuntimeManager.CreateInstance(reference);

        RuntimeManager.AttachInstanceToGameObject(audioEvent, target.transform);

        audioEvent.start();
        return audioEvent;
    }

    /// <summary>
    /// Similar to play sound, stops an FMOD sound using a reference.
    /// </summary>
    /// <param name="audioEvent">the desired sound instance</param>
    /// <param name="fade">toggles wether the sound will fade when done playing</param>
    public void StopSound(EventInstance audioEvent, bool fade = false)
    {
        if (audioEvent.isValid())
        {
            audioEvent.stop(fade ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
        }
        else
        {
            Debug.LogWarning("Null audioEvent. Please use a real event instance.");
        }
    }

    /// <summary>
    /// Toggles a sound, pausing and unpausing a specified sound.
    /// </summary>
    /// <param name="audioEvent">the desired sound instance</param>
    /// <param name="paused">true if paused, false if not</param>
    public void PauseSound(EventInstance audioEvent, bool paused)
    {
        if (audioEvent.isValid())
        {
            audioEvent.setPaused(paused);
        }
        else
        {
            Debug.LogWarning("Null audioEvent. Please use a real event instance.");
        }
    }

    /// <summary>
    /// Changes the current volume instantly
    /// </summary>
    /// <param name="audioEvent">the desired sound instance</param>
    /// <param name="goalVolume">the target volume</param>
    public void AdjustVolume(EventInstance audioEvent, float goalVolume)
    {
        if (audioEvent.isValid())
        {
            audioEvent.setVolume(goalVolume);
        }
        else
        {
            Debug.LogWarning("Null audioEvent. Please use a real event instance.");
        }
    }

    /// <summary>
    /// Lerps the volume over amount of seconds.
    /// </summary>
    /// <param name="audioEvent">the desired sound instance</param>
    /// <param name="goalVolume">the target volume (range 0-10)</param>
    /// <param name="duration">the speed volume fades (in seconds)</param>
    public void AdjustVolume(EventInstance audioEvent, float goalVolume, float duration)
    {
        if (audioEvent.isValid())
        {
            StartCoroutine(Fade(audioEvent, goalVolume, duration));
        }
        else
        {
            Debug.LogWarning("Null audioEvent. Please use a real event instance.");
        }
    }

    /// <summary>
    /// Lerps the volume over amount of seconds.
    /// </summary>
    /// <param name="audioEvent">the desired sound instance</param>
    /// <param name="goalVolume">the target volume (range 0-10)</param>
    /// <param name="duration">the speed volume fades (in seconds)</param>
    private IEnumerator Fade(EventInstance audioEvent, float goalVolume, float duration)
    {
        if (goalVolume < 0)
            goalVolume = 0;
        else if (goalVolume > 10)
            goalVolume = 10;

        float timeElapsed = 0;

        audioEvent.getVolume(out var currentVolume);
        float startValue = currentVolume;
        while (timeElapsed < duration)
        {
            var newVolume = Mathf.Lerp(startValue, goalVolume, timeElapsed / duration);
            audioEvent.setVolume(newVolume);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        audioEvent.setVolume(goalVolume);
    }

    /// <summary>
    /// Internal function to convert event references to instances
    /// </summary>
    /// <param name="reference">desired reference to convert</param>
    /// <returns>an event instance</returns>
    private EventInstance ConvertReferenceToInstance(EventReference reference)
    {
        EventInstance audioEvent;

        AudioInstances.TryGetValue(reference, out audioEvent);
        return audioEvent;
    }

    /// <summary>
    /// Stops sounds and clears the instances dictionary if the manager is
    /// destroyed.
    /// </summary>
    private void OnDestroy()
    {
        // StopAllSounds();
        // AudioInstances.Clear();
    }

    /// <summary>
    /// Stops all sounds.
    /// </summary>
    public void StopAllSounds()
    {
        var bus = RuntimeManager.GetBus("bus:/");
        if (bus.isValid())
        {
            bus.stopAllEvents(STOP_MODE.IMMEDIATE);
        }
    }
}