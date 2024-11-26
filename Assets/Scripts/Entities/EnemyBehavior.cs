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
    public bool BlocksHarmonyBeam { get => false; }
    public Vector3 moveInDirection { get; private set; }
    public Vector3 Position { get => transform.position; }

    [SerializeField]
    private Vector3 _positionOffset;

    public GameObject GetGameObject { get => gameObject; }

    private GameObject _player;
    private PlayerMovement _playerMove;
    [SerializeField] private GameObject _destinationMarker;
    [SerializeField] private GameObject _destPathVFX;

    [SerializeField] private bool _atStart;
    [SerializeField] private int _currentPoint = 0;
    private int _currentPointIndex = 0;

    //Destination object values
    [SerializeField] private bool _destAtStart;
    [SerializeField] private int _destCurrentPoint = 0;
    public bool CollidingWithRay = false;

    [SerializeField] private float _destYPos = 1f;
    [SerializeField] private float _lineYPosOffset = 1f;


    //Wait time between enemy moving each individual tile while on path to next destination
    [SerializeField] private float _waitTime = 0.5f;

    [SerializeField] private float _rotationTime = 0.10f;
    [SerializeField] private Ease _rotationEase = Ease.InOutSine;

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

    [SerializeField] private int _linePosCount;
    [SerializeField] private int _tilesToDraw = 0;
    [SerializeField] private int _currentLinePoint = 0;
    [SerializeField] private LineRenderer _vfxLine;

    public bool EnemyFrozen { get; private set; } = false;

    private int _enemyMovementTime = 1;

    // Event reference for the enemy movement sound
    [SerializeField] private EventReference _enemyMove = default;
    [SerializeField] public bool sonEnemy;

    private Rigidbody _rb;

    private void Awake()
    {
        PrimeTweenConfig.warnEndValueEqualsCurrent = false;
    }

    private const float MinMoveTime = 0.175f;

    // Start is called before the first frame update
    void Start()
    {
        moveInDirection = new Vector3(0, 0, 0);

        SnapToGridSpace();
        GridBase.Instance.AddEntry(this);

        _rb = GetComponent<Rigidbody>();
        _player = PlayerMovement.Instance.gameObject;
        _playerMove = _player.GetComponent<PlayerMovement>();

        _destinationMarker.transform.SetParent(null);

        // Make sure enemies are always seen at the start
        _atStart = true;

        if (TimeSignatureManager.Instance != null)
            TimeSignatureManager.Instance.RegisterTimeListener(this);

        _vfxLine = _destPathVFX.GetComponent<LineRenderer>();

        _destPathVFX.SetActive(false);
        _destinationMarker.SetActive(false);
        UpdateDestinationMarker();
        DestinationPath();
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
    /// DestinationPath is called whenever the mouse ray collides with the enemy.
    /// This function turns the _destPathVFX and _destinationMarker objects on/off.
    /// </summary>
    public void DestinationPath()
    {
        if (CollidingWithRay)
        {
            _destPathVFX.SetActive(true);
            _destinationMarker.SetActive(true);
        }
        else
        {
            _destPathVFX.SetActive(false);
            _destinationMarker.SetActive(false);
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
    /// Coroutine that handles the enemy's movement along the provided points in the struct object list.
    /// Also contains the destination marker movement behavior for the enemy.
    /// </summary>
    /// <returns></returns>
    private IEnumerator MoveEnemy()
    {
        /// <summary>
        /// Checks to see if all enemies have finished moving via a bool in the player script 
        /// and if the enemy is currently frozen by the harmony beam
        /// </summary>

        if (!EnemyFrozen && _playerMove.playerMoved)
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
                for (; _currentPointIndex < pointTiles; ++_currentPointIndex)
                {
                    var move = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position,
                        moveInDirection);
                    var entries = GridBase.Instance.GetCellEntries(move);
                    bool breakLoop = false;
                    float movementTime = Mathf.Clamp((_waitTime / pointTiles) / _enemyMovementTime, 
                        MinMoveTime, float.MaxValue);

                    //If the next cell contains an object that is not the player then the loop breaks
                    //enemy can't move into other enemies, walls, etc.
                    foreach (var entry in entries)
                    {
                        if (entry.GetGameObject.CompareTag("Wall") && entry.IsTransparent)
                        {
                            _rb.isKinematic = true;
                            breakLoop = false;
                            break;
                        }
                        if (entry.GetGameObject == _player)
                        {
                            _rb.isKinematic = false;
                            breakLoop = false;
                            break;
                        }
                        else
                        {
                            breakLoop = true;
                            break;
                        }
                    }

                    if (breakLoop == true || EnemyFrozen)
                    {
                        break;
                    }

                    Tween.Rotation(transform, endValue: Quaternion.LookRotation(moveInDirection), duration: _rotationTime,
                        ease: _rotationEase);

                    yield return Tween.Position(transform,
                        move + _positionOffset, duration: movementTime, 
                        ease: Ease.OutBack).OnUpdate<EnemyBehavior>(target: this, (target, tween) =>
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
                if (!EnemyFrozen && _atStart == true)
                {
                    _currentPointIndex = 0;

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
                else if (!EnemyFrozen)
                {
                    _currentPointIndex = 0;

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

        Tween.Rotation(transform, endValue: Quaternion.LookRotation(moveInDirection), duration: _rotationTime,
        ease: _rotationEase);

        GridBase.Instance.UpdateEntry(this);
        RoundManager.Instance.CompleteTurn(this);
    }

    /// <summary>
    /// This function updates the position of the _destinationMarker object using the
    /// _movePoints list.
    /// </summary>
    public void UpdateDestinationMarker()
    {
        //Sets the _destinationMarker object to the enemy's current position
        _destinationMarker.transform.position = transform.position;
        Vector3 linePos = transform.position;
        linePos.y = _lineYPosOffset;

        _vfxLine.SetPosition(_currentLinePoint, linePos);

        //Looks at the time signature for the enemy so it can place multiple moves in advance
        for (int i = 0; i < _enemyMovementTime; ++i)
        {
            //Updates the current point index before moving
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

            //Finds the direction and tiles to move based on its own current point index value
            var destPoint = _movePoints[_destCurrentPoint];
            var destPointDirection = destPoint.direction;
            var destPointTiles = destPoint.tilesToMove;
            FindDirection(destPointDirection);

            _tilesToDraw += destPointTiles;
            _linePosCount = _tilesToDraw + 1;
            _vfxLine.positionCount = _linePosCount;

            //Reverses if going backward through the list
            if (!_destAtStart)
            {
                moveInDirection = -moveInDirection;
            }

            //Moves the object instantly to the destination tile (instead of overtime)
            for (int k = 0; k < destPointTiles; k++)
            {
                var move = GridBase.Instance.GetCellPositionInDirection(_destinationMarker.transform.position,
                    moveInDirection);

                _destinationMarker.transform.position = move;

                if (k <= _vfxLine.positionCount + 1)
                {
                    linePos = move;
                    linePos.y = _lineYPosOffset;
                    
                    _vfxLine.SetPosition(_currentLinePoint + 1, linePos);
                    _currentLinePoint++;
                }
            }

            //Makes sure the marker is always at a y position of 1 so it is visible on the grid
            Vector3 destPos = _destinationMarker.transform.position;
            destPos.y += _destYPos;
            _destinationMarker.transform.position = destPos;
        }

        _tilesToDraw = 0;
        _currentLinePoint = 0;
    }

    public void UpdateTimingFromSignature(Vector2Int newTimeSignature)
    {
        _enemyMovementTime = newTimeSignature.y;

        if (_enemyMovementTime <= 0)
            _enemyMovementTime = 1;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (!DebugMenuManager.Instance.Invincibility && collision.gameObject.CompareTag("Player"))
        {
            Time.timeScale = 0f;

            SceneController.Instance.ReloadCurrentScene();
        }
    }

    public TurnState TurnState => TurnState.Enemy;
    /// <summary>
    /// Called by RoundManager to start this entity's turn
    /// </summary>
    /// <param name="direction">Direction of movement</param>
    public void BeginTurn(Vector3 direction)
    {
        StartCoroutine(MoveEnemy());
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
        if (sonEnemy)
        {
            EnemyFrozen = true;
        }
    }

    /// <summary>
    /// Unfreezes the enemy.
    /// </summary>
    public void OnLaserExit()
    {
        EnemyFrozen = false;
    }

    public bool HitWrapAround { get => sonEnemy; }

    /// <summary>
    /// Places this object in the center of its grid cell
    /// </summary>
    public void SnapToGridSpace()
    {
        Vector3Int cellPos = GridBase.Instance.WorldToCell(transform.position);
        Vector3 worldPos = GridBase.Instance.CellToWorld(cellPos);
        transform.position = new Vector3(worldPos.x, transform.position.y, worldPos.z);
    }
}
