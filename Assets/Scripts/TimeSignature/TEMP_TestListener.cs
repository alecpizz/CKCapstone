/******************************************************************
*    Author: Nick Grinstead
*    Contributors: 
*    Date Created: 10/20/24
*    Description: Testing script for the time signature mechanic.
*    Can also serve as a reference for how to set up an ITimeListener.
*******************************************************************/
using UnityEngine;

public class TEMP_TestListener : MonoBehaviour, ITimeListener
{
    private TimeSignatureManager _timeSigManager;

    /// <summary>
    /// Registers to a TimeSignatureManager instance
    /// </summary>
    private void Start()
    { 
        _timeSigManager = TimeSignatureManager.Instance;

        if (_timeSigManager != null )
        {
            _timeSigManager.RegisterTimeListener(this);
        }
    }

    /// <summary>
    /// Unregisters from a TimeSignatureManager instance
    /// </summary>
    private void OnDisable()
    {
        if (_timeSigManager != null)
        {
            _timeSigManager.UnregisterTimeListener(this);
        }
    }

    /// <summary>
    /// Prints out a message containing the new time signature
    /// </summary>
    /// <param name="newTimeSignature">Updated time signature</param>
    public void UpdateTimingFromSignature(Vector2Int newTimeSignature)
    {
        Debug.Log("New player time: " + newTimeSignature.x +
            "\nNew Enemy Time: " +  newTimeSignature.y);
    }
}
