/******************************************************************
*    Author: Madison Gorman
*    Contributors: 
*    Date Created: 11/07/24
*    Description: Permits one of two cutscene types to play; either after 
*    the completion of a challenge level (comprised of a static image, 
*    subtitles, and audio) and chapter (a video)
*    References: https://www.youtube.com/watch?v=nt4qfbNAQqM (Used 
*    to implement the functionality for playing a video, particularly 
*    for the End Chapter Cutscene)
*******************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
// Referenced https://www.youtube.com/watch?v=nt4qfbNAQqM (Used to implement
// the functionality for playing a video, particularly for the End Chapter Cutscene)
using UnityEngine.Video; 

public class CutsceneFramework : MonoBehaviour
{
    [SerializeField] private bool _isChallengeCutscene;
    [SerializeField] private bool _isEndChapterCutscene;

    [SerializeField] private List<GameObject> _challengeCutsceneImages;

    // Referenced https://www.youtube.com/watch?v=nt4qfbNAQqM (Used to implement
    // the functionality for playing a video, particularly for the End Chapter Cutscene)
    [SerializeField] private VideoPlayer _endChapterCutsceneVideo;
    [SerializeField] private float _cutsceneDuration;

    [SerializeField] private int _loadingLevelIndex = 0;

    private void Start()
    {
        PlayCutscene();
    }
    
    private void PlayCutscene()
    {
        if (_isChallengeCutscene == true && _isEndChapterCutscene == false)
        {
            foreach (GameObject image in _challengeCutsceneImages)
            {
                image.SetActive(true); 
            }
        }

        if (_isEndChapterCutscene == true && _isChallengeCutscene == false)
        {
            // Referenced https://www.youtube.com/watch?v=nt4qfbNAQqM (Used to implement the
            // functionality for playing a video, particularly for the End Chapter Cutscene)
            _endChapterCutsceneVideo.Play();
        }

        // Referenced https://www.youtube.com/watch?v=nt4qfbNAQqM (Used to implement the
        // functionality for playing a video, particularly for the End Chapter Cutscene)
        StartCoroutine(CutsceneDuration());
    }

    // Referenced https://www.youtube.com/watch?v=nt4qfbNAQqM (Used to implement the
    // functionality for playing a video, particularly for the End Chapter Cutscene)
    private IEnumerator CutsceneDuration()
    {
        // Referenced https://www.youtube.com/watch?v=nt4qfbNAQqM (Used to implement the
        // functionality for playing a video, particularly for the End Chapter Cutscene)
        yield return new WaitForSeconds(_cutsceneDuration);

        // Referenced https://www.youtube.com/watch?v=nt4qfbNAQqM (Used to implement the
        // functionality for playing a video, particularly for the End Chapter Cutscene)
        _endChapterCutsceneVideo.Stop();

        foreach(GameObject image in _challengeCutsceneImages)
        {
            image.SetActive(false);
        }

        SceneController.Instance.LoadNewScene(_loadingLevelIndex);
    }
}
