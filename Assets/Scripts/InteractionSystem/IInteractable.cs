using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IInteractable : MonoBehaviour
{
    /// <summary>
    /// Field to retrieve attached GameObject
    /// </summary>
    public GameObject AttachedGameObject
        { get => gameObject; private set => AttachedGameObject = value; }

    /// <summary>
    /// This function will be implemented to contain the specific functionality
    /// for an interactable object
    /// </summary>
    public void OnInteract()
    {
        throw new System.NotImplementedException("OnInteract not implemented");
    }
}
