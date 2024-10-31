/******************************************************************
*    Author: Mitchell Young
*    Contributors: Mitchell Young
*    Date Created: 10/30/24
*    Description: Script that handles the behavior of the copy
*    enemy that matches player movement.
*******************************************************************/

using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using NaughtyAttributes;
using UnityEngine;

public class CopyBehavior : MonoBehaviour, IGridEntry, ITimeListener, ITurnListener
{
    public bool IsTransparent { get => true; }
    public Vector3 Position { get => transform.position; }
    public GameObject GetGameObject { get => gameObject; }

    [SerializeField]
    private Vector3 _positionOffset;
    [SerializeField]
    private PlayerInteraction _playerInteraction;
    [Required] [SerializeField] private GameObject _player;

    private float _movementTime = 0.55f;

    private int _copyMovementTiming = 1;
    private WaitForSeconds _waitForSeconds;

    // Start is called before the first frame update
    void Start()
    {
        GridBase.Instance.AddEntry(this);

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
        if (TimeSignatureManager.Instance != null)
            TimeSignatureManager.Instance.UnregisterTimeListener(this);
    }

    private IEnumerator DelayedInput(Vector3 moveDirection)
    {
        yield return null;

        for (int i = 0; i < _copyMovementTiming; ++i)
        {
            // Moves if there is no objects in the next grid space
            var move = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, moveDirection);
            var entries = GridBase.Instance.GetCellEntries(move);
            bool breakLoop = false;
            if (GridBase.Instance.CellIsEmpty(move))
            {
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
                        move + _positionOffset, _movementTime, ease: Ease.OutBack).OnUpdate<CopyBehavior>(target: this, (target, tween) =>
                        {
                            GridBase.Instance.UpdateEntry(this);
                        }).ToYieldInstruction();

                GridBase.Instance.UpdateEntry(this);
            }
            else
            {
                RoundManager.Instance.CompleteTurn(this);
                break;
            }

            if (_copyMovementTiming > 1)
            {
                yield return _waitForSeconds;
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
        _copyMovementTiming = newTimeSignature.x;

        if (_copyMovementTiming <= 0)
            _copyMovementTiming = 1;
    }
    public TurnState TurnState => TurnState.World;
    public void BeginTurn(Vector3 direction)
    {
        _playerInteraction.SetDirection(direction);
        StartCoroutine(DelayedInput(direction));
    }
}
