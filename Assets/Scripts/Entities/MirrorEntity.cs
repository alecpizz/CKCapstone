/******************************************************************
*    Author: Mitchell Young
*    Contributors: Mitchell Young
*    Date Created: 10/27/24
*    Description: Script that handles the behavior of the mirror
*    entity that matches or mirrors player movement.
*******************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorEntity : MonoBehaviour, IGridEntry, ITimeListener, ITurnListener
{
    public bool IsTransparent { get => true; }
    public Vector3 Position { get => transform.position; }
    public GameObject GetGameObject { get => gameObject; }

    [SerializeField]
    private Vector3 _positionOffset;
    [SerializeField]
    private PlayerInteraction _playerInteraction;
    [SerializeField]
    private bool reverseMirror = false;

    private int _mirrorMovementTiming = 1;
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
        if (reverseMirror)
        {
            moveDirection = -moveDirection;
        }

        for (int i = 0; i < _mirrorMovementTiming; ++i)
        {
            // Moves if there is no wall below the mirror entity
            var move = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, moveDirection);
            if (GridBase.Instance.CellIsEmpty(move))
            {
                gameObject.transform.position = move + _positionOffset;
                GridBase.Instance.UpdateEntry(this);
            }
            else
            {
                RoundManager.Instance.CompleteTurn(this);
                break;
            }

            if (_mirrorMovementTiming > 1)
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
        _mirrorMovementTiming = newTimeSignature.x;

        if (_mirrorMovementTiming <= 0)
            _mirrorMovementTiming = 1;
    }
    public TurnState TurnState => TurnState.World;
    public void BeginTurn(Vector3 direction)
    {
        _playerInteraction.SetDirection(direction);
        StartCoroutine(DelayedInput(direction));
    }
}
