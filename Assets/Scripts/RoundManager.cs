/******************************************************************
 *    Author: Alec Pizziferro
 *    Contributors: Nick Grinstead, Trinity Hutson
 *    Date Created: 10/22/24
 *    Description: Manager for turn based movement mechanics.
 *******************************************************************/

using System;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Enum for the current state. Must contain a delimiter.
/// Contains Player, World and None.
/// </summary>
public enum TurnState
{
    Player = 0,
    Enemy = 1,
    None = 2,
}


/// <summary>
/// Manager for turn based movement mechanics.
/// </summary>
[DefaultExecutionOrder(-5000)]
public sealed class RoundManager : MonoBehaviour
{
    public static RoundManager Instance { get; private set; }
    private TurnState _turnState = TurnState.None;
    private readonly Dictionary<TurnState, List<ITurnListener>> _turnListeners = new();
    private readonly Dictionary<TurnState, int> _completedTurnCounts = new();
    private PlayerControls _playerControls;
    private Vector3 _lastMovementInput;
    private bool _movementRegistered = false;
    private float _movementRegisteredTime = -1;
    [SerializeField] private float _inputBufferWindow = 0.5f;
    [SerializeField] private EventReference _playerTurnEvent;
    [SerializeField] private EventReference _enemyTurnEvent;

    /// <summary>
    /// Whether someone is having their turn.
    /// </summary>
    public bool TurnInProgress => _turnState != TurnState.None;

    /// <summary>
    /// Whether it's the player's turn.
    /// </summary>
    public bool IsPlayerTurn => _turnState == TurnState.Player;

    /// <summary>
    /// Whether enemies exist in the given scene.
    /// </summary>
    public bool EnemiesPresent => _turnListeners[TurnState.Enemy].Count > 0;

    /// </summary>
    /// Whether it's the enemy's turn.
    /// </summary>
    public bool IsEnemyTurn => _turnState == TurnState.Enemy;

