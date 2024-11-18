/******************************************************************
*    Author: Mitchell Young
*    Contributors: Mitchell Young
*    Date Created: 10/27/24
*    Description: Script that handles the behavior of the mirror and
*    copy enemy that mirrors or copies player movement.
*******************************************************************/

using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;

public class MirrorAndCopyBehavior : MonoBehaviour, IGridEntry, ITimeListener, ITurnListener, IHarmonyBeamEntity
{
    public bool IsTransparent { get => true; }
    public bool BlocksHarmonyBeam { get => false; }
    public Vector3 Position { get => transform.position; }
    public GameObject GetGameObject { get => gameObject; }

    public bool EnemyFrozen { get; private set; } = false;

    [SerializeField]
    private Vector3 _positionOffset;
    [SerializeField]
    private PlayerInteraction _playerInteraction;
    private GameObject _player;

    //Determines whether or not the enemy's movement is reversed
    [SerializeField] private bool _mirrored;

    private float _movementTime = 0.55f;

    private int _movementTiming = 1;
    private WaitForSeconds _waitForSeconds;

    [SerializeField] private float _rotationTime = 0.10f;
    [SerializeField] private Ease _rotationEase = Ease.InOutSine;

    // Bool checked if this enemy is a Son Enemy
    [SerializeField] private bool sonEnemy;

    private void Awake()
    {
        PrimeTweenConfig.warnEndValueEqualsCurrent = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        GridBase.Instance.AddEntry(this);

        _player = PlayerMovement.Instance.gameObject;

        if (TimeSignatureManager.Instance != null)
            TimeSignatureManager.Instance.RegisterTimeListener(this);
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
        if (!EnemyFrozen)
        {
            if (_mirrored)
            {
                moveDirection = -moveDirection;
            }


            for (int i = 0; i < _movementTiming; ++i)
            {
                // Moves if there is no objects in the next grid space
                var move = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, moveDirection);
                var entries = GridBase.Instance.GetCellEntries(move);
                bool canMove = true;

                if (GridBase.Instance.CellIsEmpty(move))
                {
                    //If the next cell contains an object that is not the player then the loop breaks
                    //enemy can't move into other enemies, walls, etc.
                    foreach (var entry in entries)
                    {
                        if (entry.GetGameObject != _player)
                        {
                            canMove = false;
                            break;
                        }
                    }
                    if (canMove == true)
                    {
                        Tween.Rotation(transform, endValue: Quaternion.LookRotation(moveDirection), duration: _rotationTime,
                        ease: _rotationEase);

                        yield return Tween.Position(transform,
                            move + _positionOffset, _movementTime, ease: Ease.OutBack).OnUpdate<MirrorAndCopyBehavior>(target: this, (target, tween) =>
                            {
                                GridBase.Instance.UpdateEntry(this);
                            }).ToYieldInstruction();
                    }

                    GridBase.Instance.UpdateEntry(this);
                }
                else
                {
                    if (_movementTiming > 1)
                    {
                        yield return _waitForSeconds;
                    }

                    RoundManager.Instance.CompleteTurn(this);
                    break;
                }

                if (_movementTiming > 1)
                {
                    yield return _waitForSeconds;
                }
            }
        }

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
    public TurnState TurnState => TurnState.World;

    public void BeginTurn(Vector3 direction)
    {
        StartCoroutine(MoveEnemy(direction));
    }

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
}
