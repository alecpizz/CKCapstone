/******************************************************************
*    Author: Cole Stranczek
*    Contributors: Cole Stranczek, Nick Grinstead, Alex Laubenstein, Trinity Hutson, Alec Pizziferro
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

public class PlayerMovement : MonoBehaviour, IGridEntry, ITurnListener
{
    private PlayerControls _input;
    public Vector3 FacingDirection { get; private set; }

    public bool playerMoved = false;
    public bool enemiesMoved = true;
    public bool IsTransparent { get => true; }
    public Vector3 Position { get => transform.position; }
    public GameObject GetGameObject { get => gameObject; }

    [SerializeField]
    private Vector3 _positionOffset;
    [SerializeField]
    private PlayerInteraction _playerInteraction;

    [SerializeField]
    private float delayTime = 0.5f;

    private bool _enemiesPresent = true;

    // Start is called before the first frame update
    void Start()
    {
        FacingDirection = new Vector3(0, 0, 0);

        GridBase.Instance.AddEntry(this);

        // Referencing and setup of the Input Action functions

        if (GameObject.FindGameObjectsWithTag("Enemy") == null)
        {
            _enemiesPresent = false;
        }
    }

    private void OnEnable()
    {
        RoundManager.Instance.RegisterListener(this);
    }

    /// <summary>
    /// Unregistering from input actions
    /// </summary>
    private void OnDisable()
    {
        RoundManager.Instance.UnRegisterListener(this);
    }
    

    /// <summary>
    /// Coroutine that makes the player wait to let the enemies finish moving before
    /// being able to move again.
    /// </summary>
    IEnumerator DelayNextInput()
    {
        yield return null;

        if (_enemiesPresent)
        {
            yield return new WaitForSeconds(delayTime);
            enemiesMoved = true;
        }
        RoundManager.Instance.CompleteTurn(this);
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

    public TurnState TurnState { get; }
    public void BeginTurn(Vector3 direction)
    {
        _playerInteraction.SetDirection(direction);
        var move = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, direction);
        if (!GridBase.Instance.CellIsEmpty(move))
        {
            playerMoved = false;
            StartCoroutine(DelayNextInput());
        }

        if ((GridBase.Instance.CellIsEmpty(move)) ||
            (DebugMenuManager.Instance.GhostMode))
        {
            playerMoved = true;
            gameObject.transform.position = move + _positionOffset;
            GridBase.Instance.UpdateEntry(this);
            StartCoroutine(DelayNextInput());
        }
    }
}
