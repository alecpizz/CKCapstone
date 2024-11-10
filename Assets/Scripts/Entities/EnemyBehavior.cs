/******************************************************************
*    Author: Cole Stranczek
*    Contributors: Cole Stranczek, Mitchell Young, Nick Grinstead, Alec Pizziferro
*    Date Created: 10/3/24
*    Description: Script that handles the behavior of the enemy,
*    from movement to causing a failstate with the player
*******************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using PrimeTween;
using Unity.VisualScripting;
using FMODUnity;

public class EnemyBehavior : MonoBehaviour, IGridEntry, ITimeListener, ITurnListener, IHarmonyBeamEntity
{
    public bool IsTransparent { get => false; }
    public Vector3 moveInDirection { get; private set; }
    public Vector3 Position { get => transform.position; }

    [SerializeField]
    private Vector3 _positionOffset;

    public GameObject GetGameObject { get => gameObject; }

    [SerializeField] private GameObject _player;
    [SerializeField] private GameObject _destinationMarker;

    [SerializeField] private bool _atStart;
    [SerializeField] private int _currentPoint = 0;

    [SerializeField] private bool _destAtStart;
    [SerializeField] private int _destCurrentPoint = 0;

    private PlayerMovement _playerMoveRef;

    //Wait time between enemy moving each individual tile while on path to next destination
    [SerializeField] private float _waitTime = 0.5f;

    //List of movePoint structs that contain a direction enum and a tiles to move integer.
    public enum Direction { Up, Down, Left, Right }

    [System.Serializable]
    private struct movePoints
    {
        public Direction direction;
        public int tilesToMove;
    }
    [SerializeField] private List<movePoints> _movePoints;

    //Check true in the inspector if the enemy is moving in a circular pattern (doesn't want to move back and forth)
    [SerializeField] private bool _circularMovement = false;

    public bool EnemyFrozen { get; private set; } = false;

    private int _enemyMovementTime = 1;

    [SerializeField] private float _tempMoveTime = 0.5f;

    // Event reference for the enemy movement sound
    [SerializeField] private EventReference _enemyMove = default;

    // Start is called before the first frame update
    void Start()
    {
        moveInDirection = new Vector3(0, 0, 0);

        GridBase.Instance.AddEntry(this);

        _playerMoveRef = _player.GetComponent<PlayerMovement>();

        // Make sure enemies are always seen at the start
        _atStart = true;

        if (TimeSignatureManager.Instance != null)
            TimeSignatureManager.Instance.RegisterTimeListener(this);

        UpdateDestinationMarker();
    }

    private void OnEnable()
    {
        if (RoundManager.Instance != null)
            RoundManager.Instance.RegisterListener(this);
    }


    /// <summary>
    /// Unregisters from player input
    /// </summary>
    private void OnDisable()
    {
        if (RoundManager.Instance != null)
            RoundManager.Instance.UnRegisterListener(this);
        if (TimeSignatureManager.Instance != null)
            TimeSignatureManager.Instance.UnregisterTimeListener(this);
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
    private IEnumerator DelayedInput()
    {
        /// <summary>
        /// Checks to see if all enemies have finished moving via a bool in the player script 
        /// and if the enemy is currently frozen by the harmony beam
        /// </summary>

        if (!EnemyFrozen)
        {
            for (int i = 0; i < _enemyMovementTime; ++i)
            {
                /// <summary>
                /// Looks at current point the the struct object list to pull the current 
                /// direction (enum) and amount of tiles to move in direction (int)
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
                /// For loop repeats enemy moving over a tile in the direction given until 
                /// either it sees another object in that direction
                /// that isn't the player (will move into players but not walls/enemies).
                /// </summary>
                for (int j = 0; j < pointTiles; j++)
                {
                    var move = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position,
                        moveInDirection);
                    var entries = GridBase.Instance.GetCellEntries(move);
                    bool breakLoop = false;

                    //If the next cell contains an object that is not the player then the loop breaks
                    //enemy can't move into other enemies, walls, etc.
                    foreach (var entry in entries)
                    {
                        if (entry.GetGameObject != _player)
                        {
                            breakLoop = true;
                            break;
                        }
                    }

                    if (breakLoop == true)
                    {
                        break;
                    }

                    yield return Tween.Position(transform,
                        move + _positionOffset, _tempMoveTime, ease: Ease.OutBack).OnUpdate<EnemyBehavior>(target: this, (target, tween) =>
                        {
                            GridBase.Instance.UpdateEntry(this);
                        }).ToYieldInstruction();

                    AudioManager.Instance.PlaySound(_enemyMove);
                    GridBase.Instance.UpdateEntry(this);
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
            UpdateDestinationMarker();
        }
        GridBase.Instance.UpdateEntry(this);
        RoundManager.Instance.CompleteTurn(this);
    }

    public void UpdateDestinationMarker()
    {
        _destinationMarker.transform.position = transform.position;

        for (int i = 0; i < _enemyMovementTime; ++i)
        {
            if (_destAtStart == true)
            {
                if (_destCurrentPoint >= _movePoints.Count - 1)
                {
                    if (!_circularMovement)
                    {
                        _destAtStart = false;
                    }
                    else
                    {
                        _destCurrentPoint = 0;
                    }
                }
                else
                {
                    _destCurrentPoint++;
                }
            }
            else
            {
                if (_destCurrentPoint <= 0)
                {
                    _destAtStart = true;
                }
                else
                {
                    _destCurrentPoint--;
                }
            }

            var destPoint = _movePoints[_destCurrentPoint];
            var destPointDirection = destPoint.direction;
            var destPointTiles = destPoint.tilesToMove;
            FindDirection(destPointDirection);

            if (!_destAtStart)
            {
                moveInDirection = -moveInDirection;
            }

            for (int k = 0; k < destPointTiles; k++)
            {
                var move = GridBase.Instance.GetCellPositionInDirection(_destinationMarker.transform.position,
                    moveInDirection);

                _destinationMarker.transform.position = move;
            }

            Vector3 destPos = _destinationMarker.transform.position;
            destPos.y += 1;
            _destinationMarker.transform.position = destPos;
        }
    }

    public void UpdateTimingFromSignature(Vector2Int newTimeSignature)
    {
        _enemyMovementTime = newTimeSignature.y;

        if (_enemyMovementTime <= 0)
            _enemyMovementTime = 1;
    }

    public TurnState TurnState => TurnState.Enemy;
    public void BeginTurn(Vector3 direction)
    {
        StartCoroutine(DelayedInput());
    }

    /// <summary>
    /// Can force enemy turn to end early
    /// </summary>
    public void ForceTurnEnd()
    {
        StopAllCoroutines();
        GridBase.Instance.UpdateEntry(this);
        RoundManager.Instance.CompleteTurn(this);
    }
    
    public bool AllowLaserPassThrough { get => true; }
    /// <summary>
    /// Freezes the enemy.
    /// </summary>
    public void OnLaserHit()
    {
        EnemyFrozen = true;
    }

    /// <summary>
    /// Unfreezes the enemy.
    /// </summary>
    public void OnLaserExit()
    {
        EnemyFrozen = false;
    }

    public bool HitWrapAround { get => true; }
}
