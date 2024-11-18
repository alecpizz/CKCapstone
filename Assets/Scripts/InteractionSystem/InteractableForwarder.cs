/******************************************************************
*    Author: Alec Pizziferro
*    Contributors: Nullptr
*    Date Created: 11/18/2024
*    Description: A forwarder for interactables. Designed to
 *   simply re-locate an interaction.
*******************************************************************/
using SaintsField.Playa;
using UnityEngine;

/// <summary>
/// A forwarder for interactables. Designed to simply re-locate an interaction.
/// </summary>
public class InteractableForwarder : MonoBehaviour, IInteractable
{
    public GameObject GetGameObject { get; }

    [LayoutStart("Settings", ELayout.Background | ELayout.TitleBox)] [SerializeField]
    [Tooltip("Whether the interaction presses should be forwarded.")]
    private bool _forwardInteract = true;
    [Tooltip("Whether leave events should be forwarded.")]
    [SerializeField] private bool _forwardLeave = true;
    [Tooltip("Whether enter events should be forwarded.")]
    [SerializeField] private bool _forwardEnter = true;
    [SerializeField] private InterfaceReference<IInteractable> _targetInteractable;
    
    /// <summary>
    /// Whenever the player presses the interact action, forward the
    /// OnInteract call if possible.
    /// </summary>
    public void OnInteract()
    {
        if (!_forwardInteract)
        {
            return;
        }
        _targetInteractable.Interface?.OnInteract();
    }

    /// <summary>
    /// Whenever the player leaves the interactable,
    /// forward the OnLeave call if possible.
    /// </summary>
    public void OnLeave()
    {
        if (!_forwardLeave)
        {
            return;
        }
        _targetInteractable.Interface?.OnLeave();
    }

    /// <summary>
    /// Whenever the player enters the interactable,
    /// forward the OnEnter call if possible.
    /// </summary>
    public void OnEnter()
    {
        if (!_forwardEnter)
        {
            return;
        }
        _targetInteractable.Interface?.OnEnter();
    }
}