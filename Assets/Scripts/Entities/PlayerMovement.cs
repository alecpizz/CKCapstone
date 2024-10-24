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
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;

public class PlayerMovement : MonoBehaviour, IGridEntry
{
    private PlayerControls _input;
    public Vector3 FacingDirection { get; private set; }

    public bool playerMoved = true;
    public bool enemiesMoved = true;
    public bool IsTransparent { get => true; }
    public Vector3 Position { get => transform.position; }
    public GameObject GetGameObject { get => gameObject; }

    [SerializeField]
    private Vector3 _positionOffset;

    [SerializeField]
    private float delayTime = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        FacingDirection = new Vector3(0, 0, 0);

        GridBase.Instance.AddEntry(this);

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
    }

    public void MovementPerformed(InputAction.CallbackContext context)
    {
        Vector2 key = context.ReadValue<Vector2>();
        Vector3 direction = new(key.x, 0, key.y);
        Vector3 lastPos = gameObject.transform.position;

        // Move if there is no wall below the player or if ghost mode is enabled
        var move = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, direction);
        if ((GridBase.Instance.CellIsEmpty(move) && enemiesMoved == true) ||
            (DebugMenuManager.Instance.GhostMode && enemiesMoved == true))
        {
            playerMoved = true;
            gameObject.transform.position = move + _positionOffset;
            GridBase.Instance.UpdateEntry(this);
            StartCoroutine(delayNextInput());
        }

        if (gameObject.transform.position == lastPos)
        {
            playerMoved = false;
        }
    }

    IEnumerator delayNextInput()
    {
        yield return null;

        if (GameObject.FindGameObjectsWithTag("Enemy") != null)
        {
            yield return new WaitForSeconds(delayTime);
            enemiesMoved = true;
        }
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
}
