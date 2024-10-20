using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEMP_TestListener : MonoBehaviour, ITimeListener
{
    private TimeSignatureManager _timeSigManager;

    private void Start()
    { 
        _timeSigManager = TimeSignatureManager.Instance;

        if (_timeSigManager != null )
        {
            _timeSigManager.RegisterTimeListener(this);
        }
    }

    private void OnDisable()
    {
        if (_timeSigManager != null)
        {
            _timeSigManager.UnregisterTimeListener(this);
        }
    }

    public void UpdateTimingFromSignature(Vector2Int newTimeSignature)
    {
        Debug.Log("New player time: " + newTimeSignature.x +
            "\nNew Enemy Time: " +  newTimeSignature.y);
    }
}
