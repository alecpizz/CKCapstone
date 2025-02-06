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
using UnityEngine.Serialization;
using UnityEngine.InputSystem;
using PrimeTween;
using Unity.VisualScripting;
using FMODUnity;
using SaintsField.Playa;

public class EnemyBehavior : MonoBehaviour, IGridEntry, ITimeListener,
    ITurnListener, IHarmonyBeamEntity
{
    public bool IsTransparent
    {
        get => false;
    }

    public bool BlocksHarmonyBeam
    {
        get => false;
    }

    public Vector3 moveInDirection { get; private set; }

    public Vector3 Position
    {
        get => transform.position;
    }

    [SerializeField] private Vector3 _positionOffset;

    public GameObject EntryObject
    {
        get => gameObject;
    }

    private PlayerControls _input;
    [SerializeField] private GameObject _destinationMarker;
    [SerializeField] private GameObject _destPathVFX;
    private bool _atFirstPoint;

    [SerializeField] private int _currentPoint = 0;
    private int _currentPointIndex = 0;

    //Destination object values
    private bool _destAtFirstPoint = true;

    [SerializeField] private int _destCurrentPoint = 0;
    public bool CollidingWithRay = false;

    [SerializeField] private float _destYPos = 1f;
    [SerializeField] private float _lineYPosOffset = 1f;

    //Wait time between enemy moving each individual tile while on path to next destination
    [PlayaInfoBox("Time for the enemy to move between each tile. " +
                  "\n This will be divided by the number of spaces it will move.")]
    [SerializeField]
    private float _waitTime = 0.5f;

    [PlayaInfoBox("The floor for how fast the enemy can move.")] [SerializeField]
    private float _minMoveTime = 0.175f;

    [SerializeField] private bool _currentToggle = true;

    [SerializeField] private float _rotationTime = 0.10f;
    [SerializeField] private Ease _rotationEase = Ease.InOutSine;
    [SerializeField] private Ease _movementEase = Ease.OutBack;

    //List of movePoint structs that contain a direction enum and a tiles to move integer.
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    [System.Serializable]
    private struct movePoints
    {
        public Direction direction;
        public int tilesToMove;

        public Vector3 GetDirection()
        {
            return direction switch
            {
                Direction.Up => Vector3.forward,
                Direction.Down => Vector3.back,
                Direction.Left => Vector3.left,
                Direction.Right => Vector3.right,
                _ => Vector3.zero
            };
        }
    }

    [SerializeField] private List<movePoints> _movePoints;

    //Check true in the inspector if the enemy is moving in
    //a circular pattern (doesn't want to move back and forth)
    [SerializeField] private bool _circularMovement = false;

    [SerializeField] private int _linePosCount;
    [SerializeField] private int _tilesToDraw = 0;
    [SerializeField] private int _currentLinePoint = 0;
    [SerializeField] private LineRenderer _vfxLine;

    private bool _isFrozen = false;

    // Event reference for the enemy movement sound
    [SerializeField] private EventReference _enemyMove = default;
    [SerializeField] public bool sonEnemy;

    private List<Vector3Int> _moveDestinations = new();

    // Timing from metronome
    private int _enemyMovementTime = 1;

    private Rigidbody _rb;

    private void Awake()
    {
        PrimeTweenConfig.warnEndValueEqualsCurrent = false;
    }

    /// <summary>
    /// Start is called before the first frame update.
    /// </summary>
    private void Start()
    {
        moveInDirection = new Vector3(0, 0, 0);
        SnapToGridSpace();
        BuildCellList();
        GridBase.Instance.AddEntry(this);

        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;

        _destinationMarker.transform.SetParent(null);

        // Make sure enemies are always seen at the start
        _atFirstPoint = true;

        if (TimeSignatureManager.Instance != null)
        {
            TimeSignatureManager.Instance.RegisterTimeListener(this);
        }

        _vfxLine = _destPathVFX.GetComponent<LineRenderer>();
        _vfxLine.positionCount = 2;

        _destPathVFX.SetActive(false);
        _destinationMarker.SetActive(false);
        for (int i = 0; i < _enemyMovementTime; i++)
        {
            UpdateDestinationMarker();
        }

        DestinationPath();

        _input = new PlayerControls();
        _input.InGame.Enable();

        _input.InGame.Toggle.performed += PathingToggle;
    }

    /// <summary>
    /// Registers the instance in the RoundManager.
    /// </summary>
    private void OnEnable()
    {
        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.RegisterListener(this);
        }
    }

    private void OnDrawGizmos()
    {
        if (_moveDestinations.Count == 0)
        {
            var grid = GridBase.Instance;
            if (grid != null)
            {
                List<Vector3Int> cells = new();
                Vector3 prevCell = transform.position;
                cells.Add(grid.WorldToCell(transform.position));
                foreach (var pt in _movePoints)
                {
                    for (int i = 0; i < pt.tilesToMove; i++)
                    {
                        var newPos = grid.GetCellPositionInDirection(prevCell,
                            pt.GetDirection());
                        prevCell = newPos;
                    }

                    cells.Add(grid.WorldToCell(prevCell));
                }

                for (int i = 0; i < cells.Count; i++)
                {
                    var color = Color.HSVToRGB((float) i / cells.Count, 1f, 1f);
                    Gizmos.color = color;
                    Gizmos.DrawSphere(grid.CellToWorld(cells[i]), 0.2f);
                }
            }
        }
        else
        {
            var grid = GridBase.Instance;
            if (grid != null)
            {
                for (var i = 0; i < _moveDestinations.Count; i++)
                {
                    var cell = _moveDestinations[i];
                    var color = Color.HSVToRGB((float) i / _moveDestinations.Count, 1f, 1f);
                    Gizmos.color = color;
                    Gizmos.DrawSphere(grid.CellToWorld(cell), 0.2f);
                }
            }
        }
    }

    /// <summary>
    /// Unregisters from player input
    /// </summary>
    private void OnDisable()
    {
        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.UnRegisterListener(this);
        }

        if (TimeSignatureManager.Instance != null)
        {
            TimeSignatureManager.Instance.UnregisterTimeListener(this);
        }

        _input.InGame.Toggle.performed -= PathingToggle;
        _input.InGame.Disable();
    }

    private void BuildCellList()
    {
        var grid = GridBase.Instance;
        Vector3 prevCell = transform.position;
        _moveDestinations.Add(grid.WorldToCell(transform.position));
        foreach (var pt in _movePoints)
        {
            for (int i = 0; i < pt.tilesToMove; i++)
            {
                var newPos = grid.GetCellPositionInDirection(prevCell,
                    pt.GetDirection());
                prevCell = newPos;
            }

            _moveDestinations.Add(grid.WorldToCell(prevCell));
        }
    }

    /// <summary>
    /// DestinationPath is called whenever the mouse ray collides with the enemy.
    /// This function turns the _destPathVFX and _destinationMarker objects on/off.
    /// </summary>
    public void DestinationPath()
    {
        if (!_currentToggle)
        {
            return;
        }

        _destPathVFX.SetActive(CollidingWithRay);
        _destinationMarker.SetActive(CollidingWithRay);
    }

    /// <summary>
    /// Toggles all enemy pathing on the current level when the player
    /// presses spacebar.
    /// </summary>
    /// <param name="context"></param>
    private void PathingToggle(InputAction.CallbackContext context)
    {
        _destPathVFX.SetActive(_currentToggle);
        _destinationMarker.SetActive(_currentToggle);

        _currentToggle = !_currentToggle;
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
        // Checks to see if all enemies have finished moving via a bool in the player script 
        // and if the enemy is currently frozen by the harmony beam
        if (!_isFrozen)
        {
            for (int i = 0; i < _enemyMovementTime; ++i)
            {
                //Looks at current point the the struct object list to pull the current direction
                //(enum) and amount of tiles to move in direction (int)
                var point = _movePoints[_currentPoint];
                var pointDirection = point.direction;
                var pointTiles = point.tilesToMove;
                FindDirection(pointDirection);

                //Reverses move direction if going back through the list
                if (!_atFirstPoint)
                {
                    moveInDirection = -moveInDirection;
                }

                // For loop repeats enemy moving over a tile in the direction given until either it sees another
                // object in that direction that isn't the player (will move into players but not walls/enemies).
                for (; _currentPointIndex < pointTiles; ++_currentPointIndex)
                {
                    var move = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position,
                        moveInDirection);
                    var entries = GridBase.Instance.GetCellEntries(move);
                    bool breakLoop = false;
                    float movementTime = Mathf.Clamp((_waitTime / pointTiles) / _enemyMovementTime,
                        _minMoveTime, float.MaxValue);

                    //If the next cell contains an object that is not the player then the loop breaks
                    //enemy can't move into other enemies, walls, etc.
                    foreach (var entry in entries)
                    {
                        if (entry.EntryObject.CompareTag("Wall") && entry.IsTransparent)
                        {
                            _rb.isKinematic = true;
                            breakLoop = false;
                            break;
                        }

                        if (entry.EntryObject == PlayerMovement.Instance.gameObject)
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

                    if (breakLoop == true || _isFrozen)
                    {
                        break;
                    }

                    Tween.Rotation(transform, endValue: Quaternion.LookRotation(moveInDirection),
                        duration: _rotationTime,
                        ease: _rotationEase);

                    yield return Tween.Position(transform,
                            move + _positionOffset, duration: movementTime,
                            ease: _movementEase)
                        .OnUpdate<EnemyBehavior>(target: this,
                            (target, tween) => { GridBase.Instance.UpdateEntry(this); })
                        .ToYieldInstruction();

                    AudioManager.Instance.PlaySound(_enemyMove);
                    GridBase.Instance.UpdateEntry(this);
                }

                // If the current point is equal to the length of the list then the if/else statement 
                // will check the atFirstPoint bool and concurrently reverse through the list
                if (!_isFrozen && _atFirstPoint == true)
                {
                    _currentPointIndex = 0;

                    if (_currentPoint >= _movePoints.Count - 1)
                    {
                        if (!_circularMovement)
                        {
                            _atFirstPoint = false;
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
                else if (!_isFrozen)
                {
                    _currentPointIndex = 0;

                    if (_currentPoint <= 0)
                    {
                        _atFirstPoint = true;
                    }
                    else
                    {
                        _currentPoint--;
                    }
                }
            }

            UpdateDestinationMarker();
        }

        Tween.Rotation(transform, endValue: Quaternion.LookRotation(moveInDirection),
            duration: _rotationTime, ease: _rotationEase);

        GridBase.Instance.UpdateEntry(this);
        RoundManager.Instance.CompleteTurn(this);
    }

    /// <summary>
    /// This function updates the position of the _destinationMarker object using the
    /// _movePoints list.
    /// </summary>
    private void UpdateDestinationMarker()
    {
        //Sets the _destinationMarker object to the enemy's current position
        _destinationMarker.transform.position = transform.position;
        Vector3 linePos = transform.position;
        linePos.y = _lineYPosOffset;
        _vfxLine.SetPosition(0, linePos);
        //Looks at the time signature for the enemy so it can place multiple moves in advance

        EvaluateNextMove(ref _indicatorIndex, ref _indicatorReturningToStart);

        //Finds the direction and tiles to move based on its own current point index value
        var destPoint = _moveDestinations[_indicatorIndex];
        var destPointWorld = GridBase.Instance.CellToWorld(destPoint);

        linePos = destPointWorld;
        linePos.y = _lineYPosOffset;

        _vfxLine.SetPosition(1, destPointWorld);
        destPointWorld.y += _destYPos;
        _destinationMarker.transform.position = destPointWorld;

        _tilesToDraw = 0;
        _currentLinePoint = 0;
    }

    public void UpdateTimingFromSignature(Vector2Int newTimeSignature)
    {
        _enemyMovementTime = newTimeSignature.y;

        if (_enemyMovementTime <= 0)
        {
            _enemyMovementTime = 1;
        }
    }

    public TurnState TurnState => TurnState.Enemy;

    //TEMP VARS for rewrite
    private int _moveIndex = 0;
    private bool _isReturningToStart = false;
    private int _indicatorIndex = 0;
    private bool _indicatorReturningToStart = false;

    /// <summary>
    /// Called by RoundManager to start this entity's turn
    /// </summary>
    /// <param name="direction">Direction of movement</param>
    public void BeginTurn(Vector3 direction)
    {
        // StartCoroutine(MoveEnemy());
        //figure out where we should move next.
        if (_isFrozen)
        {
            RoundManager.Instance.CompleteTurn(this);
            return;
        }

        StartCoroutine(MovementRoutine());
    }

    private IEnumerator MovementRoutine()
    {
        bool blocked = false;
        for (int i = 0; i < _enemyMovementTime; i++)
        {
            int prevMove = _moveIndex;
            bool prevReturn = _isReturningToStart;
            EvaluateNextMove(ref _moveIndex, ref _isReturningToStart);
            var movePt = _moveDestinations[_moveIndex];
            var currCell = GridBase.Instance.WorldToCell(transform.position);
            var dist = Vector3Int.Distance(currCell, movePt);
            //cell is only one apart.
            var goalCell = GetLongestPath(GridBase.Instance.CellToWorld(movePt));
            Debug.Log($"Move Point {movePt}, goal Point {goalCell}");
            if (goalCell != movePt)
            {
                Debug.Log($"Blocked! Altering memory!");
                blocked = true;
                //we can still make the move, we just need to try again because we were blocked.
                _moveIndex = prevMove;
                _isReturningToStart = prevReturn;
            }

            //already at this spot, next turn.
            if (goalCell == currCell) continue;
            var rotationDir = (GridBase.Instance.CellToWorld(movePt) - transform.position).normalized;
            var moveWorld = GridBase.Instance.CellToWorld(movePt);
            dist = Mathf.Max(dist, 1f);
            float movementTime = Mathf.Clamp((_waitTime / dist) / _enemyMovementTime,
                _minMoveTime, float.MaxValue);
            var tween = Tween
                .Position(transform, endValue: moveWorld + _positionOffset,
                    duration: movementTime, _movementEase).OnUpdate(
                    target: this,
                    (_, _) =>
                    {
                        GridBase.Instance.UpdateEntry(this);
                        //not a fan of this but it should be more consistent than 
                        //using collisions
                        //also just math comparisons, no memory accessing outside of Position.
                        if (GridBase.Instance.WorldToCell(PlayerMovement.Instance.Position) ==
                            GridBase.Instance.WorldToCell(transform.position) &&
                            !DebugMenuManager.Instance.Invincibility)
                        {
                            //hit a player!
                            SceneController.Instance.ReloadCurrentScene();
                        }
                    }).ToYieldInstruction();
            var rotTween = Tween.Rotation(transform, endValue: Quaternion.LookRotation(rotationDir),
                duration: _rotationTime,
                ease: _rotationEase).ToYieldInstruction();
            yield return rotTween;
            yield return tween;
            AudioManager.Instance.PlaySound(_enemyMove);
            GridBase.Instance.UpdateEntry(this);
        }

        if (!blocked)
        {
            UpdateDestinationMarker();
        }
        RoundManager.Instance.CompleteTurn(this);
    }

    private Vector3Int GetLongestPath(Vector3 goal)
    {
        var direction = (goal - transform.position);
        direction.Normalize();
        bool stop = false;
        var originTilePos = GridBase.Instance.WorldToCell(transform.position);
        var currTilePos = GridBase.Instance.CellToWorld(originTilePos);
        do
        {
            var nextCell = GridBase.Instance.GetCellPositionInDirection(currTilePos, direction);
            stop = !CanPassThroughCell(GridBase.Instance.WorldToCell(nextCell));
            if (stop) continue;
            if (currTilePos == nextCell || nextCell == goal)
            {
                currTilePos = nextCell;
                break;
            }

            currTilePos = nextCell;
        } while (!stop);

        return GridBase.Instance.WorldToCell(currTilePos);
    }

    private bool CanPassThroughCell(Vector3Int cell)
    {
        var entries = GridBase.Instance.GetCellEntries(cell);
        foreach (var entry in entries)
        {
            //can pass thru walls
            if (entry.IsTransparent)
            {
                continue;
            }

            //can pass thru the player
            if (entry.EntryObject == PlayerMovement.Instance.gameObject)
            {
                continue;
            }

            return false;
        }

        return true;
    }

    private void EvaluateNextMove(ref int moveIndex, ref bool looped)
    {
        if (moveIndex < _moveDestinations.Count - 1)
        {
            if (!looped)
            {
                //move forward as normal
                moveIndex++;
            }
            else
            {
                moveIndex--;
                //we've returned to the start, so reset everything to be back as normal
                if (moveIndex <= 0)
                {
                    moveIndex = 0;
                    looped = false;
                }
            }
        }
        else
        {
            //we're at the end of our potential moves, so let's determine how we're gonna get back.

            if (!_circularMovement)
            {
                //we're not using circular movement, so for future turns we need to move backwards until we reach 
                // the start again. 
                looped = true;
                moveIndex--;
            }
            else
            {
                //our moves will start with 0 again.
                moveIndex = 0;
            }
        }
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

    public bool AllowLaserPassThrough
    {
        get => true;
    }

    /// <summary>
    /// Freezes the enemy.
    /// </summary>
    public void OnLaserHit()
    {
        if (sonEnemy)
        {
            _isFrozen = true;
        }
    }

    /// <summary>
    /// Unfreezes the enemy.
    /// </summary>
    public void OnLaserExit()
    {
        _isFrozen = false;
    }

    public bool HitWrapAround
    {
        get => sonEnemy;
    }

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