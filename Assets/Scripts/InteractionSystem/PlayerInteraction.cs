/******************************************************************
 *    Author: Nick Grinstead
 *    Contributors: Alec Pizziferro
 *    Date Created: 10/10/24
 *    Description: This script should be attached to the player character,
 *    and handles checking an adjacent square for interactables when
 *    an input is given.
 *******************************************************************/

using System.Collections.Generic;
using UnityEngine;
using SaintsField;

public class PlayerInteraction : MonoBehaviour
{
    private GridBase _gridBase;
    private PlayerControls _playerControls;
    private IInteractable _currentInteractable;

    /// <summary>
    /// Enabling inputs and getting grid instance
    /// </summary>
    private void Start()
    {
        _gridBase = GridBase.Instance;

        _playerControls = new PlayerControls();
        _playerControls.Enable();
        _playerControls.InGame.Interact.performed += ctx => Interact();
        _currentInteractable = null;
    }

    /// <summary>
    /// Disabling inputs
    /// </summary>
    private void OnDisable()
    {
        _playerControls.Disable();
        _playerControls.InGame.Interact.performed -= ctx => Interact();
    }

    /// <summary>
    /// Method is invoked whenever the player presses the interact input.
    /// It checks the square the player is facing for IInteractables and calls
    /// their OnInteract() method
    /// </summary>
    private void Interact()
    {
        //NOTE: this will interact with multiple interactables at once. design should avoid this.
        Vector3Int fwd =
            _gridBase.WorldToCell(_gridBase.GetCellPositionInDirection(transform.position, Vector3.forward));
        Vector3Int back =
            _gridBase.WorldToCell(_gridBase.GetCellPositionInDirection(transform.position, Vector3.back));
        Vector3Int right =
            _gridBase.WorldToCell(_gridBase.GetCellPositionInDirection(transform.position, Vector3.right));
        Vector3Int left =
            _gridBase.WorldToCell(_gridBase.GetCellPositionInDirection(transform.position, Vector3.left));

        var fwdEntries = _gridBase.GetCellEntries(fwd);
        var backEntries = _gridBase.GetCellEntries(back);
        var leftEntries = _gridBase.GetCellEntries(right);
        var rightEntries = _gridBase.GetCellEntries(left);
        // Checking if there are objects in the adjacent square
        InteractWithCell(ref fwdEntries);
        InteractWithCell(ref backEntries);
        InteractWithCell(ref leftEntries);
        InteractWithCell(ref rightEntries);
    }

    private void InteractWithCell(ref HashSet<IGridEntry> entries)
    {
        foreach (var entry in entries)
        {
            if (!entry.EntryObject.TryGetComponent<IInteractable>(out var interactable)) continue;
            _currentInteractable = interactable;
            interactable.OnInteract();
        }
    }
}