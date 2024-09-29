/******************************************************************
*    Author: Claire Noto
*    Contributors: Claire Noto
*    Date Created: 09/19/2024
*    Description: Audio Manager using FMOD. See FMOD documentation 
*    for more info
*******************************************************************/
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] EventReference enemy = default;
    Dictionary<EventReference, EventInstance> AudioInstances;

    [SerializeField] [Range(0, 4)] private int MusicLayering = 0;

    private EventInstance key;

    public static AudioManager instance;

    private void Awake()
    {
        instance = this;
        AudioInstances = new Dictionary<EventReference, EventInstance>();
    }

    void Start()
    {
        key = PlaySound(enemy);
    }

    private bool paused = false;

    void Update()
    {
        key.setParameterByName("MusicLayering", MusicLayering);
    }

    /// <summary>
    /// Plays an FMOD sound using a reference. Just add an eventreference and setup the sound
    /// in the inspector. See the FMOD Guide in resources for more info.
    /// </summary>
    /// <param name="reference">the desired sound reference</param>
    /// <param name="gameObject">gameobject the sound plays from</param>
    /// <returns>an EventInstance, save it if you need to use a parameter</returns>
    public EventInstance PlaySound(EventReference reference, GameObject gameObject = null)
    {
        EventInstance audioEvent;

        if (AudioInstances.ContainsKey(reference))
            AudioInstances.TryGetValue(reference, out audioEvent);
        else
        {
            audioEvent = RuntimeManager.CreateInstance(reference);
            AudioInstances.Add(reference, audioEvent);
        }

        if (gameObject)
        {
            RuntimeManager.AttachInstanceToGameObject(audioEvent, gameObject.transform);
        }

        audioEvent.start();
        return audioEvent;
    }

    /// <summary>
    /// Similar to play sound, stops an FMOD sound using a reference. 
    /// </summary>
    /// <param name="reference">the desired sound reference</param>
    /// <param name="fade">toggles wether the sound will fade when done playing</param>
    public void StopSound(EventReference reference, bool fade = false)
    {
        EventInstance audioEvent = ConvertReferenceToInstance(reference);

        RuntimeManager.DetachInstanceFromGameObject(audioEvent);

        if (fade)
            audioEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        else
            audioEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }


    /// <summary>
    /// Toggles a sound, pausing and unpausing a specified sound.
    /// </summary>
    /// <param name="reference">the desired sound reference</param>
    /// <param name="paused">true if paused, false if not</param>
    public void ToggleSound(EventReference reference, bool paused)
    {
        EventInstance audioEvent = ConvertReferenceToInstance(reference);

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
}
