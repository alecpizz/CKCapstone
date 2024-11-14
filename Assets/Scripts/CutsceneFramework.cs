/******************************************************************
*    Author: Madison Gorman
*    Contributors: 
*    Date Created: 11/07/24
*    Description: Permits two types of cutscenes to play, after the 
*    completion of a challenge room (comprised of a static image, 
*    subtitles, and audio) and chapter (a video)
*    Referenced https://www.youtube.com/watch?v=nt4qfbNAQqM to 
*    implement the functionality surrounding playing a video for 
*    the End Chapter Cutscene
*******************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CutsceneFramework : MonoBehaviour
{
    [SerializeField] private bool _isChallengeCutscene;
    [SerializeField] private bool _isEndChapterCutscene;

    [SerializeField] private List<GameObject> _challengeCutsceneImages;

    //[SerializeField] private int _loadingSceneIndex = 0;

    private void Start()
    {
        PlayCutscene();
    }
    
    private void PlayCutscene()
    {
        if(_isChallengeCutscene == true && _isEndChapterCutscene == false)
        {
            foreach (GameObject image in _challengeCutsceneImages)
            {
                image.SetActive(true); 
            }
        }

        if (_isEndChapterCutscene == true && _isChallengeCutscene == false)
        {
            
        }

        //SceneController.Instance.LoadNewScene(_loadingSceneIndex);
    }
}
