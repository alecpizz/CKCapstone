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

    public Vector3 Position
    {
        get => transform.position;
    }

    public GameObject EntryObject
    {
        get => gameObject;
    }

    [SerializeField] private Vector3 _positionOffset;
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

    private float _movementTime;
    // Timing from metronome
    private int _playerMovementTiming = 1;
    private WaitForSeconds _waitForSeconds;

    //to tell when player finishes a move
    public UnityEvent OnPlayerMoveComplete;

    // Event references for the player movement sounds
    [SerializeField] private EventReference _playerMove = default;
    [SerializeField] private EventReference _playerCantMove = default;

    public static PlayerMovement Instance;
    private static readonly int Forward = Animator.StringToHash("Forward");
    private static readonly int Right = Animator.StringToHash("Right");
    private static readonly int Left = Animator.StringToHash("Left");
    private static readonly int Backward = Animator.StringToHash("Backward");

    [SerializeField] private Animator _animator;

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
        FacingDirection = new Vector3(0, 0, 0);
        if (RoundManager.Instance.EnemiesPresent)
        {
            _animator.SetBool("Enemies", true);
        }

        SnapToGridSpace();
        GridBase.Instance.AddEntry(this);

        if (TimeSignatureManager.Instance != null)
            TimeSignatureManager.Instance.RegisterTimeListener(this);

        _waitForSeconds = new WaitForSeconds(_delayTime);

        _movementTime = RoundManager.Instance.EnemiesPresent ? 
            _withEnemiesMovementTime : _noEnemiesMovementTime;
    }

    /// <summary>
    /// Registers instance to the RoundManager
    /// </summary>
    private void OnEnable()
    {
        if (RoundManager.Instance != null)
            RoundManager.Instance.RegisterListener(this);
    }

    /// <summary>
    /// Unregistering from input actions
    /// </summary>
    private void OnDisable()
    {
        if (RoundManager.Instance != null)
            RoundManager.Instance.UnRegisterListener(this);
        if (TimeSignatureManager.Instance != null)
            TimeSignatureManager.Instance.UnregisterTimeListener(this);
    }


    /// <summary>
    /// Helper coroutine for performing movement with a delay
    /// </summary>
    /// <param name="moveDirection">Direction of player movement</param>
    /// <returns>Waits for short delay while moving</returns>
    private IEnumerator MovementDelay(Vector3 moveDirection)
    {
        yield return new WaitForSeconds(_rotationTime);
        float modifiedMovementTime = Mathf.Clamp(_movementTime / _playerMovementTiming,
            _minMovementTime, float.MaxValue);

        for (int i = 0; i < _playerMovementTiming; i++)
        {
            // Move if there is no wall below the player or if ghost mode is enabled
            var move = GridBase.Instance.GetCellPositionInDirection
                (gameObject.transform.position, moveDirection);
            var readPos = move;
            readPos.y = gameObject.transform.position.y;
            
            if ((GridBase.Instance.CellIsTransparent(move) 
                && gameObject.transform.position != readPos) ||
                (DebugMenuManager.Instance.GhostMode))
            {
                _animator.SetTrigger(Forward);
                yield return Tween.Position(transform,
                    move + _positionOffset, duration: modifiedMovementTime, 
                    _movementEase).ToYieldInstruction();
                GridBase.Instance.UpdateEntry(this);
            }

            if (_playerMovementTiming > 1)
            {
                yield return _waitForSeconds;
            }
        }

        RoundManager.Instance.CompleteTurn(this);
    }

    /// <summary>
    /// Reloads scene when player hits an enemy
    /// </summary>
    /// <param name="collision">Data from collision</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (!DebugMenuManager.Instance.Invincibility 
            && collision.gameObject.CompareTag("Enemy") ||
            !DebugMenuManager.Instance.Invincibility 
            && collision.gameObject.CompareTag("SonEnemy"))
        {
            // Checks if the enemy is frozen; if they are, doesn't reload the scene
            EnemyBehavior enemy = collision.collider.GetComponent<EnemyBehavior>();
            if (enemy == null)
                return;

            MirrorAndCopyBehavior mirrorCopy = collision.collider.GetComponent<MirrorAndCopyBehavior>();
            if (mirrorCopy == null)
                return;

            Time.timeScale = 0f;

            SceneController.Instance.ReloadCurrentScene();
        }
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
        Vector3Int dir = new Vector3Int((int) direction.x, (int) direction.y, (int) direction.z);

        bool isSameDirection = FacingDirection == direction;

        FacingDirection = direction; //End of animation section
        _playerInteraction.SetDirection(direction);

        float rotationTime = isSameDirection ? 0 : _rotationTime;

        Tween.Rotation(transform, endValue: Quaternion.LookRotation(direction), duration: rotationTime,
            ease: _rotationEase).OnComplete(
            () =>
            {
                var move = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, direction);
                if ((GridBase.Instance.CellIsTransparent(move) || DebugMenuManager.Instance.GhostMode))
                {
                    AudioManager.Instance.PlaySound(_playerMove);
                    StartCoroutine(MovementDelay(direction));
                    OnPlayerMoveComplete?.Invoke(); //keeps track of movement completion
                }
                else
                {
                    AudioManager.Instance.PlaySound(_playerCantMove);
                    RoundManager.Instance.RequestRepeatTurnStateRepeat(this);
                }
            });
        
    }

    /// <summary>
    /// Called by switches to end the player turn early
    /// </summary>
    public void ForceTurnEnd()
    {
        StopAllCoroutines();
        GridBase.Instance.UpdateEntry(this);
        RoundManager.Instance.CompleteTurn(this);
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