    /// <summary>
    /// Sets the singleton instance and initializes the dictionaries for
    /// state tracking.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(Instance.gameObject);
            Instance = this;
        }

        _playerControls = new PlayerControls();

        for (int i = 0; i <= (int)TurnState.None; i++)
        {
            _turnListeners.Add((TurnState)i, new List<ITurnListener>());
            _completedTurnCounts.Add((TurnState)i, 0);
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Resets default turn events since we can't set default values anymore.
    /// </summary>
    private void Reset()
    {
        _playerTurnEvent = EventReference.Find("event:/Turn Start Player");
        _enemyTurnEvent = EventReference.Find("event:/Turn Start Enemy");
    }
#endif


    /// <summary>
    /// Enables the player controls and hooks a callback for movement input.
    /// </summary>
    private void OnEnable()
    {
        _playerControls.Enable();
        _playerControls.InGame.Movement.performed += RegisterMovementInput;
    }

    /// <summary>
    /// Disables the player controls and un-hooks a callback for movement input.
    /// </summary>
    private void OnDisable()
    {
        _playerControls.InGame.Movement.performed -= RegisterMovementInput;
        _playerControls.Disable();
    }

    /// <summary>
    /// Allows a direction input to be held when it comes to movement
    /// </summary>
    private void Update()
    {
        if (_playerControls.InGame.Movement.IsPressed() && !TurnInProgress)
        {
            PerformMovement();
        }
    }

    /// <summary>
    /// Invoked when a movement input is pressed.
    /// Will attempt to move if possible, but if it's not the player's turn
    /// the movement will be rejected.
    /// </summary>
    /// <param name="obj"></param>
    private void RegisterMovementInput(InputAction.CallbackContext obj)
    {
        Vector2 input = _playerControls.InGame.Movement.ReadValue<Vector2>();
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            input.y = 0;
        }
        else
        {
            input.x = 0;
        }
        Vector3 dir = new Vector3(input.x, 0f, input.y);
        _lastMovementInput = dir;
        if (_turnState != TurnState.None)
        {
            return;
        }
        _movementRegistered = true;
        _movementRegisteredTime = Time.unscaledTime;

        if (_turnState != TurnState.None) return;

        PerformMovement();
    }

    /// <summary>
    /// Helper method for performing movement. Aids in performing buffered inputs.
    /// </summary>
    private void PerformMovement()
    {
        if (!_movementRegistered) return;

        if (!_playerControls.InGame.Movement.IsPressed())
        {
            _movementRegistered = false;
            return;
        }

        _turnState = TurnState.Player;

        //only play player turn sound if there's enemies in the scene.
        if (_turnListeners[TurnState.Enemy].Count > 0)
        {
            AudioManager.Instance.PlaySound(_playerTurnEvent);
        }
        //perform the turn now so that it's frame perfect.
        foreach (var turnListener in _turnListeners[TurnState.Player])
        {
            turnListener.BeginTurn(_lastMovementInput);
        }
    }

    /// <summary>
    /// Call this method to complete the turn of the entity.
    /// </summary>
    /// <param name="listener"></param>
    public void CompleteTurn(ITurnListener listener)
    {
        //don't complete if it's not our turn. this shouldn't happen
        if (listener.TurnState != _turnState &&
            (listener.SecondaryTurnState != _turnState ||
            listener.SecondaryTurnState == TurnState.None))
        {
            Debug.LogError("Tried to complete turn while it wasn't our turn state." +
                           $" Listener {listener.TurnState}, state {_turnState}");
            return;
        }

        TurnState listenerTurnState = listener.TurnState == _turnState ?
            listener.TurnState : listener.SecondaryTurnState;

        //check if all entities in this turn state have completed their turn.
        _completedTurnCounts[listenerTurnState]++;
        if (_completedTurnCounts[listenerTurnState] < _turnListeners[listenerTurnState].Count) return;
        _completedTurnCounts[listenerTurnState] = 0;

        //find out who's turn is next, if it's nobody's, stop.
        var next = GetNextTurn(_turnState);
        if (next is null or TurnState.None)
        {
            _turnState = TurnState.None;
            // Attempts to move player if they buffered an input
            if(Time.unscaledTime - _movementRegisteredTime <= _inputBufferWindow)
                PerformMovement();

            return;
        }

        //begin the next group's turns.
        _turnState = next.Value;

        while (_turnListeners[_turnState].Count == 0 && _turnState != TurnState.None)
        {
            next = GetNextTurn(_turnState);
            if (next is null)
            {
                _turnState = TurnState.None;
                break;
            }
            _turnState = next.Value;
        }

        if (_turnState != TurnState.None)
        {
            if (IsEnemyTurn)
            {
                AudioManager.Instance.PlaySound(_enemyTurnEvent);
            }
            foreach (var turnListener in _turnListeners[_turnState])
            {
                turnListener.BeginTurn(_lastMovementInput);
            }
        }
    }

    /// <summary>
    /// Attempts to repeat a turn state if possible.
    /// </summary>
    /// <param name="listener">The listener to repeat a turn.</param>
    public void RequestRepeatTurnStateRepeat(ITurnListener listener)
    {
        //Stops Held Movement
        _movementRegistered = false;

        // Returns if it's not the listener's turn
        if (listener.TurnState != _turnState &&
            (listener.SecondaryTurnState != _turnState ||
            listener.SecondaryTurnState == TurnState.None))
        {
            return;
        }

        TurnState listenerTurnState = listener.TurnState == _turnState ?
            listener.TurnState : listener.SecondaryTurnState;

        _completedTurnCounts[listenerTurnState] = 0;
        var prev = GetPreviousTurn(listenerTurnState);
        if (prev is null or TurnState.None)
        {
            _turnState = TurnState.None;
            return;
        }

        _turnState = prev.Value;
        foreach (var turnListener in _turnListeners[_turnState])
        {
            turnListener.BeginTurn(_lastMovementInput);
        }
    }

    /// <summary>
    /// Registers the listener to the manager.
    /// Will attempt to start the entity's turn
    /// if possible. 
    /// </summary>
    /// <param name="listener">The listener to add.</param>
    public void RegisterListener(ITurnListener listener)
    {
        if (listener == null) return;

        bool addedListener = false;
        if (!_turnListeners[listener.TurnState].Contains(listener))
        {
            _turnListeners[listener.TurnState].Add(listener);
            addedListener = true;
        }
        if (listener.SecondaryTurnState != TurnState.None &&
            !_turnListeners[listener.SecondaryTurnState].Contains(listener))
        {
            _turnListeners[listener.SecondaryTurnState].Add(listener);
            addedListener = true;
        }
        if (!addedListener) { return; }

        //we added something during mid turn!
        if (listener.TurnState != _turnState && 
            (listener.SecondaryTurnState != _turnState || 
            listener.SecondaryTurnState == TurnState.None)) return;
        listener.BeginTurn(_lastMovementInput);
    }

    /// <summary>
    /// Removes the listener from the manager.
    /// </summary>
    /// <param name="listener">The listener to remove.</param>
    public void UnRegisterListener(ITurnListener listener)
    {
        if (listener == null) return;
        if (_turnListeners[listener.TurnState].Contains(listener))
        {
            _turnListeners[listener.TurnState].Remove(listener);
        }
        if (listener.SecondaryTurnState != TurnState.None &&
            _turnListeners[listener.SecondaryTurnState].Contains(listener))
        {
            _turnListeners[listener.SecondaryTurnState].Remove(listener);
        }
    }

    /// <summary>
    /// Helper method to get the next turn state.
    /// </summary>
    /// <param name="turnState">The turn state to compare against.</param>
    /// <returns>A future turn, can be null.</returns>
    private static TurnState? GetNextTurn(TurnState turnState)
    {
        return turnState switch
        {
            TurnState.Player => TurnState.Enemy,
            TurnState.Enemy => TurnState.None,
            _ => null
        };
    }

    /// <summary>
    /// Helper method to get the previous turn state.
    /// </summary>
    /// <param name="turnState">The turn state to compare against.</param>
    /// <returns>A past turn, can be null.</returns>
    private static TurnState? GetPreviousTurn(TurnState turnState)
    {
        return turnState switch
        {
            TurnState.Player => TurnState.None,
            TurnState.Enemy => TurnState.Player,
            _ => null
        };
    }
}