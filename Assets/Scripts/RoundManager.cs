/******************************************************************
 *    Author: Alec Pizziferro
 *    Contributors: Nick Grinstead, Trinity Hutson
 *    Date Created: 10/22/24
 *    Description: Manager for turn based movement mechanics.
 *******************************************************************/

using System;
using System.Collections.Generic;
using FMODUnity;
using PrimeTween;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    private Vector2 _lastRegistered = Vector2.zero;
    private bool _inputBuffered = false;
    private bool _autocompleteActive = false;
    private float _movementRegisteredTime = -1;
    private Vector2 _registeredInput = Vector2.zero;

    [SerializeField] private float _inputBufferWindow = 0.5f;

    // This second buffer window helps prevent double movements in scenes with no enemies
    [SerializeField] private float _noEnemiesBufferWindow = 0.25f;

    private bool _isListeningForMoveEnd = false;

    [Header("Autocomplete Mechanic")] [SerializeField, Tooltip("Timescale during autocomplete dash")]
    private float _autocompleteSpeed = 3;

    [SerializeField] private float _autocompleteWindow = 0.2f;
    [SerializeField] private Image _speedUI;

    public event Action<bool> AutocompleteToggled;
    private bool _sprintHeld = false;

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

        PrimeTweenConfig.warnTweenOnDisabledTarget = false;
        _playerControls = new PlayerControls();

        for (int i = 0; i <= (int) TurnState.None; i++)
        {
            _turnListeners.Add((TurnState) i, new List<ITurnListener>());
            _completedTurnCounts.Add((TurnState) i, 0);
        }
    }

    /// <summary>
    /// Enables the player controls and hooks a callback for movement input.
    /// </summary>
    private void OnEnable()
    {
        _playerControls.Enable();
        _playerControls.InGame.MoveUp.performed += ctx => _lastRegistered = new Vector2(0, 1);
        _playerControls.InGame.MoveUp.performed += RegisterMovementInput;
        _playerControls.InGame.MoveDown.performed += ctx => _lastRegistered = new Vector2(0, -1);
        _playerControls.InGame.MoveDown.performed += RegisterMovementInput;
        _playerControls.InGame.MoveRight.performed += ctx => _lastRegistered = new Vector2(1, 0);
        _playerControls.InGame.MoveRight.performed += RegisterMovementInput;
        _playerControls.InGame.MoveLeft.performed += ctx => _lastRegistered = new Vector2(-1, 0);
        _playerControls.InGame.MoveLeft.performed += RegisterMovementInput;
        _playerControls.InGame.GameSpeed.performed += SprintPerformed;
        _playerControls.InGame.GameSpeed.started += SprintPerformed;
        _playerControls.InGame.GameSpeed.canceled += SprintPerformed;
    }

    /// <summary>
    /// Disables the player controls and un-hooks a callback for movement input.
    /// </summary>
    private void OnDisable()
    {
        _playerControls.InGame.MoveUp.performed -= ctx => _registeredInput = new Vector2(0, 1);
        _playerControls.InGame.MoveUp.performed -= RegisterMovementInput;
        _playerControls.InGame.MoveDown.performed -= ctx => _registeredInput = new Vector2(0, -1);
        _playerControls.InGame.MoveDown.performed -= RegisterMovementInput;
        _playerControls.InGame.MoveRight.performed -= ctx => _registeredInput = new Vector2(1, 0);
        _playerControls.InGame.MoveRight.performed -= RegisterMovementInput;
        _playerControls.InGame.MoveLeft.performed -= ctx => _registeredInput = new Vector2(-1, 0);
        _playerControls.InGame.MoveLeft.performed -= RegisterMovementInput;
        _playerControls.InGame.GameSpeed.performed -= SprintPerformed;
        _playerControls.InGame.GameSpeed.started -= SprintPerformed;
        _playerControls.InGame.GameSpeed.canceled -= SprintPerformed;
        _playerControls.Disable();

        if (_isListeningForMoveEnd)
        {
            PlayerMovement.Instance.OnPlayerMoveComplete -= CheckForBufferedInput;
        }
    }

    /// <summary>
    /// Allows a direction input to be held when it comes to movement
    /// </summary>
    private void Update()
    {
        // Not being called unless movement is blocked
        if (_playerControls.InGame.Movement.IsPressed() && !TurnInProgress)
        {
            if (PlayerMovement.Instance.CanMove && Time.timeScale == 1)
            {
                PerformMovement();
            }
        }
    }

    /// <summary>
    /// Called when the turn state returns to None to check if the player buffered an input
    /// </summary>
    private void CheckForBufferedInput()
    {
        if (Time.unscaledTime - _movementRegisteredTime <=
            (EnemiesPresent ? _inputBufferWindow : _noEnemiesBufferWindow))
        {
            if (_isListeningForMoveEnd)
            {
                PlayerMovement.Instance.OnPlayerMoveComplete -= CheckForBufferedInput;
                _isListeningForMoveEnd = false;
            }

            PerformMovement();
        }
    }

    /// <summary>
    /// Callback for when sprint button is pressed.
    /// Stores sprint value instead of polling.
    /// </summary>
    /// <param name="ctx">The context of the button to evaluate.</param>
    private void SprintPerformed(InputAction.CallbackContext ctx)
    {
        _sprintHeld = ctx.ReadValueAsButton();
    }
    
    /// <summary>
    /// Invoked when a movement input is pressed.
    /// Will attempt to move if possible, but if it's not the player's turn
    /// the movement will be rejected.
    /// </summary>
    /// <param name="obj"></param>
    private void RegisterMovementInput(InputAction.CallbackContext obj)
    {
        if (SceneController.Instance.Transitioning)
        {
            return;
        }
        if (Time.timeScale > 1)
            return;

        var dir = GetNormalizedInput();
        //Don't use direction if zero
        if (dir == Vector3.zero)
           return;

        bool doubleTap = _lastMovementInput == dir &&
                         Time.unscaledTime - _movementRegisteredTime
                         <= _autocompleteWindow && _turnState != TurnState.None && !_autocompleteActive;
        if (EnemiesPresent && doubleTap)
        {
            EnableAutocomplete();
        }
        
        _lastMovementInput = dir;

        // If a movement is already registered, then flag as buffered
        if (_movementRegistered && !_autocompleteActive)
        {
            _inputBuffered = true;
        }

        _movementRegistered = true;
        _movementRegisteredTime = Time.unscaledTime;
        
        PerformMovement();
    }

    /// <summary>
    /// Helper method for performing movement. Aids in performing buffered inputs.
    /// </summary>
    private void PerformMovement()
    {
        // Return if no movement is registered or if the game is paused
        if (_turnState != TurnState.None ||
            (!_movementRegistered && !_inputBuffered) ||
            DebugMenuManager.Instance.PauseMenu)
        {
            return;
        }

        // Stop moving if no movement input was pressed
        if (!_playerControls.InGame.Movement.IsPressed() && !_inputBuffered)
        {
            _movementRegistered = false;
            return;
        }

        _turnState = TurnState.Player;
        
        if (EnemiesPresent && _sprintHeld)
        {
            EnableAutocomplete();
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

        TurnState listenerTurnState =
            listener.TurnState == _turnState ? listener.TurnState : listener.SecondaryTurnState;

        //check if all entities in this turn state have completed their turn.
        _completedTurnCounts[listenerTurnState]++;
        if (_completedTurnCounts[listenerTurnState] < _turnListeners[listenerTurnState].Count) return;
        _completedTurnCounts[listenerTurnState] = 0;

        // Clears buffered input once player has successfully moved
        if (_turnState == TurnState.Player)
        {
            _inputBuffered = false;
           // DisableAutocomplete();
        }

        //find out who's turn is next, if it's nobody's, stop.
        var next = GetNextTurn(_turnState);
        if (next is null or TurnState.None)
        {
            _turnState = TurnState.None;
            DisableAutocomplete();
            CheckForBufferedInput();
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

        if (_turnState != TurnState.None && SceneController.Instance != null &&
            !SceneController.Instance.Transitioning)
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
        if (!_playerControls.InGame.Movement.IsPressed() && !_inputBuffered)
        {
            _movementRegistered = false;
        }

        // Returns if it's not the listener's turn
        if (listener.TurnState != _turnState &&
            (listener.SecondaryTurnState != _turnState ||
             listener.SecondaryTurnState == TurnState.None))
        {
            return;
        }

        DisableAutocomplete();

       
        TurnState listenerTurnState =
            listener.TurnState == _turnState ? listener.TurnState : listener.SecondaryTurnState;

        _completedTurnCounts[listenerTurnState] = 0;
        var prev = GetPreviousTurn(listenerTurnState);
        if (prev is null or TurnState.None)
        {
            _turnState = TurnState.None;
            if (!_isListeningForMoveEnd)
            {
                _isListeningForMoveEnd = true;
                PlayerMovement.Instance.OnPlayerMoveComplete += CheckForBufferedInput;
            }

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

        if (!addedListener)
        {
            return;
        }

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
    /// Speeds up timescale for a short duration. Call DisableAutocomplete() to toggle off
    /// </summary>
    private void EnableAutocomplete()
    {
        _autocompleteActive = true;
        Time.timeScale = _autocompleteSpeed;
        AutocompleteToggled?.Invoke(true);
        Tween.Alpha(_speedUI, 1, 0.2f, Ease.OutSine).OnComplete(() => { _speedUI.gameObject.SetActive(true); });
    }

    /// <summary>
    /// Returns timescale to default
    /// </summary>
    private void DisableAutocomplete()
    {
        _autocompleteActive = false;
        Time.timeScale = 1;
        AutocompleteToggled?.Invoke(false);
        Tween.Alpha(_speedUI, 0, 0.4f, Ease.OutSine).OnComplete(() => { _speedUI.gameObject.SetActive(false); });
    }

    /// <summary>
    /// Fetches and normalizes the input of the player
    /// </summary>
    /// <returns>Normalized input vector</returns>
    private Vector3 GetNormalizedInput()
    {
        Vector2 input = _playerControls.InGame.Movement.ReadValue<Vector2>().normalized;
        //disable input if diagonal
        if (_playerControls.InGame.Movement.ReadValue<Vector2>().x != 0 &&
            _playerControls.InGame.Movement.ReadValue<Vector2>().y != 0){return new Vector3(0f, 0f, 0f);}
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            input.y = 0;
            input.x = input.x < 0 ? -1 : 1;
        }
        else
        {
            input.x = 0;
            input.y = input.y < 0 ? -1 : 1;
        }

        if (input != _lastRegistered)
        {
            input = _lastRegistered;
        }
        return new Vector3(input.x, 0f, input.y);
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