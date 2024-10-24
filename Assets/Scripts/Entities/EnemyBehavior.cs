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
    public Vector3 moveInDirection { get; private set; }
    public Vector3 Position { get => transform.position; }

    [SerializeField]
    private Vector3 _positionOffset;

    public GameObject GetGameObject { get => gameObject; }

    private PlayerControls _input;

    [Required] [SerializeField] private GameObject _player;

    [SerializeField] private bool _atStart;
    [SerializeField] private int _currentPoint = 0;

    private PlayerMovement _playerMoveRef;

    //Wait time between enemy moving each individual tile while on path to next destination
    [SerializeField] private float _waitTime = 0.05f;

    //List of Move Point Scriptable Objects
    [SerializeField] private List<MovePoints> _movePoints;

    public bool enemyFrozen = false;

    // Start is called before the first frame update
    void Start()
    {
        moveInDirection = new Vector3(0, 0, 0);

        GridBase.Instance.AddEntry(this);

        // Getting player input for the sake of triggering enemy movement.
        // There's 100% a better way to do this but every attempt Mitchell
        // and I made to just get a bool from the player script to signal
        // when it's moving didn't work, so this was just done to get 
        // SOMETHING working
        _input = new PlayerControls();
        _input.InGame.Enable();
        _input.InGame.Movement.performed += EnemyMove;

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
        _input.InGame.Movement.performed -= EnemyMove;
    }

    /// <summary>
    /// Function that calls the DelayedInput coroutine
    /// </summary>
    /// <param name="obj"></param>
    public void EnemyMove(InputAction.CallbackContext obj)
    {
        if (_playerMoveRef.enemiesMoved == true)
        {
            StartCoroutine(DelayedInput());
        }
    }

    /// <summary>
    /// Coroutine that handles the enemy's movement along the provided points in the scriptable object list
    /// </summary>
    /// <returns></returns>
    IEnumerator DelayedInput()
    {
        yield return null;

        if (_currentPoint > _movePoints.Count - 1)
        {
            Debug.Log(_movePoints.Count - 1);
            _currentPoint = _movePoints.Count - 1;
        }

        /// <summary>
        /// Checks to see if all enemies have finished moving via a bool in the player script (needs to be reworked) and if the enemy is currently frozen by the harmony beam
        /// </summary>
        /// 
        yield return new WaitForSeconds(0.1f);
        if (_playerMoveRef.playerMoved == true && enemyFrozen == false)
        {
            if (_atStart)
            {
                _playerMoveRef.enemiesMoved = false;

                /// <summary>
                /// Looks at current point the the scriptable object list to pull the current direction (string) and amount of tiles to move in direction (int)
                /// </summary>
                var usingPoint = _movePoints[_currentPoint];
                var pointDirection = usingPoint.direction;
                var pointTiles = usingPoint.tilesToMove;

                if (pointDirection == "Up" || pointDirection == "up")
                {
                    moveInDirection = Vector3.forward;
                }
                if (pointDirection == "Down" || pointDirection == "down")
                {
                    moveInDirection = Vector3.back;
                }
                if (pointDirection == "Left" || pointDirection == "left")
                {
                    moveInDirection = Vector3.left;
                }
                if (pointDirection == "Right" || pointDirection == "right")
                {
                    moveInDirection = Vector3.right;
                }

                /// <summary>
                /// Uses a loop to determine how many tiles to move in a direction based on the current scriptable object's variables
                /// </summary>
                for (int i = 0; i < pointTiles; i++)
                {
                    var move = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, moveInDirection);
                    if (!GridBase.Instance.CellIsEmpty(move) && _playerMoveRef.Position.x != move.x && _playerMoveRef.Position.z != move.z)
                    {
                        Debug.Log(move);
                        Debug.Log(_playerMoveRef.Position);
                        break;
                    }
                    gameObject.transform.position = move + _positionOffset;
                    GridBase.Instance.UpdateEntry(this);

                    yield return new WaitForSeconds(_waitTime);
                }

                /// <summary>
                /// If the current point is equal to the length of the list then the if/else statement 
                /// will check the atStart bool and concurrently reverse through the list
                /// </summary>
                if (_currentPoint >= _movePoints.Count - 1)
                {
                    _atStart = false;
                }
                else
                {
                    _currentPoint++;
                }
            }
            else
            {

                _playerMoveRef.enemiesMoved = false;

                var usingPoint = _movePoints[_currentPoint];
                var pointDirection = usingPoint.direction;
                var pointTiles = usingPoint.tilesToMove;

                //In reverse for going back through the list
                if (pointDirection == "Up" || pointDirection == "up")
                {
                    moveInDirection = Vector3.back;
                }
                if (pointDirection == "Down" || pointDirection == "down")
                {
                    moveInDirection = Vector3.forward;
                }
                if (pointDirection == "Left" || pointDirection == "left")
                {
                    moveInDirection = Vector3.right;
                }
                if (pointDirection == "Right" || pointDirection == "right")
                {
                    moveInDirection = Vector3.left;
                }

                for (int i = 0; i < pointTiles; i++)
                {
                    var move = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, moveInDirection);
                    if (!GridBase.Instance.CellIsEmpty(move) && _playerMoveRef.Position.x != move.x && _playerMoveRef.Position.z != move.z)
                    {
                        break;
                    }
                    gameObject.transform.position = move + _positionOffset;
                    GridBase.Instance.UpdateEntry(this);

                    yield return new WaitForSeconds(_waitTime);
                }

                if (_currentPoint <= 0)
                {
                    _atStart = true;
                }
                else
                {
                    _currentPoint--;
                }
            }
        }
    }
}
