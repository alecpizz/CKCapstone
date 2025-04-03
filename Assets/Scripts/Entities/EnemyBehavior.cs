/******************************************************************
 *    Author: Cole Stranczek
 *    Contributors: Cole Stranczek, Mitchell Young, Nick Grinstead, Alec Pizziferro, Alex Laubenstein
 *    Jamison Parks
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
using SaintsField;
using SaintsField.Playa;

public class EnemyBehavior : MonoBehaviour, IGridEntry, ITimeListener,
    ITurnListener, IHarmonyBeamEntity
{
    public bool IsTransparent
    {
        get => !_isFrozen;
    }

    public bool BlocksHarmonyBeam
    {
        get => false;
    }

    public Vector3 Position
    {
        get => transform.position;
    }

    public Transform EntityTransform 
    { 
        get => transform; 
    }

    [SerializeField] private Vector3 _positionOffset;

    public GameObject EntryObject
    {
        get => gameObject;
    }

    [SerializeField] private GameObject _destinationMarker;
    [SerializeField] private GameObject _destPathVFX;

    //Destination object values

    public bool CollidingWithRay { get; set; }= false;

    [SerializeField] private float _destYPos = 1f;
    [SerializeField] private float _lineYPosOffset = 1f;

    [PlayaInfoBox("Time delay from when an enemy starts their turn and actually begins moving." +
        "\n This is meant to prevent enemies from moving before the player starts to move.")]
    [PropRange(0f, 0.5f)]
    [SerializeField]
    private float _timeBeforeTurn = 0.1f;

    //Wait time between enemy moving each individual tile while on path to next destination
    [PlayaInfoBox("Time for the enemy to move between each tile. " +
                  "\n This will be divided by the number of spaces it will move.")]
    [SerializeField]
    private float _waitTime = 0.5f;

    [PlayaInfoBox("The floor for how fast the enemy can move.")] [SerializeField]
    private float _minMoveTime = 0.175f;

    private bool _currentGroupToggle = true;
    private bool _currentSoloToggle = true;

    [SerializeField] private float _rotationTime = 0.10f;
    [SerializeField] private Ease _rotationEase = Ease.InOutSine;
    [SerializeField] private Ease _movementEase = Ease.OutBack;
    private bool _endRotate = false;

    private int _offsetDestCount = 0;
    private bool _signatureIsChanged = false;
    private bool _firstTurnBack = false;
    private bool _metronomeTriggered = false;
    private bool _notFirstCheck = false;

    /// <summary>
    /// Helper enum for enemy directions.
    /// </summary>
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    /// <summary>
    /// Struct to hold an enemy's move. Contains a direction and magnitude.
    /// </summary>
    [System.Serializable]
    private struct movePoints
    {
        public Direction direction;
        public int tilesToMove;

        /// <summary>
        /// Grabs the vector3 equivalent of the direction assigned to this struct.
        /// </summary>
        /// <returns>A vector3 equivalent of direction.</returns>
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
    [InfoBox("If the enemy uses circular movement, after reaching the end of its moves, " +
             "it will go back to its first move, rather than moving backwards through its moves.", EMessageType.Info)]
    [SerializeField] private bool _circularMovement = false;

    private LineRenderer _vfxLine;
    [SerializeField] private float _enemyRotateToMovementDelay = 0.2f;

    private bool _isFrozen = false;

    // Event reference for the enemy movement sound
    [SerializeField] private EventReference _enemyMove = default;

    [FormerlySerializedAs("sonEnemy")] [SerializeField]
    private bool _isSonEnemy;

    private readonly List<Vector3Int> _moveDestinations = new();

    // Timing from metronome
    private int _enemyMovementTime = 1;

    private Rigidbody _rb;

    private int _moveIndex = 0;
    private bool _isReturningToStart = false;
    private int _indicatorIndex = 0;
    private bool _indicatorReturningToStart = false;
    private int _currentEnemyIndex = 0;

    //public static PlayerMovement Instance;
    private static readonly int Forward = Animator.StringToHash("Forward");
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int Frozen = Animator.StringToHash("Frozen");
    private static readonly int Turn = Animator.StringToHash("Turn");


    [SerializeField] private Animator _animator;

    /// <summary>
    /// Disables a PrimeTween warning.
    /// </summary>
    private void Awake()
    {
        PrimeTweenConfig.warnEndValueEqualsCurrent = false;
        PrimeTweenConfig.warnZeroDuration = false;
    }

    /// <summary>
    /// Start is called before the first frame update.
    /// </summary>
    private void Start()
    {
        SnapToGridSpace();
        BuildCellList();
        GridBase.Instance.AddEntry(this);

        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;

        _destinationMarker.transform.SetParent(null);

        // Make sure enemies are always seen at the start

        if (TimeSignatureManager.Instance != null)
        {
            TimeSignatureManager.Instance.RegisterTimeListener(this);
        }

        _vfxLine = _destPathVFX.GetComponent<LineRenderer>();
        _vfxLine.positionCount = 2;

        _destPathVFX.SetActive(false);
        _destinationMarker.SetActive(false);

        UpdateDestinationMarker();
        DestinationPath();
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

    /// <summary>
    /// Visualizer for enemy pathing in editor.
    /// </summary>
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
    /// Unregisters from from managers
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
    }

    /// <summary>
    /// Takes the assigned moves to the enemy and builds a list
    /// of static points, relative to the enemy's initial position. 
    /// </summary>
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
    /// This function turns the __destPathVFX and __destinationMarker objects on/off.
    /// </summary>
    public void DestinationPath()
    {
        if (!_currentGroupToggle)
        {
            return;
        }

        _destPathVFX.SetActive(CollidingWithRay);
        _destinationMarker.SetActive(CollidingWithRay);
    }

    /// <summary>
    /// Toggles all enemy pathing on the current level when the player
    /// presses Q.
    /// </summary>
    /// <param name="context"></param>
    public void PathingToggle(bool isCycling)
    {
        if (isCycling)
        {
            _currentSoloToggle = true;

            _destPathVFX.SetActive(false);
            _destinationMarker.SetActive(false);

            _currentGroupToggle = true;
        }
        else
        {
            _destPathVFX.SetActive(_currentGroupToggle);
            _destinationMarker.SetActive(_currentGroupToggle);

            _currentGroupToggle = !_currentGroupToggle;
        }
    }

    /// <summary>
    /// Toggles enemy pathing individually for enemiesin  the current level when the player
    /// presses a bumper .
    /// </summary>
    /// <param name="context"></param>
    public void PathingCycle()
    {
        _destPathVFX.SetActive(_currentSoloToggle);
        _destinationMarker.SetActive(_currentSoloToggle);

        _currentSoloToggle = !_currentSoloToggle;
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
        //Looks at the time signature for the enemy so it can place multiple moves in advance

        NextMarkerDestination(ref _indicatorIndex, ref _indicatorReturningToStart);
        _vfxLine.positionCount = _moveDestinations.Count;

        //Finds the direction and tiles to move based on its own current point index value
        var destPoint = _moveDestinations[_indicatorIndex];
        var destPointWorld = GridBase.Instance.CellToWorld(destPoint);

        //Sets each point in the _vfx line along the tile path in the _moveDestinations list
        //starting from the enemy's current position
        for (int i = 0; i < _moveDestinations.Count; i++)
        {
            Vector3 _vfxPos = GridBase.Instance.CellToWorld(_moveDestinations[i]);
            _vfxPos.y = _lineYPosOffset;

            _vfxLine.SetPosition(i, _vfxPos);
        }

        destPointWorld.y += _destYPos;
        _destinationMarker.transform.position = destPointWorld;
    }

    /// <summary>
    /// Implementation of ITimeListeners time sig method.
    /// Stores the enemy movement time.
    /// </summary>
    /// <param name="newTimeSignature">The new time signature.</param>
    public void UpdateTimingFromSignature(Vector2Int newTimeSignature)
    {
        if (_enemyMovementTime != newTimeSignature.y && _notFirstCheck)
        {
            _metronomeTriggered = true;
        }
        
        if (!_notFirstCheck)
        {
            _notFirstCheck = true;
        }

        _enemyMovementTime = newTimeSignature.y;

        if (_enemyMovementTime <= 0)
        {
            _enemyMovementTime = 1;
        }
    }

    public TurnState TurnState => TurnState.Enemy;
    public TurnState SecondaryTurnState => TurnState.None;

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

    /// <summary>
    /// Coroutine for handling enemy movement. Handles determining pathing,
    /// Tween movement, and updating the indicator.
    /// </summary>
    /// <returns>null</returns>
    private IEnumerator MovementRoutine()
    {
        yield return new WaitForSeconds(_timeBeforeTurn);

        bool blocked = false;
        for (int i = 0; i < _enemyMovementTime; i++)
        {
            int prevMove = _moveIndex;
            bool prevReturn = _isReturningToStart;
            EvaluateNextMove(ref _moveIndex, ref _isReturningToStart);
            var movePt = _moveDestinations[_moveIndex];
            var currCell = GridBase.Instance.WorldToCell(transform.position);
            var goalCell = GetLongestPath(GridBase.Instance.CellToWorld(movePt));

            if (_moveIndex == _moveDestinations.Count - 1 && !_circularMovement)
            {
                _endRotate = true;
            }
            //we were blocked by something, adjust memory
            if (goalCell != movePt)
            {
                blocked = true;
                //we can still make the move in the future, we just need to try again because we were blocked.
                _moveIndex = prevMove;
                _isReturningToStart = prevReturn;
            }
            //already at this spot, next turn.
            if (goalCell == currCell && !blocked)
            {
                continue;
            }

            if (_isFrozen)
            {
                blocked = true;
                continue;
            }

            if (_animator != null)
            {
                _animator.SetBool(Frozen, false);
                _animator.SetBool(Forward, true);
            }
            var dist = Vector3Int.Distance(currCell, goalCell);
            var rotationDir = (GridBase.Instance.CellToWorld(goalCell) - transform.position).normalized;
            var moveWorld = GridBase.Instance.CellToWorld(goalCell);

            dist = Mathf.Max(dist, 1f);
            float movementTime = Mathf.Clamp((_waitTime / _enemyMovementTime) * dist,
                _minMoveTime, float.MaxValue);
            var tween = Tween
                .Position(transform, endValue: moveWorld + _positionOffset,
                    duration: movementTime, _movementEase).OnUpdate(
                    target: this,
                    (_, _) =>
                    {
                        HarmonyBeam.TriggerHarmonyScan?.Invoke();
                        GridBase.Instance.UpdateEntry(this);
                        //not a fan of this but it should be more consistent than 
                        //using collisions
                        //also just math comparisons, no memory accessing outside of Position.
                        if (GridBase.Instance.WorldToCell(PlayerMovement.Instance.Position) ==
                            GridBase.Instance.WorldToCell(transform.position) &&
                            !DebugMenuManager.Instance.Invincibility)
                        {
                            //hit a player!
                            PlayerMovement.Instance.OnDeath();
                            if (_animator != null)
                            {
                                _animator.SetBool(Attack, true);
                            }
                            SceneController.Instance.ReloadCurrentScene();
                        }
                    });
            AudioManager.Instance.PlaySound(_enemyMove);
            if (rotationDir != transform.forward && _animator != null)
            {
                //_animator.SetBool(Turn, true);
            }
            yield return Tween.Rotation(transform, endValue: Quaternion.LookRotation(rotationDir),
                duration: _rotationTime,
                ease: _rotationEase).Chain(Tween.Delay(_enemyRotateToMovementDelay)).Chain(tween).ToYieldInstruction();
            if (_animator != null)
            {
                //_animator.SetBool(Turn, false);
            }
            if (_animator != null)
            {
                _animator.SetBool(Forward, false);
            }
            GridBase.Instance.UpdateEntry(this);

            if (_endRotate)
            {
                if (_animator != null)
                {
                    //_animator.SetBool(Turn, false);
                }
                yield return Tween.Rotation(transform, endValue: Quaternion.LookRotation(-rotationDir),
                duration: _rotationTime,
                ease: _rotationEase).Chain(Tween.Delay(_enemyRotateToMovementDelay)).ToYieldInstruction();
                _endRotate = false;
            }
            if (_animator != null)
            {
                //_animator.SetBool(Turn, false);
            }
        }
        if (!blocked)
        {
            UpdateDestinationMarker();
        }
        
        RoundManager.Instance.CompleteTurn(this);

    }

    /// <summary>
    /// Determines the longest path from the enemy's current position
    /// to its goal. WIll stop if it's blocked by something.
    /// </summary>
    /// <param name="goal">The goal position, in world space to get to.</param>
    /// <returns>The updated goal cell.</returns>
    private Vector3Int GetLongestPath(Vector3 goal)
    {
        var direction = (goal - transform.position);
        direction.Normalize();
        bool stop = false;
        var originTilePos = GridBase.Instance.WorldToCell(transform.position);
        var currTilePos = GridBase.Instance.CellToWorld(originTilePos);
        //loop thru all tiles in the direction of the goal, stopping if blocked or if we're at the goal.
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

    /// <summary>
    /// Determines if the enemy can pass through the cell.
    /// </summary>
    /// <param name="cell">The cell to check.</param>
    /// <returns>True if the enemy can pass through.</returns>
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

    /// <summary>
    /// Determines the next move index for the enemy.
    /// </summary>
    /// <param name="moveIndex">Reference to the evaluated move index.</param>
    /// <param name="looped">Reference to the evaluated loop state.</param>
    private void EvaluateNextMove(ref int moveIndex, ref bool looped)
    {
        //not at the end of our list of moves.
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
                    _endRotate = true;
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
                //our moves will start with 1 since we're circularly repeating our movement.
                moveIndex = 1;
            }
        }

        _currentEnemyIndex = moveIndex;
    }


    /// <summary>
    /// Determines the next move index for the enemy's pathing indicator
    /// using the current time signature.
    /// </summary>
    /// <param name="moveIndex">Reference to the evaluated move index.</param>
    /// <param name="looped">Reference to the evaluated loop state.</param>
    private void NextMarkerDestination(ref int moveIndex, ref bool looped)
    {
        //if the time signature changes the destination marker position changes
        //based on current enemy position
        if (_metronomeTriggered)
        {
            _signatureIsChanged = !_signatureIsChanged;
            if (!looped)
            {
                moveIndex = _moveDestinations.Count - 1;
            }
            else
            {
                moveIndex = 0;
            }

            if (!_signatureIsChanged)
            {
                _firstTurnBack = true;
            }
            _metronomeTriggered = false;
        }

        //not at the end of our list of moves.
        if (moveIndex < _moveDestinations.Count - 1)
        {
            if (!looped)
            {
                //move forward according to time signature
                moveIndex += _enemyMovementTime;

                //if time signature exceeds enemy position count start reversing
                HandleOverflow(ref _indicatorIndex, ref looped);
            }
            else
            {
                moveIndex-=_enemyMovementTime;
                //we've returned to the start, so reset everything to be back as normal
                if (moveIndex <= 0)
                {
                    if (_signatureIsChanged)
                    {
                        moveIndex = _moveDestinations.Count - 1;
                    }
                    else
                    {
                        moveIndex = 0;
                    }
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
                moveIndex -= _enemyMovementTime;
                if (!_firstTurnBack)
                {
                    looped = true;
                }
                else
                {
                    moveIndex += _enemyMovementTime;
                    _firstTurnBack = false;
                }

                //if time signature exceeds enemy position count start reversing
                HandleOverflow(ref _indicatorIndex, ref looped);
            }
            else
            {
                //our moves will start with the enemy time signature since we're circularly repeating our movement.
                moveIndex = _currentEnemyIndex + _enemyMovementTime;

                //if the number of moves exceeds the list count start from 0 and then add the amount remaining.
                if (moveIndex > _moveDestinations.Count - 1)
                {
                    int offsetCircular = moveIndex - (_moveDestinations.Count - 1);
                    moveIndex = 0;
                    moveIndex += offsetCircular;
                }
            }
        }
    }

    /// <summary>
    /// Handles moveIndex for the NextMarkerDestination function if the
    /// time signature movement exceeds the boundaries of 0 or the
    /// _moveDestinations list count.
    /// </summary>
    /// <param name="moveIndex">Reference to the evaluated move index.</param>
    /// <param name="looped">Reference to the evaluated loop state.</param>
    private void HandleOverflow(ref int moveIndex, ref bool looped)
    {
        //If going back through the list check for less than before
        //greater than.
        if (looped)
        {
            //Increases if below 0
            if (moveIndex < 0)
            {
                _offsetDestCount = -moveIndex;
                moveIndex = 0;
                moveIndex += _offsetDestCount;
            }
            //Decreases if over _moveDestinations count
            if (moveIndex > _moveDestinations.Count - 1)
            {
                _offsetDestCount = -moveIndex;
                moveIndex = _moveDestinations.Count - 1;
                moveIndex += _offsetDestCount;
            }
        }
        //If going normally through the list check for greater than before
        //less than.
        else
        {
            //Decreases if over _moveDestinations count
            if (moveIndex > _moveDestinations.Count - 1)
            {
                _offsetDestCount = -moveIndex;
                moveIndex = _moveDestinations.Count - 1;
                moveIndex += _offsetDestCount;
            }
            //Increases if below 0
            if (moveIndex < 0)
            {
                _offsetDestCount = -moveIndex;
                moveIndex = 0;
                moveIndex += _offsetDestCount;
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
        if (_isSonEnemy)
        {
            if (_animator != null)
            {
                _animator.SetBool(Frozen, true);
            }
            _isFrozen = true;
        }
    }

    /// <summary>
    /// Unfreezes the enemy.
    /// </summary>
    public void OnLaserExit()
    {
        if (_animator != null)
        {
            _animator.SetBool(Frozen, false);
        }
        _isFrozen = false;
    }

    public bool HitWrapAround
    {
        get => _isSonEnemy;
    }

    /// <summary>
    /// Places this object in the center of its grid cell
    /// </summary>
    public void SnapToGridSpace()
    {
        Vector3Int cellPos = GridBase.Instance.WorldToCell(transform.position);
        Vector3 worldPos = GridBase.Instance.CellToWorld(cellPos);
        transform.position = new Vector3(worldPos.x, transform.position.y, worldPos.z) + _positionOffset;
    }
}