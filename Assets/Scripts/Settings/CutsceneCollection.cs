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
    [SerializeField] private string cutsceneName = "";

    private int _levelIndexToLoad = 0;

    /// <summary>
    /// Sets unlocked to true if the cutscene has been seen
    /// </summary>
    private void Start()
    {
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

        _levelIndexToLoad = SceneManager.GetActiveScene().buildIndex;
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
        SaveDataManager.SetLoadedFromPause(true);
        string scenePath = SceneUtility.GetScenePathByBuildIndex(_levelIndexToLoad);
        SaveDataManager.SetSceneLoadedFrom(scenePath);
        SaveDataManager.SetLastFinishedLevel(scenePath);

        Debug.Log(SaveDataManager.GetLastFinishedLevel());
        Debug.Log(SaveDataManager.GetLoadedFromPause());
        Debug.Log(SaveDataManager.GetSceneLoadedFrom());

        //SceneManager.LoadScene(cutsceneName);
    }

}
