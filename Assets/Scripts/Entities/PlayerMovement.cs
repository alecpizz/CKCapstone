/******************************************************************
*    Author: Cole Stranczek
*    Contributors: Cole Stranczek, Nick Grinstead, Alex Laubenstein, Trinity Hutson
*    Date Created: 9/22/24
*    Description: Script that handles the player's movement along
*    the grid
*******************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;

public class PlayerMovement : MonoBehaviour, IGridEntry, ITimeListener
{
    public Action PlayerFinishedMoving;

    private PlayerControls _input;
    public Vector3 FacingDirection { get; private set; }

    public bool PlayerMoved { get => _playerMovementComplete; 
        private set => _playerMovementComplete = value; }
    public bool enemiesMoved = true;
    public bool IsTransparent { get => true; }
    public Vector3 Position { get => transform.position; }
    public GameObject GetGameObject { get => gameObject; }

    [SerializeField]
    private Vector3 _positionOffset;
    [SerializeField]
    private PlayerInteraction _playerInteraction;

    [SerializeField]
    private float _delayTime = 0.1f;

    private bool _playerMovementComplete = true;
    private int _playerMovementTiming = 1;
    private TimeSignatureManager _timeSigManager;

    // Start is called before the first frame update
    void Start()
    {
        FacingDirection = new Vector3(0, 0, 0);

        GridBase.Instance.AddEntry(this);

        _timeSigManager = TimeSignatureManager.Instance;
        if (_timeSigManager != null)
        {
            _timeSigManager.RegisterTimeListener(this);
        }

        // Referencing and setup of the Input Action functions
        _input = new PlayerControls();
        _input.InGame.Enable();
        _input.InGame.Movement.performed += MovementPerformed;
    }

    /// <summary>
    /// Unregistering from input actions
    /// </summary>
    private void OnDisable()
    {
        _input.InGame.Disable();
        _input.InGame.Movement.performed -= MovementPerformed;

        if (_timeSigManager != null)
        {
            _timeSigManager.UnregisterTimeListener(this);
        }
    }

    /// <summary>
    /// Moves the player to the next grid tile if able
    /// </summary>
    /// <param name="context">Input callback context</param>
    public void MovementPerformed(InputAction.CallbackContext context)
    {
        Vector2 key = context.ReadValue<Vector2>();
        Vector3 direction = new(key.x, 0, key.y);

        _playerInteraction.SetDirection(direction);

        var move = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, direction);

        if ((GridBase.Instance.CellIsEmpty(move) || DebugMenuManager.Instance.GhostMode) &&
                _playerMovementComplete && enemiesMoved)
        {
            _playerMovementComplete = false;
            StartCoroutine(MovementDelay(direction));
        }
    }

    /// <summary>
    /// Helper coroutine for performing movement with a delay
    /// </summary>
    /// <param name="moveDirection">Direction of player movement</param>
    /// <returns>Waits for short delay while moving</returns>
    private IEnumerator MovementDelay(Vector3 moveDirection)
    {
        for (int i = 0; i < _playerMovementTiming; i++)
        {
            // Move if there is no wall below the player or if ghost mode is enabled
            var move = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, moveDirection);
            if ((GridBase.Instance.CellIsEmpty(move)) ||
                (DebugMenuManager.Instance.GhostMode))
            {
                gameObject.transform.position = move + _positionOffset;
                GridBase.Instance.UpdateEntry(this);
            }
            else
            {
                break;
            }

            yield return new WaitForSeconds(_delayTime);
        }

        _playerMovementComplete = true;
        PlayerFinishedMoving?.Invoke();
    }

    /// <summary>
    /// Reloads scene when player hits an enemy
    /// </summary>
    /// <param name="collision">Data from collision</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Checks if the enemy is frozen; if they are, doesn't reload the scene
            EnemyBehavior enemy = collision.collider.GetComponent<EnemyBehavior>();
            if (enemy == null || enemy.enemyFrozen)
                return;

            Time.timeScale = 0f;

            SceneController.Instance.ReloadCurrentScene();
        }
    }

    /// <summary>
    /// Receives the new player movement speed when time signature updates
    /// </summary>
    /// <param name="newTimeSignature">The new time signature</param>
    public void UpdateTimingFromSignature(Vector2Int newTimeSignature)
    {
        _playerMovementTiming = newTimeSignature.x;

        if (_playerMovementTiming <= 0)
            _playerMovementTiming = 1;
    }
}
