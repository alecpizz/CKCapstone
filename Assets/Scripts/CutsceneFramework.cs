/******************************************************************
*    Author: Madison Gorman
*    Contributors: 
*    Date Created: 11/07/24
*    Description: Permits a cutscene (comprised of a static image, 
*    subtitles, and audio) to play after the completion
*    of a challenge room.
*******************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CutsceneFramework : MonoBehaviour
{
    // [SerializeField] private int _loadingSceneIndex = 0;

    [SerializeField] private List<GameObject> _cutsceneImages;

    private void Start()
    {
        PlayCutscene();
    }
    
    private void PlayCutscene()
    {
        foreach (GameObject image in _cutsceneImages)
        {
            Instantiate(image);
        }
    }
    // SceneController.Instance.LoadNewScene(_loadingSceneIndex);
}
