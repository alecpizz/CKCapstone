/******************************************************************
*    Author: Nick Grinstead
*    Contributors: David Henvick, Alec Pizziferro
*    Date Created: 10/10/24
*    Description: Testing script being used in GYM_InteractionTesting.
*    Prints a message when interacted with.
*******************************************************************/
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class TEMP_TestInteractable : MonoBehaviour, IInteractable
{
    public GameObject GetGameObject { get => gameObject; }

    /// <summary>
    /// Called by PlayerInteraction to print a message
    /// </summary>
    public void OnInteract()
    {
        UnityEngine.Debug.Log("You successfully interacted with an object");
    }
    /// <summary>
    /// Called by PlayerInteraction to print a message
    /// </summary>
    public void OnLeave()
    {
        UnityEngine.Debug.Log("You successfully left an interaction with an object");
    }

    public void OnEnter()
    {
        Debug.Log("You entered an interaction");
    }
}
