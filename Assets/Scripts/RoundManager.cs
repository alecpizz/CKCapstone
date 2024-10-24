using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


public enum TurnType
{
    None = 0,
    Player = 1,
    World = 2
}

public interface ITurnListener
{
    TurnType TurnType { get; }
    bool TurnComplete { get; set; }
    bool TurnStarted { get; set; }
    void PerformTurn(Vector3 direction);
    void Register();
    void UnRegister();
}

public class TurnComparer : IComparer<TurnType>
{
    public int Compare(TurnType x, TurnType y) => x - y;
}

[DefaultExecutionOrder(-5000)]
public class RoundManager : MonoBehaviour
{
    [SerializeField] [ReadOnly] private TurnType turnType = TurnType.Player;
    public static RoundManager Instance { get; private set; }
    private Dictionary<TurnType, List<ITurnListener>> _turnListeners;
    private int _currentTurnIndex;
    public Action<Vector2Int> OnWorldUpdate;
    public Action<Vector2Int> OnPlayerUpdate;
    private PlayerControls _playerControls;

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
            new Dictionary<TurnType, List<ITurnListener>>
            {
                {TurnType.Player, new List<ITurnListener>()},
                {TurnType.World, new List<ITurnListener>()},
                {TurnType.None, new List<ITurnListener>()}
            };
    }

    private void OnEnable()
    {
        _playerControls.Enable();
        _playerControls.InGame.Movement.performed += MovePerformed;
    }

    private void MovePerformed(InputAction.CallbackContext obj)
    {
        Vector3 input = obj.ReadValue<Vector2>();
        Vector3 dir = new Vector3(input.x, 0, input.y);
        UpdateTurn(dir);
    }

    private void UpdateTurn(Vector3 direction)
    {
        foreach (var pair in _turnListeners)
        {
            if (pair.Key != (TurnType) _currentTurnIndex) continue;
            foreach (var turnListener in pair.Value)
            {
                if (!turnListener.TurnStarted)
                {
                    turnListener.TurnStarted = true;
                    turnListener.PerformTurn(direction);
                    continue;
                }

                if (!turnListener.TurnComplete)
                {
                    continue;
                }

                turnListener.TurnComplete = false;
                turnListener.TurnStarted = false;
                if (turnListener != pair.Value[^1]) continue;
                //done
                _currentTurnIndex = ++_currentTurnIndex % _turnListeners.Count;
                if ((TurnType) _currentTurnIndex != TurnType.World)
                {
                    UpdateTurn(direction);
                }
            }
        }
        // if (!_turnListeners[_currentTurnIndex].TurnStarted)
        // {
        //     _turnListeners[_currentTurnIndex].PerformTurn(direction);
        //     _turnListeners[_currentTurnIndex].TurnStarted = true;
        //     return;
        // }
        //
        // if (!_turnListeners[_currentTurnIndex].TurnComplete)
        // {
        //     return;
        // }
        //
        // _turnListeners[_currentTurnIndex].TurnStarted = false;
        // _turnListeners[_currentTurnIndex].TurnComplete = false;
        // _currentTurnIndex = ++_currentTurnIndex % _turnListeners.Count;
        // print(_currentTurnIndex);
        // UpdateTurn(direction);
    }

    private void OnDisable()
    {
        _playerControls.InGame.Movement.performed -= MovePerformed;
        _playerControls.Disable();
    }


    public bool IsPlayerTurn => turnType >= TurnType.Player;
    public bool IsWorldTurn => turnType == TurnType.World;

    public void RegisterListener(ITurnListener listener)
    {
        _turnListeners[listener.TurnType].Add(listener);
        _currentTurnIndex = (int)TurnType.Player;
    }

    public void UnRegisterListener(ITurnListener listener)
    {
        _turnListeners[listener.TurnType].Add(listener);
        _currentTurnIndex = (int)TurnType.Player;
    }
}