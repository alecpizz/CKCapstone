using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: Trinity
/// Coconspirator: Ryan
/// Description: Places where the player exits the level
/// </summary>
public class Exit : MonoBehaviour
{
    [SerializeField] int _sceneIDToLoad;
    SceneTransitions.TransitionType transitionType;
    private Vector2 _directionOfSceneToLoad;

    [SerializeField] GameObject _disabledVisual;
    [SerializeField] GameObject _enabledVisual;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        
    }

    public void UseExit()
    {
        //Debug.Log("<insert load next scene>");

        /*if (SaveSceneData.Instance.LevelCompleteCount() == 6)
        {
            SceneTransitions.Instance.LoadSceneWithTransition(transitionType, 7);
        }
        else
        {
            
        }*/
        SaveSceneData.Instance.SetLastSceneDirection(_directionOfSceneToLoad * -1);
        SceneTransitions.Instance.LoadSceneWithTransition(transitionType, _sceneIDToLoad);

        //GameplayManagers.Instance.Room.ResetRoom();
    }

    public void ExitActivationStatus(bool status)
    {
        GetComponent<Collider>().enabled = status;

        _enabledVisual.SetActive(status);
        _disabledVisual.SetActive(!status);
    }

    public void SetSceneIDToLoad(int newID)
    {
        _sceneIDToLoad = newID;
    }
    
    public void SetDirectionOfNextScene(Vector2 nextSceneDir)
    {
        _directionOfSceneToLoad = nextSceneDir;
    }

    public void GiveTransition(SceneTransitions.TransitionType transition)
    {
        transitionType = transition;
    }
}
