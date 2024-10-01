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

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private EventReference _enemy = default;
    [SerializeField] [Range(0, 4)] private int _musicLayering = 0;

    Dictionary<EventReference, EventInstance> AudioInstances;

    private EventInstance _key;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(Instance.gameObject);
        
        Instance = this;
        AudioInstances = new Dictionary<EventReference, EventInstance>();
    }

    void Start()
    {
        _key = PlaySound(_enemy);
    }

    void Update()
    {
        _key.setParameterByName("MusicLayering", _musicLayering);
    }

    /// <summary>
    /// Plays an FMOD sound using a reference. Just add an eventreference and setup the sound
    /// in the inspector. See the FMOD Guide in resources for more info.
    /// </summary>
    /// <param name="reference">the desired sound reference</param>
    /// <returns>an EventInstance, save it if you need to use a parameter</returns>
    public EventInstance PlaySound(EventReference reference)
    {
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
    /// Plays an FMOD sound using a reference. Just add an eventreference and setup the sound
    /// in the inspector. See the FMOD Guide in resources for more info.
    /// </summary>
    /// <param name="reference">the desired sound reference</param>
    /// <param name="gameObject">gameobject the sound plays from</param>
    /// <returns>an EventInstance, save it if the sound needs to be stopped or has a parameter</returns>
    public EventInstance PlaySound(EventReference reference, GameObject gameObject)
    {
        EventInstance audioEvent = RuntimeManager.CreateInstance(reference);

        RuntimeManager.AttachInstanceToGameObject(audioEvent, gameObject.transform);

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
        audioEvent.stop(fade ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
    }

    /// <summary>
    /// Toggles a sound, pausing and unpausing a specified sound.
    /// </summary>
    /// <param name="audioEvent">the desired sound instance</param>
    /// <param name="paused">true if paused, false if not</param>
    public void ToggleSound(EventInstance audioEvent, bool paused)
    {
        audioEvent.setPaused(paused);
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

    private void OnDestroy()
    {
        StopAllSounds();
        AudioInstances.Clear();
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
