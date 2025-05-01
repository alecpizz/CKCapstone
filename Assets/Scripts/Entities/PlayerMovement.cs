/******************************************************************
 *    Author: Cole Stranczek
 *    Contributors: Cole Stranczek, Nick Grinstead, Alex Laubenstein, 
 *    Trinity Hutson, Alec Pizziferro, Josephine Qualls, Jamison Parks
 *    Date Created: 9/22/24
 *    Description: Script that handles the player's movement along
 *    the grid
 *******************************************************************/

using PrimeTween;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using FMODUnity;
using FMOD.Studio;
using SaintsField.Playa;
using JetBrains.Annotations;
using Unity.VisualScripting;

public class PlayerMovement : MonoBehaviour, IGridEntry, ITimeListener, ITurnListener
{
    public Vector3 FacingDirection 
    { 
        get; private set; 
    }

    public bool IsTransparent
    {
        get => true;
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

    public GameObject EntryObject
    {
        get => gameObject;
    }

    public bool CanMove
    {
        get => _canMove;
    }

    public bool PlayerDied
    {
        get => _playerDied;
    }

    public Action BeamSwitchActivation;

    [SerializeField] private PlayerInteraction _playerInteraction;

    [SerializeField] private float _delayTime = 0.1f;

    [Space]
    [PlayaInfoBox("Time to move one tile based on if there are enemies or not. " +
        "\n This will be divided by the number of tiles they will move.")]
    [SerializeField] private float _noEnemiesMovementTime = 0.25f;
    [SerializeField] private float _withEnemiesMovementTime = 0.25f;
    [PlayaInfoBox("The floor for how fast the player can move.")]
    [SerializeField] private float _minMovementTime = 0.175f;
    [Space]
    [SerializeField] private float _rotationTime = 0.05f;
    [SerializeField] private Ease _rotationEase = Ease.InOutSine;
    [SerializeField] private Ease _movementEase = Ease.OutBack;

    private bool _canMove;

    private float _movementTime;
    // Timing from metronome
    private int _playerMovementTiming = 1;
    private WaitForSeconds _waitForSeconds;
    private WaitForEndOfFrame _waitForEndOfFrame;

    //to tell when player finishes a move
    public Action OnPlayerMoveComplete;

    // Event references for the player movement sounds
    [SerializeField] private EventReference _playerMove = default;
    [SerializeField] private EventReference _playerCantMove = default;
    [SerializeField] private EventReference _playerDash = default;

    public static PlayerMovement Instance;
    private static readonly int Forward = Animator.StringToHash("Forward");
    private static readonly int Attacked = Animator.StringToHash("Attacked");
    private static readonly int Wall = Animator.StringToHash("Wall");
    private static readonly int Door = Animator.StringToHash("Door");

    private bool _playerDied;

    [SerializeField] private Animator _animator;
    //How many frames the game will wait before starting the movement tween
    [SerializeField] private int _walkFrameDelay = 2;
    //How long of a delay is done between setting the wall bool true to false
    [SerializeField] private float _wallAnimationDelay = 0.01f;

    [Space]
    [Tooltip("Distance from which the player will stop when walking into an enemy.")]
    [Range(0.01f, 2f)]
    [SerializeField] private float _attackLungeDistance = 2f;
    [Range(0.01f, 2f)]
    [SerializeField] private float _hugLungeDistance = 2f;

    [Header("Dash")]
    [SerializeField] private ParticleSystem _dashParticles;
    [SerializeField] private TrailRenderer[] _dashTrails;

    private Tween _moveTween;

    /// <summary>
    /// Sets instance upon awake.
    /// </summary>
    private void Awake()
    {
        Instance = this;
        PrimeTweenConfig.warnEndValueEqualsCurrent = false;
    }

    /// <summary>
    ///  Start is called before the first frame update
    /// </summary>
    private void Start()
    {
        _canMove = true;
        FacingDirection = new Vector3(0, 0, 0);

        SnapToGridSpace();
        GridBase.Instance.AddEntry(this);

        if (TimeSignatureManager.Instance != null)
            TimeSignatureManager.Instance.RegisterTimeListener(this);

        _waitForSeconds = new WaitForSeconds(_delayTime);
        _waitForEndOfFrame = new WaitForEndOfFrame();

        _movementTime = RoundManager.Instance.EnemiesPresent ? 
            _withEnemiesMovementTime : _noEnemiesMovementTime;
    }

    /// <summary>
    /// Registers instance to the RoundManager
    /// </summary>
    private void OnEnable()
    {
        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.RegisterListener(this);
            RoundManager.Instance.AutocompleteToggled += OnAutocompleteToggledEvent;
        }

        _dashParticles.Stop();
        foreach (TrailRenderer t in _dashTrails)
            t.emitting = false;
    }

