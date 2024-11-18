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
    private HashSet<IGridEntry> _gridEntries;
    private IInteractable _currentInteractable;
    private Vector3 _facingDirection;

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
        _currentInteractable?.OnInteract();
    }

    /// <summary>
    /// Attempts to find an interactable in a nearby cell.
    /// </summary>
    /// <returns>The found interactable, can be null.</returns>
    private IInteractable TryGetInteractable()
    {
        Vector3 cellPositionToCheck = 
            _gridBase.GetCellPositionInDirection(transform.position, 
                _facingDirection);
        Vector3Int cellCoordinatesToCheck = _gridBase.WorldToCell(cellPositionToCheck);
        if (_gridBase.CellIsEmpty(cellCoordinatesToCheck))
        {
            return null;
        }
        _gridEntries = _gridBase.GetCellEntries(cellCoordinatesToCheck);
        foreach (var entry in _gridEntries)
        {
            if (entry.GetGameObject.TryGetComponent<IInteractable>(out var interactable))
            {
                return interactable;
            }
        }
        return null;
    }

    /// <summary>
    /// This is a function built to do two things. 
    /// first it is built so that this script has access to the current direction that the player is facing, without circular dependencies.
    /// second is to call the OnLeave function for the current interactable and to clear that very same variable after.
    /// </summary>
    /// <param name="direction"></param> a vector that represents the direction the player is currently facing.
    public void SetDirection(Vector3 direction)
    {
        if (_currentInteractable != null)
        {
            _currentInteractable.OnLeave();
            _currentInteractable = null;
        }
        _facingDirection = direction;
        var interactable = TryGetInteractable();
        _currentInteractable = interactable;
        interactable?.OnEnter();
    }
}
