/******************************************************************
*    Author: Nick Grinstead
*    Contributors: Rider Hagen, Trinity Hutson
*    Date Created: 10/20/24
*    Description: Script for tracking the current time signature.
*    Has methods for updating to a new time signature and pushs updates
*    to any registered ITimeListener scripts.
*******************************************************************/
using System.Collections.Generic;
using UnityEngine;
using SaintsField;
using TMPro;
using UnityEngine.UI;
using FMODUnity;

public class TimeSignatureManager : MonoBehaviour
{
    public static TimeSignatureManager Instance;

    public bool TimeSigInUse { get; private set; } = false;

    public bool MetronomeInUse { get; private set; } = false;

    [InfoBox("First number is player time, second is enemy time", EMessageType.Info)]
    [OnValueChanged(nameof(UpdateListeners))]
    [SerializeField] private Vector2Int _timeSignature;

    [InfoBox("This is the time signature that metronomes will toggle to", EMessageType.Info)]
    [SerializeField] private Vector2Int _secondaryTimeSignature;
    private Vector2Int _startingTimeSignature;
    private bool _isToggled = false;

    [SerializeField] private TMP_Text _metronomePredictor;

    private List<ITimeListener> _timeListeners = new List<ITimeListener>();

    [Header("Juice")]
    [SerializeField]
    private ParticleSystem _timeSigChangeParticles;
    [SerializeField]
    private EventReference _timeSigChangeSFX;

    /// <summary>
    /// Creating an instance and ensuring time signature has valid values
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }

        Instance = this;

        if (_timeSignature.x <= 0)
        {
            _timeSignature.x = 1;
        }
        if (_timeSignature.y <= 0)
        {
            _timeSignature.y = 1;
        }

        if (_secondaryTimeSignature.x <= 0)
        {
            _secondaryTimeSignature.x = 1;
        }
        if (_secondaryTimeSignature.y <= 0)
        {
            _secondaryTimeSignature.y = 1;
        }

        _startingTimeSignature = _timeSignature;

        if (_metronomePredictor != null)
        {
            _metronomePredictor.text = _secondaryTimeSignature.x + "/" + _secondaryTimeSignature.y;
        }

        TimeSigInUse = _timeSignature != Vector2Int.one || _secondaryTimeSignature != Vector2Int.one;

        MetronomeInUse = FindFirstObjectByType(typeof(MetronomeBehavior)) != null;
    }

    private void Start()
    {
        if (TimeSigInUse)
            PlayJuice();
    }

    /// <summary>
    /// Called by metronomes to toggle between two time signatures
    /// </summary>
    public void ToggleTimeSignature()
    {
        _isToggled = !_isToggled;

        _timeSignature = _isToggled ? _secondaryTimeSignature : _startingTimeSignature;

        UpdateListeners();
    }

    /// <summary>
    /// Fetches the current time signature
    /// </summary>
    /// <returns>Current Time Signature</returns>
    public Vector2Int GetCurrentTimeSignature()
    {
        return !_isToggled ? _startingTimeSignature : _secondaryTimeSignature;
    }

    /// <summary>
    /// Fetches the next time signature to be used
    /// </summary>
    /// <returns>Next Time Signature that will be swapped to if metronome is interacted with</returns>
    public Vector2Int GetNextTimeSignature()
    {
        return _isToggled ? _startingTimeSignature : _secondaryTimeSignature;
    }

    /// <summary>
    /// Method for updating the listeners that are registered to this manager
    /// </summary>
    private void UpdateListeners()
    {
        foreach (ITimeListener listener in _timeListeners)
        {
            listener.UpdateTimingFromSignature(_timeSignature);
        }
    }

    /// <summary>
    /// Method called by ITimeListener scripts to register themselves to this manager
    /// </summary>
    /// <param name="listenerToAdd">The script to register</param>
    public void RegisterTimeListener(ITimeListener listenerToAdd)
    {
        _timeListeners.Add(listenerToAdd);

        UpdateListeners();
    }

    /// <summary>
    /// Method called by ITimeListener scripts to unregister themselves to this manager
    /// </summary>
    /// <param name="listenerToRemove">The script to unregister</param>
    public void UnregisterTimeListener(ITimeListener listenerToRemove)
    {
        _timeListeners.Remove(listenerToRemove);
    }

    /// <summary>
    /// Plays SFX and VFX to draw attention to a changed time signature
    /// </summary>
    public void PlayJuice(bool playSFX = true, bool playVFX = true)
    {
        if(playSFX)
            AudioManager.Instance.PlaySound(_timeSigChangeSFX);
        if (playVFX)
            _timeSigChangeParticles.Play();
    }
}
