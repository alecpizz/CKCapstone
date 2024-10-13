/******************************************************************
*    Author: Nick Grinstead
*    Contributors: 
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
}
