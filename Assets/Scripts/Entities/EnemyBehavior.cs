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

    //List of movePoint structs that contain a direction enum and a tiles to move integer.
    public enum Direction { Up, Down, Left, Right }

    [System.Serializable]
    public struct movePoints
    {
        public Direction direction;
        public int tilesToMove;
    }
    [SerializeField] private List<movePoints> _movePoints;

    //Check true in the inspector if the enemy is moving in a circular pattern (doesn't want to move back and forth)
    [SerializeField] private bool _circularMovement = false;

    public bool enemyFrozen = false;

    // Start is called before the first frame update
    void Start()
    {
        moveInDirection = new Vector3(0, 0, 0);

        GridBase.Instance.AddEntry(this);
        _input = new PlayerControls();
        _input.InGame.Enable();
        _input.InGame.Movement.performed += EnemyMove;

        _playerMoveRef = _player.GetComponent<PlayerMovement>();

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
    /// Function that tells the enemy which direction to move in
    /// </summary>
    /// <param name="myDirection"></param>
    public void FindDirection(Direction myDirection)
    {
            switch (myDirection)
            {
                case Direction.Up:
                    moveInDirection = Vector3.forward;
                    break;
                case Direction.Down:
                    moveInDirection = Vector3.back;
                    break;
                case Direction.Left:
                    moveInDirection = Vector3.left;
                    break;
                case Direction.Right:
                    moveInDirection = Vector3.right;
                    break;
                default:
                    break;
            }
    }

    /// <summary>
    /// Coroutine that handles the enemy's movement along the provided points in the struct object list
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
        /// Checks to see if all enemies have finished moving via a bool in the player script and if the enemy is currently frozen by the harmony beam
        /// </summary>
        yield return new WaitForSeconds(0.1f);
        if (_playerMoveRef.playerMoved == true && enemyFrozen == false)
        {
            _playerMoveRef.enemiesMoved = false;

            /// <summary>
            /// Looks at current point the the struct object list to pull the current direction (enum) and amount of tiles to move in direction (int)
            /// </summary>
            var point = _movePoints[_currentPoint];
            var pointDirection = point.direction;
            var pointTiles = point.tilesToMove;
            FindDirection(pointDirection);

            //Reverses move direction if going back through the list
            if (!_atStart)
            {
                moveInDirection = -moveInDirection;
            }

            /// <summary>
            /// For loop repeats enemy moving over a tile in the direction given until either it sees another object in that direction
            /// that isn't the player (will move into players but not walls/enemies).
            /// </summary>
            for (int i = 0; i < pointTiles; i++)
            {
                var move = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, moveInDirection);

                //Checks if the next cell contains an object
                if (!GridBase.Instance.CellIsEmpty(move))
                {
                    //Uses player's current x and z position and enemy's y position to check whether or not the player is in the next cell.
                    var checkPlayerPos = new Vector3(0, 0, 0);
                    checkPlayerPos.x = _playerMoveRef.Position.x;
                    checkPlayerPos.z = _playerMoveRef.Position.z;
                    checkPlayerPos.y = move.y;

                    //If the next cell contains an object that is not in both the x and z position of the player then the loop breaks
                    //enemy can't move into other enemies, walls, etc.
                    if (move != checkPlayerPos)
                    {
                        break;
                    }
                }
                gameObject.transform.position = move + _positionOffset;
                GridBase.Instance.UpdateEntry(this);

                yield return new WaitForSeconds(_waitTime);
            }

            /// <summary>
            /// If the current point is equal to the length of the list then the if/else statement 
            /// will check the atStart bool and concurrently reverse through the list
            /// </summary>
            if (_atStart == true)
            {
                if (_currentPoint >= _movePoints.Count - 1)
                {
                    if (!_circularMovement)
                    {
                        _atStart = false;
                    }
                    else
                    {
                        _currentPoint = 0;
                    }
                }
                else
                {
                    _currentPoint++;
                }
            }
            else
            {
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
