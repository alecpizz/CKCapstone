using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndMenuController : MonoBehaviour
{
    private void Start()
    {
        AudioManager.Instance.PlayMusic("End");
        AudioManager.Instance.StopMusic("Player");
    }

    public void ReplayGame()
    {
        SaveSceneData.Instance.ResetData();

        SceneTransitions.Instance.LoadSceneWithTransition(SceneTransitions.TransitionType.Fade, 0);
    }

    public void ExitGame()
    {
        AudioManager.Instance.StopMusic("End");
        Application.Quit();
    }
}
