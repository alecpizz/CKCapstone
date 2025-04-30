/******************************************************************
 *    Author: Cole Stranczek
 *    Contributors: Cole Stranczek, Mitchell Young, Nick Grinstead, Alec Pizziferro, Alex Laubenstein, Trinity Hutson
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
using UnityEngine.UI;
using TMPro;
using UnityEngine.Analytics;

public class EnemyBehavior : MonoBehaviour, IGridEntry, ITimeListener,
    ITurnListener, IHarmonyBeamEntity, IEnemy
{
    public bool IsTransparent
    {
        get => !_isFrozen;
    }

    public bool BlocksHarmonyBeam
    {
        get => false;
    }

    public bool BlocksMovingWall
    {
        get => true;
    }

    public Vector3 Position
    {
        get => transform.position;
    }

    public Transform EntityTransform
    {
        get => transform;
    }


    public GameObject EntryObject
    {
        get => gameObject;
    }

    public bool IsSon
    {
        get => _isSonEnemy;
    }

    public static Action EnemyBeamSwitchActivation;

    [SerializeField] private GameObject _destinationMarker;
    [SerializeField] private GameObject _destPathVFX;
    [Space]
    [SerializeField] private GameObject _destPathMarkerPrefab;
    [Space]

    private List<GameObject> _subDestPathMarkers = new();

    private int _subMarkerIdx = 0;

    private ParticleSystem.MainModule _particleMainModule;
    private Renderer _destMarkerRenderer;
    private Renderer _pathVfxRenderer;
    private List<Renderer> _subMarkerRenderers = new();
    private List<ParticleSystem.MainModule> _subDestMainModules = new();
    private Color _defaultMarkerColor;

    [SerializeField] private Color _frozenMarkerColor;

    public bool CollidingWithRay { get; set; } = false;

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

    [PlayaInfoBox("The floor for how fast the enemy can move.")]
    [SerializeField]
    private float _minMoveTime = 0.175f;

    [PlayaInfoBox("Time an enemy will wait if a beam switch will be pressed" +
        "\n Should be greater than beam rotation time.")]
    [SerializeField]
    private float _waitForBeamTime = 0.2f;

    private bool _currentGroupToggle = true;
    private bool _currentSoloToggle = true;

    [SerializeField] private float _rotationTime = 0.10f;
    [SerializeField] private Ease _rotationEase = Ease.InOutSine;
    [SerializeField] private Ease _movementEase = Ease.OutBack;

    private bool _metronomeTriggered = false;
    private bool _notFirstCheck = false;
    private bool _isMoving = false;
    private bool _isCircling = false;

    private Tween _moveTween;
    private PrimeTween.Sequence _moveSequence;

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

    [SerializeField] private int _startPosOffset;
    private float _initialY = 0;

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

    [FormerlySerializedAs("sonEnemy")]
    [SerializeField]
    private bool _isSonEnemy;

    [Space]
    [Tooltip("Distance from which the enemy will stop when walking into the player.")]
    [Range(0.01f, 2f)]
    [SerializeField] private float _attackLungeDistance;

    private readonly List<Vector3Int> _moveDestinations = new();

    // Timing from metronome
    private int _enemyMovementTime = 1;
    private int _prevMovementTime = 1;

    private Rigidbody _rb;

    private int _moveIndex = 0;
    private bool _isReturningToStart = false;
    private int _indicatorIndex = 0;
    private bool _indicatorReturningToStart = false;
    private int _currentEnemyIndex = 0;
    private Vector3 _lastPosition;
    private bool _waitOnBeam = false;
    private bool _didHitPlayer = false;
    private Vector3 _rotationDir;

    //public static PlayerMovement Instance;
    private static readonly int Forward = Animator.StringToHash("Forward");
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int Frozen = Animator.StringToHash("Frozen");
    private static readonly int Turn = Animator.StringToHash("Turn");

    [SerializeField] private Animator _animator;

    [SerializeField] private float _destPathVFXMatSpeed = -0.25f;
    [SerializeField] private Material _destPathMaterial;

    /// <summary>
    /// Disables a PrimeTween warning.
    /// </summary>
    private void Awake()
    {
        PrimeTweenConfig.warnEndValueEqualsCurrent = false;
        PrimeTweenConfig.warnZeroDuration = false;
        SnapToGridSpace();
        BuildCellList();
        GridBase.Instance.AddEntry(this);

        _destPathVFXMatSpeed = -_destPathVFXMatSpeed;
        _destPathMaterial.SetFloat("_Speed", _destPathVFXMatSpeed);

        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;

        ParticleSystem markerParticleSystem = _destinationMarker.GetComponentInChildren<ParticleSystem>();
        _particleMainModule = markerParticleSystem.main;
        _destMarkerRenderer = _destinationMarker.GetComponentInChildren<Renderer>();
        _pathVfxRenderer = _destPathVFX.GetComponent<Renderer>();
        
        _defaultMarkerColor = _destMarkerRenderer.material.color;

        _lastPosition = transform.position;

        _destinationMarker.transform.SetParent(null);

        // Make sure enemies are always seen at the start
        _vfxLine = _destPathVFX.GetComponent<LineRenderer>();
        _vfxLine.positionCount = 2;

        // Disable sub markers
        ActivateDestinationMarkers(false);
    }

    /// <summary>
    /// Registers the instance in the RoundManager.
    /// </summary>
    private void Start()
    {
        if (TimeSignatureManager.Instance != null)
        {
            TimeSignatureManager.Instance.RegisterTimeListener(this);
        }
        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.RegisterListener(this);
        }
        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.BeamSwitchActivation += () => _waitOnBeam = true;
        }

        EnemyBeamSwitchActivation += () => _waitOnBeam = true;

        InitializeDestinationMarkers();

        _initialY = transform.position.y;
        StartPositionOffset();

        UpdateDestinationMarker();
        DestinationPath();
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
                    var color = Color.HSVToRGB((float)i / cells.Count, 1f, 1f);
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
                    var color = Color.HSVToRGB((float)i / _moveDestinations.Count, 1f, 1f);
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
        PlayerMovement.Instance.BeamSwitchActivation -= () => _waitOnBeam = true;
        EnemyBeamSwitchActivation -= () => _waitOnBeam = true;

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
    /// Scans for switches to alert enemies to wait for the beams to rotate.
    /// This ensures enemies don't move while being hit by a beam.
    /// </summary>
    private void ScanForHarmonySwitches()
    {
        var currTilePos = (transform.position);
        var fwd = transform.forward;
        bool stop = false;
        int spacesChecked = 0;

        while (!stop && spacesChecked < _enemyMovementTime)
        {
            spacesChecked++;

            var nextCell = GridBase.Instance.GetCellPositionInDirection(currTilePos, fwd);
            if (currTilePos == nextCell) //no where to go :(
            {
                stop = true;
            }

            currTilePos = nextCell;

            var entries = GridBase.Instance.GetCellEntries(nextCell);
            foreach (var gridEntry in entries) //check each cell
            {
                if (gridEntry == null) continue;
                //the entry has a switch type :)
                if (gridEntry.EntryObject.TryGetComponent(out SwitchTrigger entity))
                {
                    if (entity.HarmonyBeamsPresent)
                    {
                        EnemyBeamSwitchActivation?.Invoke();
                        // Reset this enemy's boolean so it can step on the switch
                        _waitOnBeam = false;
                    }
                }
                //no entry, but a cell that blocks movement. pass through.
                else if (!gridEntry.IsTransparent)
                {
                    stop = true;
                }
            }
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

        ActivateDestinationMarkers(CollidingWithRay);

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

            ActivateDestinationMarkers(true);

            _currentGroupToggle = true;
        }
        else
        {
            ActivateDestinationMarkers(_currentGroupToggle);

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
    /// Instantiates sub markers. Instantiates sub markers based on the highest time sig in the current scene
    /// </summary>
    private void InitializeDestinationMarkers()
    {
        if (TimeSignatureManager.Instance == null)
            return;

        int timeSig1 = TimeSignatureManager.Instance.GetCurrentTimeSignature().y;
        int timeSig2 = TimeSignatureManager.Instance.GetNextTimeSignature().y;

        int maxTimeSig = Mathf.Max(timeSig1, timeSig2);

        for (int i = 0; i < maxTimeSig - 1; i++)
        {
            GameObject obj = Instantiate(_destPathMarkerPrefab);

            Vector3 scale = obj.transform.localScale;
            obj.transform.localScale = scale;

            //obj.SetActive(false);

            _subDestPathMarkers.Add(obj);
            _subMarkerRenderers.Add(obj.GetComponentInChildren<Renderer>());
            ParticleSystem subDestParticles = obj.GetComponentInChildren<ParticleSystem>();
            _subDestMainModules.Add(subDestParticles.main);
        }
    }

    /// <summary>
    /// Toggles the VFX for the destination path
    /// </summary>
    /// <param name="active"></param>
    /// <param name="ignorePathLine"></param>
    private void ActivateDestinationMarkers(bool active, bool ignorePathLine = false)
    {
        if (!ignorePathLine)
            _destPathVFX.SetActive(active);

        _destinationMarker.SetActive(active);

        foreach (var o in _subDestPathMarkers)
        {
            o.SetActive(active);
        }
    }

    /// <summary>
    /// Updates an individual submarker's position based on the move index provided
    /// </summary>
    /// <param name="moveIdx">Enemy move index</param>
    private void UpdateSubMarker(int moveIdx)
    {
        var gridPoint = _moveDestinations[moveIdx];
        var worldPoint = GridBase.Instance.CellToWorld(gridPoint);
        worldPoint.y += _destYPos;
        _subDestPathMarkers[_subMarkerIdx].transform.position = worldPoint;

        _subMarkerIdx++;
    }

    /// <summary>
    /// This function updates the position of the _destinationMarker object using the
    /// _movePoints list.
    /// </summary>
    private void UpdateDestinationMarker()
    {
        //Sets the _destinationMarker object to the enemy's current position
        _destinationMarker.transform.position = _lastPosition;
        Vector3 linePos = _lastPosition;
        linePos.y = _lineYPosOffset;
        //Looks at the time signature for the enemy so it can place multiple moves in advance
        NextMarkerDestination(ref _indicatorIndex, ref _indicatorReturningToStart);

        //UpdateSubMarkers(prevIndex);

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

        UpdateAllSubMarkers();
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

        _prevMovementTime = !_notFirstCheck ? newTimeSignature.y : _enemyMovementTime;

        if (!_notFirstCheck)
        {
            _notFirstCheck = true;
        }

        _enemyMovementTime = newTimeSignature.y;

        if (_enemyMovementTime <= 0)
        {
            _enemyMovementTime = 1;
        }

        if (_metronomeTriggered)
        {
            UpdateDestinationMarker();
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

        ScanForHarmonySwitches();

        _isMoving = true;
        _lastPosition = transform.position;
        StartCoroutine(MovementRoutine());
    }

    /// <summary>
    /// Updates ALL sub markers by simulating the enemy taking their turn
    /// </summary>
    public void UpdateAllSubMarkers()
    {
        int tempMoveIndex = _moveIndex;
        bool tempLooping = _isReturningToStart;
        Vector3 lastSubPosition = transform.position;

        bool blocked = false;

        _subMarkerIdx = 0;

        for (int i = 0; i < _enemyMovementTime; i++)
        {
            int prevMove = tempMoveIndex;
            bool prevReturn = tempLooping;
            bool isVFX = true;
            EvaluateNextMove(ref tempMoveIndex, ref tempLooping, ref isVFX);

            if (i < _enemyMovementTime - 1)
            {
                UpdateSubMarker(tempMoveIndex);
                lastSubPosition = _subDestPathMarkers[_subMarkerIdx - 1].transform.position;
            }

            var movePt = _moveDestinations[tempMoveIndex];
            var currCell = GridBase.Instance.WorldToCell(transform.position);
            var goalCell = GetLongestPath(GridBase.Instance.CellToWorld(movePt), lastSubPosition);

            //we were blocked by something, adjust memory
            if (goalCell != movePt)
            {
                blocked = true;
                //we can still make the move in the future, we just need to try again because we were blocked.
                tempMoveIndex = prevMove;
                tempLooping = prevReturn;
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
        }

        for (int i = _subMarkerIdx; i < _subDestPathMarkers.Count; i++)
            _subDestPathMarkers[i].transform.position = Vector3.one * 1000;
    }

    /// <summary>
    /// Function that changes an enemy's starting position within their move
    /// points list for the first turn.
    /// </summary>
    private void StartPositionOffset()
    {
        if (_startPosOffset >= _moveDestinations.Count - 1 || _startPosOffset <= 0)
        {
            return;
        }

        for (int i = 0; i < _startPosOffset; i++)
        {
            bool isVFX = true;
            EvaluateNextMove(ref _moveIndex, ref _isReturningToStart, ref isVFX);

            var movePt = _moveDestinations[_moveIndex];
            var goalCell = GetLongestPath(GridBase.Instance.CellToWorld(movePt), transform.position);
            var moveWorld = GridBase.Instance.CellToWorld(goalCell);

            Vector3 changeTransform = moveWorld;
            changeTransform.y = _initialY;
            transform.position = changeTransform;
        }

        _moveIndex = _startPosOffset;
        _indicatorIndex = _startPosOffset;
    }

    /// <summary>
    /// Coroutine for handling enemy movement. Handles determining pathing,
    /// Tween movement, and updating the indicator.
    /// </summary>
    /// <returns>null</returns>
    private IEnumerator MovementRoutine()
    {
        yield return new WaitForSeconds(_timeBeforeTurn);

        // If the player is going to press a harmony switch, wait for the beam
        if (_waitOnBeam)
        {
            yield return new WaitForSeconds(_waitForBeamTime);
            _waitOnBeam = false;
            HarmonyBeam.TriggerHarmonyScan?.Invoke();
        }

        if (_isFrozen || _didHitPlayer)
        {
            RoundManager.Instance.CompleteTurn(this);
            yield break;
        }

        bool blocked = false;

        for (int i = 0; i < _enemyMovementTime; i++)
        {
            if (_didHitPlayer)
                continue;

            int prevMove = _moveIndex;
            bool prevReturn = _isReturningToStart;
            bool isVFX = false;
            EvaluateNextMove(ref _moveIndex, ref _isReturningToStart, ref isVFX);

            var movePt = _moveDestinations[_moveIndex];
            var currCell = GridBase.Instance.WorldToCell(transform.position);
            var goalCell = GetLongestPath(GridBase.Instance.CellToWorld(movePt), transform.position);
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
            _rotationDir = (GridBase.Instance.CellToWorld(goalCell) - transform.position).normalized;
            _rotationDir.y = 0f;
            var moveWorld = GridBase.Instance.CellToWorld(goalCell);

            dist = Mathf.Max(dist, 1f);
            float movementTime = Mathf.Clamp((_waitTime / _enemyMovementTime) * dist,
                _minMoveTime, float.MaxValue);
            _moveTween = Tween
                .Position(transform, endValue: moveWorld + CKOffsetsReference.EnemyOffset(_isSonEnemy),
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
                            _didHitPlayer = true;
                            PlayerMovement.Instance.OnDeath();
                            // When walking into a player, stops the enemy at a reasonable distance
                            // for the enemy's attack animation to play without clipping
                            if (_moveSequence.isAlive)
                            {
                                float progress = _moveSequence.progress;
                                _moveSequence.Stop();
                                Vector3 direction = PlayerMovement.Instance.Position - transform.position;
                                direction.y = 0;
                                Vector3 endPos = transform.position + (direction.normalized * _attackLungeDistance);
                                Tween.Position(transform, endValue: endPos, _enemyMovementTime * (1 - progress));
                            }
                            if (_animator != null)
                            {
                                _animator.SetBool(Attack, true);
                            }
                            SceneController.Instance.ReloadCurrentScene();
                        }
                    });
            AudioManager.Instance.PlaySound(_enemyMove);

            /*if (rotationDir != transform.forward && _animator != null)
            {
                _animator.SetBool(Turn, true);
            }*/
            _moveSequence = Tween.Rotation(transform, endValue: Quaternion.LookRotation(_rotationDir),
                duration: _rotationTime,
                ease: _rotationEase).Chain(Tween.Delay(_enemyRotateToMovementDelay)).Chain(_moveTween);
            yield return _moveSequence.ToYieldInstruction();
            /*if (_animator != null)
            {
                _animator.SetBool(Turn, false);
            }*/
            if (_animator != null)
            {
                _animator.SetBool(Forward, false);
            }
            GridBase.Instance.UpdateEntry(this);

            /*if (_animator != null)
            {
                _animator.SetBool(Turn, false);
            }*/
        }

        _isMoving = false;

        if (!blocked)
        {
            UpdateDestinationMarker();
        }

        if (_waitOnBeam)
            _waitOnBeam = false;

        if (_moveIndex == 0 || _moveIndex == _moveDestinations.Count - 1)
        {
            /*if (_animator != null)
            {
                _animator.SetBool(Turn, false);
            }*/
            if(!_didHitPlayer)
                yield return Tween.Rotation(transform, endValue: Quaternion.LookRotation(-_rotationDir),
                    duration: _rotationTime,
                    ease: _rotationEase).Chain(Tween.Delay(_enemyRotateToMovementDelay)).ToYieldInstruction();

        }

        RoundManager.Instance.CompleteTurn(this);
    }

    /// <summary>
    /// Determines the longest path from the enemy's current position
    /// to its goal. WIll stop if it's blocked by something.
    /// </summary>
    /// <param name="goal">The goal position, in world space to get to.</param>
    /// <returns>The updated goal cell.</returns>
    private Vector3Int GetLongestPath(Vector3 goal, Vector3 origin)
    {
        var direction = (goal - origin);
        direction.Normalize();
        bool stop = false;
        var originTilePos = GridBase.Instance.WorldToCell(origin);
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
    /// /// <param name="looped">Reference to the evaluated call location.</param>
    private void EvaluateNextMove(ref int moveIndex, ref bool looped, ref bool isVFX)
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
                    //checks if there is only one move in the list and will add a movement if so
                    //to prevent skipping a turn
                    if (_moveDestinations.Count - 1 == 1)
                    {
                        moveIndex++;
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
                looped = true;
                moveIndex--;
            }
            else
            {
                //our moves will start with 1 since we're circularly repeating our movement.
                moveIndex = 1;
                _isCircling = false;
            }
        }

        _currentEnemyIndex = moveIndex;

        //If moveIndex is at the first or last position the destination path vfx will reverse
        if (!isVFX)
        {
            if (_circularMovement)
            {
                return;
            }

            if (moveIndex == 0 || moveIndex == _moveDestinations.Count - 1)
            {
                _destPathVFXMatSpeed = -_destPathVFXMatSpeed;
                _destPathMaterial.SetFloat("_Speed", _destPathVFXMatSpeed);
            }
        }
    }


    /// <summary>
    /// Determines the next move index for the enemy's pathing indicator
    /// using the current time signature.
    /// </summary>
    /// <param name="moveIndex">Reference to the evaluated move index.</param>
    /// <param name="looped">Reference to the evaluated loop state.</param>
    private void NextMarkerDestination(ref int moveIndex, ref bool looped)
    {
        int changeInIndex = _metronomeTriggered ? _enemyMovementTime - _prevMovementTime :
            _enemyMovementTime;

        _metronomeTriggered = false;

        if (_circularMovement)
        {
            moveIndex += changeInIndex;

            if (moveIndex < 0)
            {
                moveIndex = _moveDestinations.Count - -moveIndex;
            }

            if (moveIndex > _moveDestinations.Count - 1)
            {
                moveIndex %= _moveDestinations.Count - 1;
                _isCircling = true;
            }

            if (moveIndex < _currentEnemyIndex && _isMoving && !_isCircling)
            {
                moveIndex = _currentEnemyIndex;
            }
            else if (moveIndex < _currentEnemyIndex && !_isCircling)
            {
                moveIndex = _currentEnemyIndex + changeInIndex;
                moveIndex %= _moveDestinations.Count - 1;
                _isCircling = true;
            }
        }
        else
        {
            if (changeInIndex < 0)
            {
                looped = !looped;
            }

            if (looped)
            {
                moveIndex -= changeInIndex;

                if (moveIndex > _currentEnemyIndex && _isMoving)
                {
                    moveIndex = _currentEnemyIndex;
                }
            }
            else
            {
                moveIndex += changeInIndex;

                if (moveIndex < _currentEnemyIndex && _isMoving)
                {
                    moveIndex = _currentEnemyIndex;
                }
            }

            int offsetIndex;

            while (moveIndex < 0 || moveIndex > _moveDestinations.Count - 1)
            {
                if (moveIndex < 0)
                {
                    moveIndex = -moveIndex;
                    looped = false;
                }
                else if (moveIndex > _moveDestinations.Count - 1)
                {
                    offsetIndex = moveIndex % (_moveDestinations.Count - 1);
                    moveIndex = (_moveDestinations.Count - 1) - offsetIndex;
                    looped = true;
                }
            }

            if (moveIndex == 0 || moveIndex == _moveDestinations.Count - 1)
            {
                looped = !looped;
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

            _particleMainModule.startColor = _frozenMarkerColor;
            _destMarkerRenderer.material.color = _frozenMarkerColor;
            _pathVfxRenderer.material.color = _frozenMarkerColor;
            foreach (var subMarker in _subMarkerRenderers)
            {
                subMarker.material.color = _frozenMarkerColor;
            }

            ParticleSystem.MainModule temp;
            for (int i = 0; i < _subDestMainModules.Count; ++i)
            {
                temp = _subDestMainModules[i];
                temp.startColor = _frozenMarkerColor;
            }
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

        _particleMainModule.startColor = _defaultMarkerColor;
        _destMarkerRenderer.material.color = _defaultMarkerColor;
        _pathVfxRenderer.material.color = _defaultMarkerColor;
        foreach (var subMarker in _subMarkerRenderers)
        {
            subMarker.material.color = _defaultMarkerColor;
        }

        ParticleSystem.MainModule temp;
        for (int i = 0; i < _subDestMainModules.Count; ++i)
        {
            temp = _subDestMainModules[i];
            temp.startColor = _defaultMarkerColor;
        }
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
        transform.position = worldPos + CKOffsetsReference.EnemyOffset(_isSonEnemy);
    }

    /// <summary>
    /// Implementation of IEnemy
    /// Rotates to face its target and then does its attack animation
    /// </summary>
    /// <param name="target"></param>
    public void AttackTarget(Transform target)
    {
        if (_didHitPlayer)
            return;

        var rotationDir = (target.position - transform.position).normalized;
        rotationDir.y = 0f;
        Tween.Rotation(transform, endValue: Quaternion.LookRotation(rotationDir),
                duration: _rotationTime,
                ease: _rotationEase);

        if (_animator != null)
            _animator.SetBool(Attack, true);

        _didHitPlayer = true;
    }
}