/******************************************************************
*    Author: Mitchell Young
*    Contributors: 
*    Date Created: 3/25/25
*    Description: Loads cutscenes based on button pressed. Button
*    cannot be pressed if the cutscene has not been seen before.
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CutsceneCollection : MonoBehaviour
{
    [SerializeField] private bool unlocked = false;
    [SerializeField] private int cutsceneNumber = 0;

    private bool loadLastScene = false;

    private void Start()
    {
        string cutsceneName = SceneUtility.GetScenePathByBuildIndex(cutsceneNumber);
        if (SaveDataManager.GetLevelCompleted(cutsceneName))
        {
            unlocked = true;
        }
        if (unlocked)
        {
            gameObject.GetComponent<Image>().color = new Color(1, 1, 1, 1);
        }
        else
        {
            gameObject.GetComponent<Image>().color = new Color(0, 0, 0, 1);
        }
    }

    /// <summary>
    /// Loads the cutscene based on build index value assigned to the button
    /// </summary>
    public void LoadCutscene()
    {
        if (!unlocked) 
        {
            return;
        }
        SceneController.Instance.LoadNewScene(cutsceneNumber);
        loadLastScene = true;
    }

}
