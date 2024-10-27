/******************************************************************
*    Author: Nick Grinstead
*    Contributors: 
*    Date Created: 10/20/24
*    Description: Interface for objects that need to know the current
*    time signature.
*******************************************************************/
using UnityEngine;

public interface ITimeListener
{
    /// <summary>
    /// Method that's called by the TimeSignatureManager
    /// Should be implemented with logic to use the received time signature
    /// </summary>
    /// <param name="newTimeSignature">Updated time signature</param>
    public void UpdateTimingFromSignature(Vector2Int newTimeSignature);
}
