/******************************************************************
 *    Author: Alec Pizziferro
 *    Contributors: N/A
 *    Date Created: 10/22/24
 *    Description: Manager for turn based movement mechanics.
 *******************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Enum for the current state. Must contain a delimiter.
/// Contains Player, World and None.
/// </summary>
public enum TurnState
{
    Player = 0,
    World = 1,
    Enemy = 2,
    None = 3,
}

/// <summary>
/// Interface for something that needs its turn managed.
/// Both methods must be implemented.
/// </summary>
public interface ITurnListener
{
    /// <summary>
    /// The category of turn this entity will be a part of.
    /// </summary>
    TurnState TurnState { get; }

    /// <summary>
    /// Method that gets called when the entity's turn begins.
    /// This should be used to start a movement animation or
    /// some other logic. 
    /// </summary>
    /// <param name="direction">The user input direction.</param>
    void BeginTurn(Vector3 direction);

    /// <summary>
    /// Method that gets called to end an entity's turn early.
    /// </summary>
    void ForceTurnEnd();
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

    /// <summary>
    /// Whether someone is having their turn.
    /// </summary>
    public bool TurnInProgress => _turnState != TurnState.None;

    /// <summary>
    /// Whether it's the player's turn.
    /// </summary>
    public bool IsPlayerTurn => _turnState == TurnState.Player;

    /// <summary>
    /// Whether it's the world's turn.
    /// </summary>
    public bool IsWorldTurn => _turnState == TurnState.World;

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
    /// Invoked when a movement input is pressed.
    /// Will attempt to move if possible, but if it's not the player's turn
    /// the movement will be rejected.
    /// </summary>
    /// <param name="obj"></param>
    private void RegisterMovementInput(InputAction.CallbackContext obj)
    {
        Vector2 input = _playerControls.InGame.Movement.ReadValue<Vector2>();
        Vector3 dir = new Vector3(input.x, 0f, input.y);
        _lastMovementInput = dir;
        _movementRegistered = true;

        if (_turnState != TurnState.None) return;

        PerformMovement();
    }

    /// <summary>
    /// Helper method for performing movement. Aids in performing buffered inputs.
    /// </summary>
    private void PerformMovement()
    {
        if (!_movementRegistered) return;

        _movementRegistered = false;
        _turnState = TurnState.Player;
        //we now wait on the update method to catch the end of the players turn
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
        if (listener.TurnState != _turnState) //don't complete if it's not our turn. this shouldn't happen
        {
            Debug.LogError("Tried to complete turn while it wasn't our turn state." +
                           $" Listener {listener.TurnState}, state {_turnState}");
            return;
        }

        //check if all entities in this turn state have completed their turn.
        _completedTurnCounts[listener.TurnState]++;
        if (_completedTurnCounts[listener.TurnState] < _turnListeners[listener.TurnState].Count) return;
        _completedTurnCounts[listener.TurnState] = 0;

        //find out who's turn is next, if it's nobody's, stop.
        var next = GetNextTurn(_turnState);
        if (next is null or TurnState.None)
        {
            _turnState = TurnState.None;
            // Attempts to move player if they buffered an input
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
        if (listener.TurnState != _turnState)
        {
            return;
        }

        _completedTurnCounts[listener.TurnState] = 0;
        var prev = GetPreviousTurn(listener.TurnState);
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
        if (_turnListeners[listener.TurnState].Contains(listener)) return;
        _turnListeners[listener.TurnState].Add(listener);
        //we added something during mid turn!
        if (listener.TurnState != _turnState) return;
        listener.BeginTurn(_lastMovementInput);
    }

    /// <summary>
    /// Removes the listener from the manager.
    /// </summary>
    /// <param name="listener">The listener to remove.</param>
    public void UnRegisterListener(ITurnListener listener)
    {
        if (listener == null) return;
        if (!_turnListeners[listener.TurnState].Contains(listener)) return;
        _turnListeners[listener.TurnState].Remove(listener);
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
            TurnState.Player => TurnState.World,
            TurnState.World => TurnState.Enemy,
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
            TurnState.World => TurnState.Player,
            TurnState.Enemy => TurnState.World,
            _ => null
        };
    }
}