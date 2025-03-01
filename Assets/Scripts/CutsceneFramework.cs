/******************************************************************
*    Author: Madison Gorman
*    Contributors: Nick Grinstead, Alex Laubenstein
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
// Referenced https://www.youtube.com/watch?v=nt4qfbNAQqM (Used to implement
// the functionality for playing a video, particularly for the End Chapter Cutscene)
using UnityEngine.Experimental.Audio;
using UnityEngine.Experimental.Video;
using UnityEngine.Video;
using FMODUnity;
using SaintsField;
using System.Runtime.InteropServices;

using Unity.Collections;


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

    private DebugInputActions _inputActions;

    private const int LATENCY_MS = 50; /* Some devices will require higher latency to avoid glitches */
    private const int DRIFT_MS = 1;
    private const float DRIFT_CORRECTION_PERCENTAGE = 0.5f;

    private VideoPlayer _endChapterCutsceneVideo;
    private AudioSampleProvider mProvider;

    private FMOD.CREATESOUNDEXINFO mExinfo;
    private FMOD.Channel mChannel;
    private FMOD.Sound mSound;

    private List<float> mBuffer = new List<float>();

    private int mSampleRate;
    private uint mDriftThresholdSamples;
    private uint mTargetLatencySamples;
    private uint mAdjustedLatencySamples;
    private int mActualLatencySamples;

    private uint mTotalSamplesWritten;
    private uint mMinimumSamplesWritten = uint.MaxValue;
    private uint mTotalSamplesRead;

    private uint mLastReadPositionBytes;

    /// <summary>
    /// Determines whether to play the Challenge or End Chapter Cutscene
    /// </summary>
    private void Start()
    {
        _inputActions = new DebugInputActions();
        _inputActions.UI.Enable();
        _inputActions.UI.SkipCutscene.performed += ctx => SkipCutscene();
        //_endChapterCutsceneVideo.loopPointReached += CheckEnd;

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
            //PlayEndChapterCutscene();
            _endChapterCutsceneVideo = GetComponent<VideoPlayer>();
            if (_endChapterCutsceneVideo == null)
            {
                Debug.LogWarning("A VideoPlayer is required to use this script. " +
                    "See Unity Documentation on how to use VideoPlayer: " +
                    "https://docs.unity3d.com/Manual/class-VideoPlayer.html");
                return;
            }

            _endChapterCutsceneVideo.audioOutputMode = VideoAudioOutputMode.APIOnly;
            _endChapterCutsceneVideo.prepareCompleted += Prepared;
            _endChapterCutsceneVideo.loopPointReached += VideoEnded;
            _endChapterCutsceneVideo.Prepare();

#if UNITY_EDITOR
            EditorApplication.pauseStateChanged += EditorStateChange;
#endif
        }
    }

    /// <summary>
    /// Unregister input actions
    /// </summary>
    private void OnDisable()
    {
        _inputActions.UI.Disable();
        _inputActions.UI.SkipCutscene.performed -= ctx => SkipCutscene();
    }

    /// <summary>
    /// Used to skip the cutscene when an input is given
    /// </summary>
    private void SkipCutscene()
    {
        StopAllCoroutines();
        SceneController.Instance.LoadNewScene(_loadingLevelIndex);
    }

    private void CheckEnd(UnityEngine.Video.VideoPlayer vp)
    {
        SceneController.Instance.LoadNewScene(_loadingLevelIndex);
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
        //StartCoroutine(CutsceneDuration());
    }

#if UNITY_EDITOR
    private void EditorStateChange(PauseState state)
    {
        if (mChannel.hasHandle())
        {
            mChannel.setPaused(state == PauseState.Paused);
        }
    }
