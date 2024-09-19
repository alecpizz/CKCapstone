using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: Trinity
/// Description: Controls the indicator for the endgame lock in the respecetive level once it is completed
/// </summary>
public class UnlockIndicator : MonoBehaviour
{
    [Header("Assignments")]
    [SerializeField]
    Renderer indicatorRenderer;

    [Header("Transition")]
    [SerializeField]
    bool doTeleportPlayer = false;

    [SerializeField]
    int startRoomID;

    [SerializeField]
    float teleportDelay = 2;

    [SerializeField]
    float timeSlow = 0.2f;

    [Header("Materials")]
    [SerializeField]
    Material baseMaterial;

    [SerializeField]
    Material completedMaterial;

    private void OnEnable()
    {
        indicatorRenderer.material = baseMaterial;
        GameplayManagers.Instance.Room.RoomVictoryEvent += OnRoomVictory;
    }

    private void OnDisable()
    {
        GameplayManagers.Instance.Room.RoomVictoryEvent -= OnRoomVictory;
    }

    private void OnRoomVictory()
    {
        indicatorRenderer.material = completedMaterial;

        if(doTeleportPlayer)
            StartCoroutine(DelayedTeleportPlayer());
    }

    IEnumerator DelayedTeleportPlayer()
    {
        Time.timeScale = timeSlow;
        yield return new WaitForSecondsRealtime(teleportDelay);
        SceneTransitions.Instance.LoadSceneWithTransition(SceneTransitions.TransitionType.Fade, startRoomID);
        Time.timeScale = 1f;
    }
}
