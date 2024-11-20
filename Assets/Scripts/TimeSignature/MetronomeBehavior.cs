/******************************************************************
*    Author: Nick Grinstead
*    Contributors: Trinity Hutson
*    Date Created: 10/28/24
*    Description: Checks for collisions with the player and then 
*       calls the TimeSignatureManager to update the time signature.
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MetronomeBehavior : MonoBehaviour
{
    public UnityEvent MetronomeTriggered;

    /// <summary>
    /// Toggles the time signature on the manager if there is one
    /// </summary>
    private void ActivateMetronome()
    {
        if (TimeSignatureManager.Instance != null)
            TimeSignatureManager.Instance.ToggleTimeSignature();

        MetronomeTriggered?.Invoke();
    }

    /// <summary>
    /// Activates the metronome and stops player movement in response 
    /// to a collision with the player.
    /// </summary>
    /// <param name="other">Data from a collision</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            ActivateMetronome();

            PlayerMovement playerMovement;
            if (other.gameObject.TryGetComponent<PlayerMovement>(out playerMovement))
            {
                playerMovement.ForceTurnEnd();
            }
        }
    }
}
