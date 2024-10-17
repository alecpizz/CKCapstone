/******************************************************************
*    Author: Cole Stranczek
*    Contributors: Cole Stranczek, Mitchell Young
*    Date Created: 10/3/24
*    Description: Script that handles the behavior of the enemy,
*    from movement to causing a failstate with the player
*******************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyAttributes;

public class EnemyBehavior : MonoBehaviour, IGridEntry
{
    public bool IsTransparent { get => true; }
    public Vector3 Position { get => transform.position; }
    public GameObject GetGameObject { get => gameObject; }

    private PlayerControls _input;

    [Required][SerializeField] private GameObject _player;

    [SerializeField] private bool _atStart;
    [SerializeField] private int _currentPoint = 0;

    private PlayerMovement _playerMoveRef;
    [SerializeField] private float _moveRate = 1f;
    [SerializeField] private float _waitTime = 1f;

    //List of Move Point Scriptable Objects
    [SerializeField] private List<MovePoints> _movePoints;

    public bool enemyFrozen = false;


    // Start is called before the first frame update
    void Start()
    {
        // Getting player input for the sake of triggering enemy movement.
        // There's 100% a better way to do this but every attempt Mitchell
        // and I made to just get a bool from the player script to signal
        // when it's moving didn't work, so this was just done to get 
        // SOMETHING working
        _input = new PlayerControls();
        _input.InGame.Enable();
        _input.InGame.MoveUp.performed += EnemyMove;
        _input.InGame.MoveDown.performed += EnemyMove;
        _input.InGame.MoveLeft.performed += EnemyMove;
        _input.InGame.MoveRight.performed += EnemyMove;

        _playerMoveRef = _player.GetComponent<PlayerMovement>();
        //GridBase.Instance.AddEntry(this);

        // Make sure enemiess are always seen at the start
        _atStart = true;
    }


    /// <summary>
    /// Unregisters from player input
    /// </summary>
    private void OnDisable()
    {
        _input.InGame.Disable();

        _input.InGame.MoveUp.performed -= EnemyMove;
        _input.InGame.MoveDown.performed -= EnemyMove;
        _input.InGame.MoveLeft.performed -= EnemyMove;
        _input.InGame.MoveRight.performed -= EnemyMove;
    }

    /// <summary>
    /// Function that handles the enemy's movement along the provided points in the list
    /// </summary>
    /// <param name="obj"></param>
    public void EnemyMove(InputAction.CallbackContext obj)
    {


        StartCoroutine(DelayedPlayerInput());
    }

    /// <summary>
    /// Delays the input from the player by 1 frame so that the player moves before enemies lock out their movement
    /// </summary>
    /// <returns></returns>
    IEnumerator DelayedPlayerInput()
    {
        yield return null;

        if (_playerMoveRef.enemiesMoved == true && enemyFrozen == false)
        {
            if (_atStart)
            {
                _currentPoint++;
                _playerMoveRef.enemiesMoved = false;

                if (_currentPoint == movePoints.Count - 1)
                {
                    _atStart = false;
                }
            }
            else
            {
                _currentPoint--;
                _playerMoveRef.enemiesMoved = false;

                if (_currentPoint == 0)
                {
                    _atStart = true;
                }
            }
        }
    }
}
