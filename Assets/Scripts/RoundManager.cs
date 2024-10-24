using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
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
}

public class RoundManager : MonoBehaviour
{
    [SerializeField] [ReadOnly] private TurnType turnType = TurnType.Player;
    public static RoundManager Instance { get; private set; }
    private List<ITurnListener> _turnListeners = new();
    public Action<Vector2Int> OnWorldUpdate;
    public Action<Vector2Int> OnPlayerUpdate;
    private PlayerControls _playerControls = new PlayerControls();
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
    }

    private void OnEnable()
    {
        _playerControls.Enable();
    }

    private void OnDisable()
    {
        _playerControls.Disable();
    }

    public bool IsPlayerTurn => turnType >= TurnType.Player;
    public bool IsWorldTurn => turnType == TurnType.World;

    public void RegisterListener(ITurnListener listener)
    {
        _turnListeners.Add(listener);
    }

    public void UnRegisterListener(ITurnListener listener)
    {
        _turnListeners.Remove(listener);
    }
}
