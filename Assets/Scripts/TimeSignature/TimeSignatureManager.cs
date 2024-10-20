using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class TimeSignatureManager : MonoBehaviour
{
    public static TimeSignatureManager Instance;

    [InfoBox("First number is player time, second is enemy time", EInfoBoxType.Normal)]
    [OnValueChanged(nameof(ChangeTimeSignatureInEditor))]
    [SerializeField] private Vector2Int _timeSignature;

    private List<ITimeListener> _timeListeners = new List<ITimeListener>();

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
    }

    public void ChangeTimeSignature(Vector2Int newTimeSignature)
    {
        _timeSignature = newTimeSignature;

        foreach (ITimeListener listener in _timeListeners)
        {
            listener.UpdateTimingFromSignature(_timeSignature);
        }
    }

    private void ChangeTimeSignatureInEditor()
    {
        foreach (ITimeListener listener in _timeListeners)
        {
            listener.UpdateTimingFromSignature(_timeSignature);
        }
    }

    public void RegisterTimeListener(ITimeListener listenerToAdd)
    {
        _timeListeners.Add(listenerToAdd);
    }

    public void UnregisterTimeListener(ITimeListener listenerToRemove)
    {
        _timeListeners.Remove(listenerToRemove);
    }
}
