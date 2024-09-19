using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Author: Ryan
/// Editor(s): Trinity
/// Description: Switches the 'turns' from player and enemies
/// </summary>
/// 
public class TurnManager : MonoBehaviour
{

    [SerializeField] private TurnState _turnState = 0;

    private int turnstateAmt;

    public event Action EnvironmentTurnEvent;

    private void Awake()
    {
        turnstateAmt = Enum.GetNames(typeof(TurnState)).Length;
    }

    // Start is called before the first frame update
    void Start()
    {
        //Progress();
        StartTurnState(TurnState.Player);
    }

    public void Progress()
    {
        IncrementTurnState();
        StartTurnState(_turnState);
        
        //NextTurnState();

        GameplayManagers.Instance.Harmonizer.TriggerAllWaves();
    }

    public void ResetProgress()
    {
        _turnState = 0;
        Progress();
    }

    private void IncrementTurnState()
    {
        int newState = (int)_turnState + 1;
        if (newState >= turnstateAmt)
            newState = 0;

        _turnState = (TurnState) newState;
    }

    private void NextTurnState()
    {
        switch (_turnState)
        {
            case (TurnState.Player):
                StartTurnState(TurnState.Enemy);
                return;
            case (TurnState.Enemy):
                StartTurnState(TurnState.Player);
                return;
        }
        
    }

    #region PlayerTurn
    public void StartPlayerTurn()
    {
        GameplayManagers.Instance.Enemy.GetPlayer().SendHarmonyWaveInMoveDirection();
    }
    #endregion

    #region EnvironmentTurn

    public void StartEnvironmentTurn()
    {
        if (EnvironmentTurnEvent == null || EnvironmentTurnEvent.GetInvocationList().Length == 0)
        {
            Progress();
            return;
        }
            
        EnvironmentTurnEvent?.Invoke();
        StartCoroutine(DelayEvent());
        StartCoroutine(TurnProcessWaves(GameplayManagers.Instance.Enemy.GetTotalTimeToMove()));
    }

    #endregion

    #region EnemyTurn
    public void StartEnemyTurn()
    {
        MoveAllEnemies();
    }

    private void MoveAllEnemies()
    {
        GameplayManagers.Instance.Enemy.MoveAllEnemies();
        StartCoroutine(DelayEvent());
    }

    IEnumerator DelayEvent()
    {
        yield return new WaitForSeconds(GameplayManagers.Instance.Enemy.GetTotalTimeToMove());
        Progress();
    }

    IEnumerator TurnProcessWaves(float turnTime)
    {
        while(turnTime > 0)
        {
            turnTime -= Time.deltaTime;
            GameplayManagers.Instance.Harmonizer.GetVisualizeAllWavesEvent().Invoke();
            yield return null;
        }
    }

    #endregion

    void StartTurnState(TurnState newState)
    {
        if (GameplayManagers.Instance.Room.GetRoomSolved())
        {
            _turnState = TurnState.Player;
            return;
        }
            
        _turnState = newState;
        switch (newState)
        {
            case (TurnState.Player):
                StartPlayerTurn();
                break;
            case (TurnState.Environment):
                StartEnvironmentTurn();
                return;
            case (TurnState.Enemy):
                StartEnemyTurn();
                break;
        }
    }

    public TurnState GetTurnState()
    {
        return _turnState;
    }
}

public enum TurnState
{
    Player,
    Environment,
    Enemy
}
