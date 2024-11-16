/******************************************************************
*    Author: Cole Stranczek
*    Contributors: Cole Stranczek, Nick Grinstead, Alex Laubenstein, Trinity Hutson, Alec Pizziferro, Josephine Qualls
*    Date Created: 9/22/24
*    Description: Script that handles the player's movement along
*    the grid
*******************************************************************/

using PrimeTween;
using System;
using System.Collections;
using UnityEngine;
using PrimeTween;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using FMODUnity;
using FMOD.Studio;

public class PlayerMovement : MonoBehaviour, IGridEntry, ITimeListener, ITurnListener
{
    public Vector3 FacingDirection { get; private set; }
    public bool IsTransparent { get => true; }
    public bool BlocksHarmonyBeam { get => false; }
    public Vector3 Position { get => transform.position; }
    public GameObject GetGameObject { get => gameObject; }

    [SerializeField]
    private Vector3 _positionOffset;
    [SerializeField]
    private PlayerInteraction _playerInteraction;

    [SerializeField]
    private float _delayTime = 0.1f;

    [SerializeField] private float _movementTime = 0.25f;

    private int _playerMovementTiming = 1;
    private WaitForSeconds _waitForSeconds;

    //to tell when player finishes a move
    public UnityEvent OnPlayerMoveComplete;

    // Event references for the player movement sounds
    [SerializeField] private EventReference _playerMove = default;
    [SerializeField] private EventReference _playerCantMove = default;

    public static PlayerMovement Instance;

    private const float MinMovementTime = 0.175f;
    
    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        FacingDirection = new Vector3(0, 0, 0);

        GridBase.Instance.AddEntry(this);

        if (TimeSignatureManager.Instance != null)
            TimeSignatureManager.Instance.RegisterTimeListener(this);

        _waitForSeconds = new WaitForSeconds(_delayTime);
    }

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
        float modifiedMovementTime = Mathf.Clamp(_movementTime / _playerMovementTiming,
            MinMovementTime, float.MaxValue);

        for (int i = 0; i < _playerMovementTiming; i++)
        {
            // Move if there is no wall below the player or if ghost mode is enabled
            var move = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position,
                moveDirection);
            if ((GridBase.Instance.CellIsTransparent(move)) ||
                (DebugMenuManager.Instance.GhostMode))
            {
                yield return Tween.Position(transform,
                    move + _positionOffset, duration: modifiedMovementTime, 
                    Ease.OutBack).ToYieldInstruction();
                GridBase.Instance.UpdateEntry(this);
            }
            else
            {
                break;
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
        if (!DebugMenuManager.Instance.Invincibility && collision.gameObject.CompareTag("Enemy"))
        {
            // Checks if the enemy is frozen; if they are, doesn't reload the scene
            EnemyBehavior enemy = collision.collider.GetComponent<EnemyBehavior>();
            if (enemy == null || enemy.EnemyFrozen)
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

    /// <summary>
    /// Invoked by the round manager to start the player's turn
    /// </summary>
    /// <param name="direction">The direction the player should move</param>
    public void BeginTurn(Vector3 direction)
    {
        _playerInteraction.SetDirection(direction);

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
}
