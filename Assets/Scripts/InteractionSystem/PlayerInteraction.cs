using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Required]
    [SerializeField] private PlayerMovement _playerMovement;

    private GridBase _gridBase;
    private PlayerControls _playerControls;

    private void Start()
    {
        _gridBase = GridBase.Instance;

        _playerControls = new PlayerControls();
        _playerControls.Enable();
        _playerControls.InGame.Interact.performed += ctx => Interact();
    }

    private void OnDisable()
    {
        _playerControls.Disable();
        _playerControls.InGame.Interact.performed -= ctx => Interact();
    }

    private void Interact()
    {

    }
}
