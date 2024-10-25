using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum TurnState
{
    Player = 0,
    World = 1,
    None = 2,
}

public interface ITurnListener
{
    TurnState TurnState { get; }
    bool TurnComplete { get; set; }
    void PerformTurn(Vector3 direction);
}


[DefaultExecutionOrder(-5000)]
public sealed class RoundManager : MonoBehaviour
{
    [SerializeField] private TurnState turnState = TurnState.None;
    public static RoundManager Instance { get; private set; }
    private Dictionary<TurnState, List<ITurnListener>> _turnListeners;
    private readonly Dictionary<ITurnListener, bool> _listenerStates = new();
    private PlayerControls _playerControls;
    private Vector3 _lastMovementInput;

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
        _turnListeners =
            new Dictionary<TurnState, List<ITurnListener>>
            {
                { TurnState.Player, new List<ITurnListener>() },
                { TurnState.World, new List<ITurnListener>() },
                { TurnState.None, new List<ITurnListener>() }
            };
    }

    private void OnEnable()
    {
        _playerControls.Enable();
        _playerControls.InGame.Movement.performed += MovementOnperformed;
    }


    private void OnDisable()
    {
        _playerControls.InGame.Movement.performed -= MovementOnperformed;
        _playerControls.Disable();
    }

    private void MovementOnperformed(InputAction.CallbackContext obj)
    {
        if (turnState != TurnState.None) return;

        Vector2 input = _playerControls.InGame.Movement.ReadValue<Vector2>();
        Vector3 dir = new Vector3(input.x, 0f, input.y);
        _lastMovementInput = dir;

        //we now wait on the update method to catch the end of the players turn
        //perform the turn now so that it's frame perfect.
        foreach (var turnListener in _turnListeners[TurnState.Player])
        {
            if (_listenerStates[turnListener]) continue;
            turnListener.PerformTurn(_lastMovementInput);
            _listenerStates[turnListener] = true;
        }
        turnState = TurnState.Player;
    }

    private void Update()
    {
        if (turnState == TurnState.None) return;
        //unecessary turn start for the player, needed for anyone else
        foreach (var turnListener in _turnListeners[turnState]) 
        {
            if (_listenerStates[turnListener]) continue;
            turnListener.PerformTurn(_lastMovementInput);
            _listenerStates[turnListener] = true;
        }

        foreach (var turnListener in _turnListeners[turnState])
        {
            if (!turnListener.TurnComplete) return;
        }

        //this state has completed, reset the turn progress and move on to the next turn.
        foreach (var turnListener in _turnListeners[turnState])
        {
            _listenerStates[turnListener] = false;
            turnListener.TurnComplete = false;
        }
        turnState++;
    }
    

    public void RegisterListener(ITurnListener listener)
    {
        _turnListeners[listener.TurnState].Add(listener);
        _listenerStates.Add(listener, false);
        //we added something during mid turn!
        if (listener.TurnState != turnState) return;
        listener.PerformTurn(_lastMovementInput);
        _listenerStates[listener] = true;
    }

    public void UnRegisterListener(ITurnListener listener)
    {
        _turnListeners[listener.TurnState].Remove(listener);
        _listenerStates.Remove(listener);
    }
}