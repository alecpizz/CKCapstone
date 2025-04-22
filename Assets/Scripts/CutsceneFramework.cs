/******************************************************************
*    Author: Madison Gorman
*    Contributors: Nick Grinstead, Alex Laubenstein, Josephine Qualls
*    Date Created: 11/07/24
*    Description: Permits one of two cutscene types to play; either after 
*    the completion of a challenge level (comprised of a static image, 
*    subtitles, and audio) and chapter (a video)
*    References: https://www.youtube.com/watch?v=nt4qfbNAQqM (Used 
*    to implement the functionality for playing a video, particularly 
*    for the End Chapter Cutscene)
*    Uses code from FMOD Documentaion
*    https://www.fmod.com/docs/2.02/unity/examples-video-playback.html
*******************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
// Referenced the linked FMOD documentation to help make video cutscenes function
using UnityEngine.Experimental.Audio;
using UnityEngine.Experimental.Video;
using UnityEngine.Video;
using FMODUnity;
using SaintsField;
using System.Runtime.InteropServices;
using FMOD;
using Unity.Collections;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.Controls;
using Debug = UnityEngine.Debug;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class CutsceneFramework : MonoBehaviour
{
    // Checks whether to play the Challenge or End Chapter Cutscene
    [SerializeField] private bool _isChallengeCutscene;
    [SerializeField] private bool _isEndChapterCutscene;

    // Defines a list identifying the image(s) to play during the Challenge Cutscene
    [SerializeField] private List<GameObject> _challengeCutsceneImages;

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
    
    [Tooltip("For endcutscene audio only")]
    [Range(0, 10)]
    [SerializeField] private float _endCutsceneVolumeOverride = 5f;

    private DebugInputActions _inputActions;

    //variables referenced from FMOD documentation for help with video plpayback
    private const int LATENCY_MS = 200; /* Some devices will require higher latency to avoid glitches */
    private const int DRIFT_MS = 1;
    private const float DRIFT_CORRECTION_PERCENTAGE = 0.6f;

    private VideoPlayer _endChapterCutsceneVideo;
    private AudioSampleProvider _mProvider;

    private FMOD.CREATESOUNDEXINFO _mExinfo;
    private FMOD.Channel _mChannel;
    private FMOD.Sound mSound;

    private List<float> _mBuffer = new List<float>();

    private int _mSampleRate;
    private uint _mDriftThresholdSamples;
    private uint _mTargetLatencySamples;
    private uint _mAdjustedLatencySamples;
    private int _mActualLatencySamples;

    private uint _mTotalSamplesWritten;
    private uint _mMinimumSamplesWritten = uint.MaxValue;
    private uint _mTotalSamplesRead;

    private uint _mLastReadPositionBytes;

    private IDisposable _mAnyButtonPressedListener;

    [SerializeField] private MenuManager _menuManager;

    private float _timer = 0f;
    [SerializeField] float _skipHoldTime = 2f;
    
    /// <summary>
    /// Determines whether to play the Challenge or End Chapter Cutscene
    /// </summary>
    private void Start()
    {
        SaveDataManager.SetLevelCompleted(SceneManager.GetActiveScene().name);
        SaveDataManager.SetLastFinishedLevel(SceneManager.GetActiveScene().name);
        _inputActions = new DebugInputActions();
        _inputActions.UI.Enable();
        _inputActions.UI.SkipCutscene.performed += ctx => SkipCutscene();
        _inputActions.UI.Pause.performed += ctx => ResumePlayAudio(_menuManager.GetPauseInvoked());

        //_endChapterCutsceneVideo.loopPointReached += CheckEnd;
        //Registers is button is pressed
        _mAnyButtonPressedListener = InputSystem.onAnyButtonPress.Call(ButtonIsPressed);

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
            //checks for a video player on the game object if the cutscene is supposed to be a end chapter cutscene
            _endChapterCutsceneVideo = GetComponent<VideoPlayer>();
            if (_endChapterCutsceneVideo == null)
            {
                Debug.LogWarning("A VideoPlayer is required to use this script. " +
                    "See Unity Documentation on how to use VideoPlayer: " +
                    "https://docs.unity3d.com/Manual/class-VideoPlayer.html");
                return;
            }
            
            //Sets up the video to play it in the scene
            _endChapterCutsceneVideo.audioOutputMode = VideoAudioOutputMode.APIOnly;
            _endChapterCutsceneVideo.prepareCompleted += Prepared;
            _endChapterCutsceneVideo.loopPointReached += VideoEnded;
            _endChapterCutsceneVideo.Prepare();
            
            #if UNITY_EDITOR
            EditorApplication.pauseStateChanged += EditorStateChange;
#endif
        }

        var cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = Color.black;
            cam.clearFlags = CameraClearFlags.SolidColor;
        }
    }
    
    /// <summary>
    /// Unregister input actions
    /// </summary>
    private void OnDisable()
    {
        _inputActions.UI.Disable();
        _inputActions.UI.SkipCutscene.performed -= ctx => SkipCutscene();
        _inputActions.UI.Pause.performed -= ctx => ResumePlayAudio(_menuManager.GetPauseInvoked());

        if(_mAnyButtonPressedListener != null)
        {
            _mAnyButtonPressedListener.Dispose();
            _mAnyButtonPressedListener = null;
        }
    }
    
    /// <summary>
    /// Pauses and plays the audio and video of a cutscene
    /// </summary>
    /// <param name="play"></param>
    public void ResumePlayAudio(bool play)
    {
        if (play && _endChapterCutsceneVideo != null)
        {
            _endChapterCutsceneVideo.Pause();
        }
        else if (_endChapterCutsceneVideo != null)
        {
            _endChapterCutsceneVideo.Play();
        }
        
        _mChannel.setPaused(play);                                       
    }

    /// <summary>
    /// Used to skip the cutscene when an input is given
    /// </summary>
    public void SkipCutscene()
    {
        if (_timer > _skipHoldTime || _menuManager.GetSkipInPause())
        {
            StopAllCoroutines();
            string scenePath = SceneUtility.GetScenePathByBuildIndex(_loadingLevelIndex);
            SaveDataManager.SetLastFinishedLevel(scenePath);
            if (SaveDataManager.GetLoadedFromPause())
            {
                SaveDataManager.SetLoadedFromPause(false);
                SceneManager.LoadScene(SaveDataManager.GetSceneLoadedFrom());
                return;
            }
            SceneController.Instance.LoadNewScene(_loadingLevelIndex);

            if(SceneManager.GetActiveScene().name.Equals("CSCN_Act5_End"))
            {
                SaveDataManager.SetLastFinishedLevel(SceneManager.GetActiveScene().name);
            }
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
    /// Does an action any time a button is pressed within a Cutscene
    /// </summary>
    /// <param name="button"></param>
    private void ButtonIsPressed(InputControl button)
    {
        var scene = SceneManager.GetActiveScene().name;

        if (scene.Substring(0, 2).Equals("CS"))
        {
            Debug.Log("Hold the Space bar to skip!");
        }
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
        yield return new WaitForSeconds(_cutsceneDuration);

        // Loads the next level, marked by a specified index
        if (SaveDataManager.GetLoadedFromPause())
        {
            SaveDataManager.SetLoadedFromPause(false);
            SceneManager.LoadScene(SaveDataManager.GetSceneLoadedFrom());
            yield break;
        }
        SceneController.Instance.LoadNewScene(_loadingLevelIndex);
    }

    /// <summary>
    /// All code from this point on references the FMOD documentation linked at the top of the script
    /// makes sure that the video pauses when the editor is paused
    /// </summary>
    /// <param name="state"></param>
#if UNITY_EDITOR
    private void EditorStateChange(PauseState state)
    {
        if (_mChannel.hasHandle())
        {
            _mChannel.setPaused(state == PauseState.Paused);
        }
    }
    #endif

    /// <summary>
    /// If the video player is destroyed stop trying to play the video
    /// </summary>
    private void OnDestroy()
    {
        _mChannel.stop();
        mSound.release();

        #if UNITY_EDITOR
        EditorApplication.pauseStateChanged -= EditorStateChange;
        #endif
    }

    /// <summary>
    /// Checks for the end of a video file and brings you to the next level
    /// </summary>
    /// <param name="vp"></param>
    private void VideoEnded(VideoPlayer vp)
    {
        if (SaveDataManager.GetLoadedFromPause())
        {
            SaveDataManager.SetLoadedFromPause(false);
            SceneManager.LoadScene(SaveDataManager.GetSceneLoadedFrom());
            return;
        }
        SceneController.Instance.LoadNewScene(_loadingLevelIndex);
        //if video isn't looping pause the video
        if (!vp.isLooping)
        {
            _mChannel.setPaused(true);
        }
    }

    /// <summary>
    /// Sets up the video for playback based off of all of the set variables at the start of the script
    /// </summary>
    /// <param name="vp"></param>
    private void Prepared(VideoPlayer vp)
    {
        _mProvider = vp.GetAudioSampleProvider(0);
        _mSampleRate = (int)(_mProvider.sampleRate * _endChapterCutsceneVideo.playbackSpeed);

        _mDriftThresholdSamples = (uint)(_mSampleRate * DRIFT_MS) / 1000;
        _mTargetLatencySamples = (uint)(_mSampleRate * LATENCY_MS) / 1000;
        _mAdjustedLatencySamples = _mTargetLatencySamples;
        _mActualLatencySamples = (int)_mTargetLatencySamples;

        _mExinfo.cbsize = Marshal.SizeOf(typeof(FMOD.CREATESOUNDEXINFO));
        _mExinfo.numchannels = _mProvider.channelCount;
        _mExinfo.defaultfrequency = _mSampleRate;
        _mExinfo.length = _mTargetLatencySamples * (uint)_mExinfo.numchannels * sizeof(float);
        _mExinfo.format = FMOD.SOUND_FORMAT.PCMFLOAT;
        
        FMODUnity.RuntimeManager.CoreSystem.createSound("", FMOD.MODE.LOOP_NORMAL | FMOD.MODE.OPENUSER, ref _mExinfo, out mSound);
        _mProvider.sampleFramesAvailable += SampleFramesAvailable;
        _mProvider.enableSampleFramesAvailableEvents = true;
        _mProvider.freeSampleFrameCountLowThreshold = _mProvider.maxSampleFrameCount - _mTargetLatencySamples;
        
        vp.Play();
    }

    /// <summary>
    /// Samples available frames to compensate for lag and speed up
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="sampleFrameCount"></param>
    private void SampleFramesAvailable(AudioSampleProvider provider, uint sampleFrameCount)
    {
        using (NativeArray<float> buffer = new NativeArray<float>((int)sampleFrameCount * provider.channelCount, Allocator.Temp))
        {
            uint samplesWritten = provider.ConsumeSampleFrames(buffer);
            _mBuffer.AddRange(buffer);
            
            // Drift compensation
            // If we are behind our latency target, play a little faster
            // If we are ahead of our latency target, play a little slower            
            _mTotalSamplesWritten += samplesWritten;

            if (samplesWritten != 0 && (samplesWritten < _mMinimumSamplesWritten))
            {
                _mMinimumSamplesWritten = samplesWritten;
                _mAdjustedLatencySamples = Math.Max(samplesWritten, _mTargetLatencySamples);
            }

            int latency = (int)_mTotalSamplesWritten - (int)_mTotalSamplesRead;
            _mActualLatencySamples = (int)((0.93f * _mActualLatencySamples) + (0.03f * latency));

            int playbackRate = _mSampleRate;
            if (_mActualLatencySamples < (int)(_mAdjustedLatencySamples - _mDriftThresholdSamples))
            {
                playbackRate = _mSampleRate - (int)(_mSampleRate * (DRIFT_CORRECTION_PERCENTAGE / 100.0f));
            }
            else if (_mActualLatencySamples > (int)(_mAdjustedLatencySamples + _mDriftThresholdSamples))
            {
                playbackRate = _mSampleRate + (int)(_mSampleRate * (DRIFT_CORRECTION_PERCENTAGE / 100.0f));
            }
            _mChannel.setFrequency(playbackRate);
            _mChannel.setVolume(_endCutsceneVolumeOverride);
        }
    }

    /// <summary>
    /// If there is an End Chapter Cutscene being played make sure the video is being played smoothly
    /// </summary>
    private void Update()
    {
        //Skips cutscene after 2 secs of holding Space bar
        if (_inputActions.UI.SkipCutscene.IsPressed())
        {
            _timer += Time.deltaTime;
            
            if(_timer > _skipHoldTime)
            {
                SkipCutscene();
            }
        }
        else
        {
            _timer = 0f;
        }
        

        if (_isEndChapterCutscene)
        {
            //Checks for an availale cutscene
            if (!_endChapterCutsceneVideo.isPrepared)
            {
                return;
            }

            // Need to wait before playing to provide adequate space between read and write positions   
            if (!_mChannel.hasHandle() && _mTotalSamplesWritten > _mAdjustedLatencySamples)
            {
                FMOD.ChannelGroup mMasterChannelGroup;
                FMODUnity.RuntimeManager.CoreSystem.getMasterChannelGroup(out mMasterChannelGroup);
                FMODUnity.RuntimeManager.CoreSystem.playSound(mSound, mMasterChannelGroup, false, out _mChannel);
            }

            if (_mBuffer.Count > 0 && _mChannel.hasHandle())
            {
                uint readPositionBytes;
                _mChannel.getPosition(out readPositionBytes, FMOD.TIMEUNIT.PCMBYTES);


                //Account for wrapping             
                uint bytesRead = readPositionBytes - _mLastReadPositionBytes;
                if (readPositionBytes < _mLastReadPositionBytes)
                {
                    bytesRead += _mExinfo.length;
                }

                if (bytesRead > 0 && _mBuffer.Count >= bytesRead)
                {
                    // Fill previously read data with fresh samples                
                    IntPtr ptr1, ptr2;
                    uint lenBytes1, lenBytes2;
                    var res = mSound.@lock(_mLastReadPositionBytes, bytesRead, out ptr1, out ptr2, out lenBytes1, out lenBytes2);
                    if (res != FMOD.RESULT.OK) Debug.LogError(res);

                    // Though exinfo.format is float, data retrieved from Sound::lock is in bytes,
                    // therefore we only copy (len1+len2)/sizeof(float) full float values across                
                    int lenFloats1 = (int)(lenBytes1 / sizeof(float));
                    int lenFloats2 = (int)(lenBytes2 / sizeof(float));
                    int totalFloatsRead = lenFloats1 + lenFloats2;
                    float[] tmpBufferFloats = new float[totalFloatsRead];

                    _mBuffer.CopyTo(0, tmpBufferFloats, 0, tmpBufferFloats.Length);
                    _mBuffer.RemoveRange(0, tmpBufferFloats.Length);

                    if (lenBytes1 > 0)
                    {
                        Marshal.Copy(tmpBufferFloats, 0, ptr1, lenFloats1);
                    }
                    if (lenBytes2 > 0)
                    {
                        Marshal.Copy(tmpBufferFloats, lenFloats1, ptr2, lenFloats2);
                    }

                    res = mSound.unlock(ptr1, ptr2, lenBytes1, lenBytes2);
                    if (res != FMOD.RESULT.OK) Debug.LogError(res);
                    _mLastReadPositionBytes = readPositionBytes;
                    _mTotalSamplesRead += (uint)(totalFloatsRead / _mExinfo.numchannels);
                }
            }
        }
    }
}
