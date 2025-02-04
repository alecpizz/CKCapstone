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
using FMODUnity;
using SaintsField;

public class CutsceneFramework : MonoBehaviour
{
    // Checks whether to play the Challenge or End Chapter Cutscene
    [SerializeField] private bool _isChallengeCutscene;
    [SerializeField] private bool _isEndChapterCutscene;

    // Defines a list identifying the image(s) to play during the Challenge Cutscene
    [SerializeField] private List<GameObject> _challengeCutsceneImages;

    // Referenced https://www.youtube.com/watch?v=nt4qfbNAQqM (Used to implement
    // the functionality for playing a video, particularly for the End Chapter Cutscene)
    // Identifies the video to be played during the End Chapter Cutscene
    [SerializeField] private VideoPlayer _endChapterCutsceneVideo;

    // Referenced https://www.youtube.com/watch?v=nt4qfbNAQqM (Used to implement
    // the functionality for playing a video, particularly for the End Chapter Cutscene)
    // Identifies the duration of the cutscene, used to determine the time
    // before loading the next level
    [SerializeField] private float _cutsceneDuration;

    // Identifies the audio to be played during the cutscene
    [SerializeField] private EventReference _cutsceneAudio; 

    // Identifies the index of the level to be loaded after the cutscene plays
    [Scene]
    [SerializeField] private int _loadingLevelIndex = 0;

    [SerializeField] private float _audioVolumeOverride = 150f;
    /// <summary>
    /// Determines whether to play the Challenge or End Chapter Cutscene
    /// </summary>
    private void Start()
    {
        // Plays the Challenge Cutscene, provided that only the corresponding boolean
        // (_isChallengeCutscene) is true
        if (_isChallengeCutscene && !_isEndChapterCutscene)
        {
            PlayChallengeCutscene();
        }

        // Plays the End Chapter Cutscene, provided that only the corresponding boolean 
        // (_isEndChapterCutscene) is true
        if (_isEndChapterCutscene && !_isChallengeCutscene)
        {
            PlayEndChapterCutscene();
        }
    }
    
    /// <summary>
    /// Plays the Challenge Cutscene
    /// </summary>
    private void PlayChallengeCutscene()
    {
        // Displays the image(s) to be played during the Challenge Cutscene
        foreach (GameObject image in _challengeCutsceneImages)
        {
            image.SetActive(true);
        }

        // Plays the audio accompanying the Challenge Cutscene
        var instance = AudioManager.Instance.PlaySound(_cutsceneAudio);
        AudioManager.Instance.AdjustVolume(instance, _audioVolumeOverride);

        // Referenced https://www.youtube.com/watch?v=nt4qfbNAQqM (Used to implement the
        // functionality for playing a video, particularly for the End Chapter Cutscene)
        // Plays the Challenge Cutscene for a specified amount of time before loading the next level
        StartCoroutine(CutsceneDuration());
    }

    /// <summary>
    /// Plays the End Chapter Cutscene
    /// </summary>
    private void PlayEndChapterCutscene()
    {
        // Referenced https://www.youtube.com/watch?v=nt4qfbNAQqM (Used to implement the
        // functionality for playing a video, particularly for the End Chapter Cutscene)
        // Displays the video to be played during the End Chapter Cutscene
        _endChapterCutsceneVideo.Play();

        // Plays the audio accompanying the End Chapter Cutscene
        var instance = AudioManager.Instance.PlaySound(_cutsceneAudio);
        AudioManager.Instance.AdjustVolume(instance, _audioVolumeOverride);

        // Referenced https://www.youtube.com/watch?v=nt4qfbNAQqM (Used to implement the
        // functionality for playing a video, particularly for the End Chapter Cutscene)
        // Plays the End Chapter Cutscene for a specified amount of time before loading the next level
        StartCoroutine(CutsceneDuration());
    }

    /// <summary>
    /// Referenced https://www.youtube.com/watch?v=nt4qfbNAQqM (Used to implement the
    /// functionality for playing a video, particularly for the End Chapter Cutscene)
    /// The cutscene plays for a specified amount of time, before loading the next scene
    /// </summary>
    /// <returns></returns> Amount of time the cutscene should play
    private IEnumerator CutsceneDuration()
    {
        // Referenced https://www.youtube.com/watch?v=nt4qfbNAQqM (Used to implement the
        // functionality for playing a video, particularly for the End Chapter Cutscene)
        // Permits the cutscene to play for a specified amount of time
        yield return new WaitForSecondsRealtime(_cutsceneDuration);

        // Loads the next level, marked by a specified index
        SceneController.Instance.LoadNewScene(_loadingLevelIndex);
    }
}
