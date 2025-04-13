/******************************************************************
*    Author: Mitchell Young
*    Contributors: Mitchell Young, Nick Grinstead, Jamison Parks, Alec Pizziferro
*    Date Created: 10/27/24
*    Description: Script that handles the behavior of the mirror and
*    copy enemy that mirrors or copies player movement.
*******************************************************************/

using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;
using SaintsField.Playa;
using SaintsField;

public class MirrorAndCopyBehavior : MonoBehaviour, IGridEntry, ITimeListener, ITurnListener, IHarmonyBeamEntity
{
    public bool IsTransparent { get => !EnemyFrozen; }
    public bool BlocksHarmonyBeam { get => false; }
    public bool BlocksMovingWall { get => true; }
    public Vector3 Position { get => transform.position; }
    public Transform EntityTransform { get => transform; }
    public GameObject EntryObject { get => gameObject; }

    public bool EnemyFrozen { get; private set; } = false;
    
    private GameObject _player;

    //Determines whether or not the enemy's movement is reversed
    [SerializeField] private bool _mirrored;

    [PlayaInfoBox("Time delay from when an enemy starts their turn and actually begins moving." +
      "\n This is meant to prevent enemies from moving before the player starts to move.")]
    [PropRange(0f, 0.2f)]
    [SerializeField]
    private float _timeBeforeTurn = 0.1f;

    [PlayaInfoBox("Time it takes to move one space.")]
    [SerializeField] private float _movementTime = 0.55f;

    [PlayaInfoBox("The floor for how fast the enemy can move.")]
    [SerializeField] private float _minMoveTime = 0.175f;

    [PlayaInfoBox("Time an enemy will wait if a beam switch will be pressed" +
       "\n Should be greater than beam rotation time.")]
    [SerializeField]
    private float _waitForBeamTime = 0.2f;

    // Timing from metronome
    private int _movementTiming = 1;
    private WaitForSeconds _waitForSeconds;

    [SerializeField] private float _rotationTime = 0.10f;
    [SerializeField] private Ease _rotationEase = Ease.InOutSine;
    [SerializeField] private Ease _movementEase = Ease.OutBack;

    // Bool checked if this enemy is a Son Enemy
    [SerializeField] private bool sonEnemy;

    private Rigidbody _rb;
 
    //public static PlayerMovement Instance;
    private static readonly int Forward = Animator.StringToHash("Forward");
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int Frozen = Animator.StringToHash("Frozen");
    private static readonly int Turn = Animator.StringToHash("Turn");

    [SerializeField] private Animator _animator;
    
    //
    [SerializeField] private EventReference _walkSound;

    private bool _waitOnBeam = false;

    /// <summary>
    /// Prime tween configuration
    /// </summary>
    private void Awake()
    {
        PrimeTweenConfig.warnEndValueEqualsCurrent = false;
    }

    /// <summary>
    /// Registers to the time signature and finds player
    /// </summary>
    void Start()
    {
        GridBase.Instance.AddEntry(this);
        PlayerMovement.Instance.BeamSwitchActivation += () => _waitOnBeam = true;

        _player = PlayerMovement.Instance.gameObject;
        _rb = GetComponent<Rigidbody>();

        if (TimeSignatureManager.Instance != null)
            TimeSignatureManager.Instance.RegisterTimeListener(this);
    }

    /// <summary>
    /// Unregisters from round manager
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
        PlayerMovement.Instance.BeamSwitchActivation -= () => _waitOnBeam = true;

        if (RoundManager.Instance != null)
            RoundManager.Instance.UnRegisterListener(this);

        //Unregisters time
        if (TimeSignatureManager.Instance != null)
            TimeSignatureManager.Instance.UnregisterTimeListener(this);
    }

    /// <summary>
    /// Moves the enemy in either the same direction or the opposite direction of the player.
    /// </summary>
    /// <param name="moveDirection"></param>
    /// <returns></returns>
    private IEnumerator MoveEnemy(Vector3 moveDirection)
    {
        yield return new WaitForSeconds(_timeBeforeTurn);

        // If the player is going to press a harmony switch, wait for the beam
        if (_waitOnBeam && sonEnemy)
        {
            yield return new WaitForSeconds(_waitForBeamTime);
            _waitOnBeam = false;
            HarmonyBeam.TriggerHarmonyScan?.Invoke();
        }

        if (!EnemyFrozen)
        {
            _animator.SetBool(Frozen, false);

            if (_mirrored)
            {
                moveDirection = -moveDirection;
            }

            float modifiedMovementTime = Mathf.Clamp(_movementTime / _movementTiming,
                        _minMoveTime, float.MaxValue);

            for (int i = 0; i < _movementTiming; ++i)
            {
                // Moves if there is no objects in the next grid space
                var move = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, moveDirection);
                bool canMove = GridBase.Instance.CellIsTransparent(move);

                if (canMove == true)
                {
                    if (moveDirection != transform.forward)
                    {
                        if (_animator != null)
                        {
                            _animator.SetBool(Turn, true);
                        }
                    }
                    _animator.SetBool(Forward, true);
                    Tween.Rotation(transform, endValue: Quaternion.LookRotation(moveDirection), duration: _rotationTime,
                    ease: _rotationEase);
                    if (_animator != null)
                    {
                        _animator.SetBool(Turn, false);
                    }

                    if (AudioManager.Instance != null && _mirrored)
                    {
                        AudioManager.Instance.PlaySound(_walkSound);
                    }

                    yield return Tween.Position(transform,
                        move + CKOffsetsReference.MirrorCopyEnemyOffset(_mirrored), modifiedMovementTime, ease: _movementEase).OnUpdate<MirrorAndCopyBehavior>(target: this, (target, tween) =>
                        {
                            GridBase.Instance.UpdateEntry(this);
                        }).ToYieldInstruction();
                    _animator.SetBool(Forward, false);
                    HarmonyBeam.TriggerHarmonyScan?.Invoke();

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
                }
                else
                {
                    break;
                }
            }
        }
        GridBase.Instance.UpdateEntry(this);
        RoundManager.Instance.CompleteTurn(this);
    }

    /// <summary>
    /// Receives the new player movement speed when time signature updates
    /// </summary>
    /// <param name="newTimeSignature">The new time signature</param>
    public void UpdateTimingFromSignature(Vector2Int newTimeSignature)
    {
        _movementTiming = newTimeSignature.x;

        if (_movementTiming <= 0)
            _movementTiming = 1;
    }

    public TurnState TurnState => TurnState.Enemy;
    public TurnState SecondaryTurnState => TurnState.None;

    /// <summary>
    /// Starts the enemy's movement coroutine
    /// </summary>
    /// <param name="direction">The direction the player moved</param>
    public void BeginTurn(Vector3 direction)
    {
        StartCoroutine(MoveEnemy(direction));
    }

    /// <summary>
    /// Forcibly ends the enemy's turn
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
            if (_animator != null)
            {
                _animator.SetBool(Frozen, true);
            }
            EnemyFrozen = true;
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
        EnemyFrozen = false;
    }


    public bool HitWrapAround { get => sonEnemy; }

    /// <summary>
    /// Called to center the enemy on its grid space
    /// </summary>
    public void SnapToGridSpace()
    {
        Vector3Int cellPos = GridBase.Instance.WorldToCell(transform.position);
        Vector3 worldPos = GridBase.Instance.CellToWorld(cellPos) + CKOffsetsReference.MirrorCopyEnemyOffset(_mirrored);
        transform.position = worldPos;
    }
}
