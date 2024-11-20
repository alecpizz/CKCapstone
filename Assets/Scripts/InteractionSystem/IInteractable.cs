/******************************************************************
*    Author: Nick Grinstead
*    Contributors: David Henvick, Alec Pizziferro
*    Date Created: 10/10/24
*    Description: Interface for objects that the player can interact
*    with.
*******************************************************************/
using UnityEngine;

public interface IInteractable
{
    /// <summary>
    /// Field to retrieve attached GameObject
    /// </summary>
    GameObject GetGameObject { get; }


    /// <summary>
    /// This function will be implemented to contain the specific functionality
    /// for an interactable object
    /// </summary>
    public void OnInteract();

    /// <summary>
    /// This function will be implemented for when the player is no longer interacting with the interactable
    /// </summary>
    public void OnLeave();
}