    /// <summary>
    /// Unregistering from input actions
    /// </summary>
    private void OnDisable()
    {
        _playerDied = false;
        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.UnRegisterListener(this);
            RoundManager.Instance.AutocompleteToggled -= OnAutocompleteToggledEvent;
        }
            
        if (TimeSignatureManager.Instance != null)
            TimeSignatureManager.Instance.UnregisterTimeListener(this);
    }

    /// <summary>
    /// Unregisters input actions on player death
    /// </summary>
    public void OnDeath()
    {
        _animator.SetBool(Attacked, true);
        _playerDied = true;
        _canMove = false;
        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.UnRegisterListener(this);
            RoundManager.Instance.AutocompleteToggled -= OnAutocompleteToggledEvent;
        }
        if (TimeSignatureManager.Instance != null)
            TimeSignatureManager.Instance.UnregisterTimeListener(this);
    }

    /// <summary>
    /// Helper coroutine for performing movement with a delay
    /// </summary>
    /// <param name="moveDirection">Direction of player movement</param>
    /// <returns>Waits for short delay while moving</returns>
    private IEnumerator MovePlayer(Vector3 moveDirection)
    {
        float modifiedMovementTime = Mathf.Clamp(_movementTime / (_playerMovementTiming + 1),
            _minMovementTime, float.MaxValue);

        for (int i = 0; i < _playerMovementTiming; i++)
        {
            if (_playerDied)
                yield break;
            // Move if there is no wall below the player or if ghost mode is enabled
            var move = GridBase.Instance.GetCellPositionInDirection
                (gameObject.transform.position, moveDirection);

            var readPos = move;
            readPos.y = gameObject.transform.position.y;
            
            if ((GridBase.Instance.CellIsTransparent(move) 
                && gameObject.transform.position != readPos) ||
                (DebugMenuManager.Instance.GhostMode))
            {
                GridBase.Instance.UpdateEntryAtPosition(this, move);
                _animator.SetBool(Forward, true);
                for (int j = 0; j < _walkFrameDelay; j++)
                {
                    yield return _waitForEndOfFrame;
                }

                _moveTween = Tween.Position(transform,
                    move + CKOffsetsReference.MotherOffset, duration: modifiedMovementTime,
                    _movementEase);
                _moveTween.OnUpdate(target: this, (_, _) =>
                {
                    CheckForEnemyCollision(move);
                });

                yield return _moveTween.ToYieldInstruction();
                _animator.SetBool(Forward, false);
            }

            if (_playerMovementTiming > 1)
            {
                yield return _waitForSeconds;
            }
        }

        _canMove = true;
        OnPlayerMoveComplete?.Invoke();
    }

    /// <summary>
    /// Allows the player to check if they hit an enemy while moving
    /// </summary>
    /// <param name="move">Where the player is moving to</param>
    private bool CheckForEnemyCollision(Vector3 move)
    {
        var gridEntries = GridBase.Instance.GetCellEntries(move);

        foreach (var gridEntry in gridEntries)
        {
            if (gridEntry is IEnemy)
            {
                IEnemy enemy = gridEntry as IEnemy;
                enemy.AttackTarget(transform);
                // When walking into an enemy, stops the player at a reasonable distance
                // for the enemy's attack animation to play without clipping
                if (_moveTween.isAlive)
                {
                    float progress = _moveTween.progress;
                    float distance = enemy.IsSon ? _hugLungeDistance : _attackLungeDistance;
                    _moveTween.Stop();
                    Vector3 endPos = transform.position + (FacingDirection * distance);
                    Tween.Position(transform, endValue: endPos, _movementTime * (1 - progress));
                }
                OnDeath();
                SceneController.Instance.ReloadCurrentScene();
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Receives the new player movement speed when time signature updates
    /// </summary>
    /// <param name="newTimeSignature">The new time signature</param>
    public void UpdateTimingFromSignature(Vector2Int newTimeSignature)
    {
        _playerMovementTiming = newTimeSignature.x;

        if (_playerMovementTiming <= 0)
            _playerMovementTiming = 1;
    }

    public TurnState TurnState => TurnState.Player;
    public TurnState SecondaryTurnState => TurnState.None;

    /// <summary>
    /// Invoked by the round manager to start the player's turn
    /// </summary>
    /// <param name="direction">The direction the player should move</param>
    public void BeginTurn(Vector3 direction)
    { 
        if (_canMove)
        {
            _canMove = false;

            Vector3Int dir = new Vector3Int((int)direction.x, (int)direction.y, (int)direction.z);

            bool isSameDirection = FacingDirection == direction;

            FacingDirection = direction; //End of animation section

            float rotationTime = isSameDirection ? 0 : _rotationTime;

            Tween.Rotation(transform, endValue: Quaternion.LookRotation(direction), duration: rotationTime,
                ease: _rotationEase).OnComplete(
                () =>
                {
                    var move = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, direction);

                    if ((move.x != transform.position.x || move.z != transform.position.z) && 
                    (GridBase.Instance.CellIsTransparent(move) || DebugMenuManager.Instance.GhostMode))
                    {
                        AudioManager.Instance.PlaySound(_playerMove);
                        ScanForHarmonySwitches();
                        StartCoroutine(MovePlayer(direction));
                        RoundManager.Instance.CompleteTurn(this);
                    }
                    else
                    {
                        _animator.SetBool(Wall, true);
                        _canMove = true;
                        AudioManager.Instance.PlaySound(_playerCantMove);
                        RoundManager.Instance.RequestRepeatTurnStateRepeat(this);
                    }
                }).Chain(Tween.Delay(_wallAnimationDelay, () => {
                    _animator.SetBool(Wall, false);
                }));
        }
        else
        {
            RoundManager.Instance.RequestRepeatTurnStateRepeat(this);
        }
    }

    /// <summary>
    /// Called by switches to end the player turn early
    /// </summary>
    public void ForceTurnEnd()
    {
        if (!RoundManager.Instance.IsPlayerTurn) {  return; }

        StopAllCoroutines();
        GridBase.Instance.UpdateEntry(this);
        RoundManager.Instance.CompleteTurn(this);
        _canMove = true;
    }

    public void DoorTurnEnd()
    {
        _animator.SetBool(Door, true);
        if (!RoundManager.Instance.IsPlayerTurn) { return; }

        StopAllCoroutines();
        GridBase.Instance.UpdateEntry(this);
        RoundManager.Instance.CompleteTurn(this);
        _canMove = true;
    }

    /// <summary>
    /// Places this object in the center of its grid cell
    /// </summary>
    public void SnapToGridSpace()
    {
        Vector3Int cellPos = GridBase.Instance.WorldToCell(transform.position);
        Vector3 worldPos = GridBase.Instance.CellToWorld(cellPos) + CKOffsetsReference.MotherOffset;
        transform.position = worldPos;
    }

    /// <summary>
    /// Toggles dash particles when the autocomplete is toggled on/off
    /// </summary>
    /// <param name="isActive"></param>
    private void OnAutocompleteToggledEvent(bool isActive)
    {
        if (isActive)
        {
            _dashParticles.Play();
            foreach (TrailRenderer t in _dashTrails)
                t.emitting = true;
            
            if(_dashParticles.isPlaying)
            AudioManager.Instance.PlaySound(_playerDash);
        }
            
        else
        {
            _dashParticles.Stop();
            foreach (TrailRenderer t in _dashTrails)
                t.emitting = false;
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

        while (!stop && spacesChecked < _playerMovementTiming)
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
                    BeamSwitchActivation?.Invoke();
                }
                //no entry, but a cell that blocks movement. pass through.
                else if (!gridEntry.IsTransparent)
                {
                    stop = true;
                }
            }
        }
    }
}
