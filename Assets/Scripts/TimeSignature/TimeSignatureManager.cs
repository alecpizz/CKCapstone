/******************************************************************
*    Author: Nick Grinstead
*    Contributors: 
*    Date Created: 10/20/24
*    Description: Script for tracking the current time signature.
*    Has methods for updating to a new time signature and pushs updates
*    to any registered ITimeListener scripts.
*******************************************************************/
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class TimeSignatureManager : MonoBehaviour
{
    public static TimeSignatureManager Instance;

    [InfoBox("First number is player time, second is enemy time", EInfoBoxType.Normal)]
    [OnValueChanged(nameof(UpdateListeners))]
    [SerializeField] private Vector2Int _timeSignature;

    [InfoBox("This is the time signature that metronomes will toggle to", EInfoBoxType.Normal)]
    [SerializeField] private Vector2Int _secondaryTimeSignature;
    private Vector2Int _startingTimeSignature;
    private bool _isToggled = false;

    private List<ITimeListener> _timeListeners = new List<ITimeListener>();

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
}
