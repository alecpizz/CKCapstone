/******************************************************************
*    Author: Nick Grinstead
*    Contributors: 
*    Date Created: 10/10/24
*    Description: This script should be attached to the player character,
*    and handles checking an adjacent square for interactables when
*    an input is given.
*******************************************************************/
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class PlayerInteraction : MonoBehaviour
{
    [Required]
    [SerializeField] private PlayerMovement _playerMovement;

    private GridBase _gridBase;
    private PlayerControls _playerControls;
    private HashSet<IGridEntry> _gridEntries;

    /// <summary>
    /// Enabling inputs and getting grid instance
    /// </summary>
    private void Start()
    {
        _gridBase = GridBase.Instance;

        _playerControls = new PlayerControls();
        _playerControls.Enable();
        _playerControls.InGame.Interact.performed += ctx => Interact();
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
        // Finding coordinates of the adjacent square
        Vector3 cellPositionToCheck = 
            _gridBase.GetCellPositionInDirection(transform.position, 
            _playerMovement.FacingDirection);

        Vector3Int cellCoordinatesToCheck = _gridBase.WorldToCell(cellPositionToCheck);

        // Checking if there are objects in the adjacent square
        if (_gridBase.CellIsEmpty(cellCoordinatesToCheck)) { return; }

        // Checking the square's entries for any interactables
        _gridEntries = _gridBase.GetCellEntries(cellCoordinatesToCheck);
        foreach (var entry in _gridEntries)
        {
            IInteractable interactable;
            if (entry.GetGameObject.TryGetComponent<IInteractable>(out interactable))
            {
                interactable.OnInteract();
            }
        }
    }
}
