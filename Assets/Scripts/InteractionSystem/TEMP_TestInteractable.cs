/******************************************************************
*    Author: Nick Grinstead
*    Contributors: 
*    Date Created: 10/10/24
*    Description: Testing script being used in GYM_InteractionTesting.
*    Prints a message when interacted with.
*******************************************************************/
using UnityEngine;

public class TEMP_TestInteractable : MonoBehaviour, IInteractable
{
    public GameObject GetGameObject { get => gameObject; }

    /// <summary>
    /// Called by PlayerInteraction to print a message
    /// </summary>
    public void OnInteract()
    {
        Debug.Log("You successfully interacted with an object");
    }
}