#endif

    private void OnDestroy()
    {
        mChannel.stop();
        mSound.release();

#if UNITY_EDITOR
        EditorApplication.pauseStateChanged -= EditorStateChange;
#endif
    }

    private void VideoEnded(VideoPlayer vp)
    {
        if (!vp.isLooping)
        {
            mChannel.setPaused(true);
        }
    }

    private void Prepared(VideoPlayer vp)
    {
        mProvider = vp.GetAudioSampleProvider(0);
        mSampleRate = (int)(mProvider.sampleRate * _endChapterCutsceneVideo.playbackSpeed);

        mDriftThresholdSamples = (uint)(mSampleRate * DRIFT_MS) / 1000;
        mTargetLatencySamples = (uint)(mSampleRate * LATENCY_MS) / 1000;
        mAdjustedLatencySamples = mTargetLatencySamples;
        mActualLatencySamples = (int)mTargetLatencySamples;

        mExinfo.cbsize = Marshal.SizeOf(typeof(FMOD.CREATESOUNDEXINFO));
        mExinfo.numchannels = mProvider.channelCount;
        mExinfo.defaultfrequency = mSampleRate;
        mExinfo.length = mTargetLatencySamples * (uint)mExinfo.numchannels * sizeof(float);
        mExinfo.format = FMOD.SOUND_FORMAT.PCMFLOAT;

        FMODUnity.RuntimeManager.CoreSystem.createSound("", FMOD.MODE.LOOP_NORMAL | FMOD.MODE.OPENUSER, ref mExinfo, out mSound);

        mProvider.sampleFramesAvailable += SampleFramesAvailable;
        mProvider.enableSampleFramesAvailableEvents = true;
        mProvider.freeSampleFrameCountLowThreshold = mProvider.maxSampleFrameCount - mTargetLatencySamples;

        vp.Play();
    }

    private void SampleFramesAvailable(AudioSampleProvider provider, uint sampleFrameCount)
    {
        using (NativeArray<float> buffer = new NativeArray<float>((int)sampleFrameCount * provider.channelCount, Allocator.Temp))
        {
            uint samplesWritten = provider.ConsumeSampleFrames(buffer);
            mBuffer.AddRange(buffer);

            /*
             * Drift compensation
             * If we are behind our latency target, play a little faster
             * If we are ahead of our latency target, play a little slower
             */
            mTotalSamplesWritten += samplesWritten;

            if (samplesWritten != 0 && (samplesWritten < mMinimumSamplesWritten))
            {
                mMinimumSamplesWritten = samplesWritten;
                mAdjustedLatencySamples = Math.Max(samplesWritten, mTargetLatencySamples);
            }

            int latency = (int)mTotalSamplesWritten - (int)mTotalSamplesRead;
            mActualLatencySamples = (int)((0.93f * mActualLatencySamples) + (0.03f * latency));

            int playbackRate = mSampleRate;
            if (mActualLatencySamples < (int)(mAdjustedLatencySamples - mDriftThresholdSamples))
            {
                playbackRate = mSampleRate - (int)(mSampleRate * (DRIFT_CORRECTION_PERCENTAGE / 100.0f));
            }
            else if (mActualLatencySamples > (int)(mAdjustedLatencySamples + mDriftThresholdSamples))
            {
                playbackRate = mSampleRate + (int)(mSampleRate * (DRIFT_CORRECTION_PERCENTAGE / 100.0f));
            }
            mChannel.setFrequency(playbackRate);
        }
    }

    private void Update()
    {
        if (!_endChapterCutsceneVideo.isPrepared)
        {
            return;
        }

        /*
         * Need to wait before playing to provide adequate space between read and write positions
         */
        if (!mChannel.hasHandle() && mTotalSamplesWritten > mAdjustedLatencySamples)
        {
            FMOD.ChannelGroup mMasterChannelGroup;
            FMODUnity.RuntimeManager.CoreSystem.getMasterChannelGroup(out mMasterChannelGroup);
            FMODUnity.RuntimeManager.CoreSystem.playSound(mSound, mMasterChannelGroup, false, out mChannel);
        }

        if (mBuffer.Count > 0 && mChannel.hasHandle())
        {
            uint readPositionBytes;
            mChannel.getPosition(out readPositionBytes, FMOD.TIMEUNIT.PCMBYTES);

            /*
             * Account for wrapping
             */
            uint bytesRead = readPositionBytes - mLastReadPositionBytes;
            if (readPositionBytes < mLastReadPositionBytes)
            {
                bytesRead += mExinfo.length;
            }

            if (bytesRead > 0 && mBuffer.Count >= bytesRead)
            {
                /*
                 * Fill previously read data with fresh samples
                 */
                IntPtr ptr1, ptr2;
                uint lenBytes1, lenBytes2;
                var res = mSound.@lock(mLastReadPositionBytes, bytesRead, out ptr1, out ptr2, out lenBytes1, out lenBytes2);
                if (res != FMOD.RESULT.OK) Debug.LogError(res);

                /*
                 * Though exinfo.format is float, data retrieved from Sound::lock is in bytes,
                 * therefore we only copy (len1+len2)/sizeof(float) full float values across
                 */
                int lenFloats1 = (int)(lenBytes1 / sizeof(float));
                int lenFloats2 = (int)(lenBytes2 / sizeof(float));
                int totalFloatsRead = lenFloats1 + lenFloats2;
                float[] tmpBufferFloats = new float[totalFloatsRead];

                mBuffer.CopyTo(0, tmpBufferFloats, 0, tmpBufferFloats.Length);
                mBuffer.RemoveRange(0, tmpBufferFloats.Length);

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
                mLastReadPositionBytes = readPositionBytes;
                mTotalSamplesRead += (uint)(totalFloatsRead / mExinfo.numchannels);
            }
        }
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
        //StartCoroutine(CutsceneDuration());
    }

    /// <summary>
    /// Referenced https://www.youtube.com/watch?v=nt4qfbNAQqM (Used to implement the
    /// functionality for playing a video, particularly for the End Chapter Cutscene)
    /// The cutscene plays for a specified amount of time, before loading the next scene
    /// </summary>
    /// <returns></returns> Amount of time the cutscene should play
    /*private IEnumerator CutsceneDuration()
    {
        // Referenced https://www.youtube.com/watch?v=nt4qfbNAQqM (Used to implement the
        // functionality for playing a video, particularly for the End Chapter Cutscene)
        // Permits the cutscene to play for a specified amount of time
        yield return new WaitForSecondsRealtime(_cutsceneDuration);

        // Loads the next level, marked by a specified index
        SceneController.Instance.LoadNewScene(_loadingLevelIndex);
    }*/
}
