/******************************************************************
*    Author: David Henvick
*    Contributors: 
*    Date Created: 10/30/2024
*    Description: this is the script that is used control the subtitling system for cutscenes
*******************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using FMOD.Studio;
using UnityEngine;
using TMPro;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class SubtitleManager : MonoBehaviour
{
    //code set up
    [SerializeField] private EventReference _dialogue;
    [SerializeField] private string _subtitleText;
    [SerializeField] private int _sentences;

    //visual set up
    [SerializeField] private TMP_Text _subtitleObject;

    //untested
    private EVENT_CALLBACK _callback;

    private string[] _subtitleArray;
    private static int _currentIndex;
    private EventInstance _currentDialogue;
    private bool _isPlaying;

    private char _currentCharacter;
    private string _currentString;



    /// <summary>
    /// called on scene begin. used to set variables, cut up the _subtitleText string into sentences, 
    /// and lastly start the dialogue audio.
    /// </summary>
    private void Start()
    {
        //setting up for string division;
        _subtitleArray = new string[_sentences];
        _currentCharacter = '\0';
        _currentIndex = 0;
        _currentString = "";



        for(int i = 0; i < _subtitleText.Length; i++)
        {
            _currentCharacter = _subtitleText[i];
            if (_currentCharacter == '.' || _currentCharacter == '!' || _currentCharacter == '?')
            {
                _currentString = _currentString + _currentCharacter;
                _subtitleArray[_currentIndex] = _currentString;
                _currentIndex++;
                _currentString = "";
            }
            else
            {
                _currentString = _currentString + _currentCharacter;
            }
        }
        _currentIndex = 0;

        //setting up audio detection (whether it is playing or not)
        _callback = new EVENT_CALLBACK(EventCallback);
        _currentDialogue.setCallback(_callback, EVENT_CALLBACK_TYPE.STOPPED);
        _currentDialogue.start();
    }


    /// <summary>
    /// untested
    /// </summary>
    /// <param name="type"></param>
    /// <param name="instance"></param>
    /// <param name="paramPtr"></param>
    /// <returns></returns>
    FMOD.RESULT EventCallback(EVENT_CALLBACK_TYPE type, IntPtr instance, IntPtr paramPtr)
    {
        if (type is EVENT_CALLBACK_TYPE.STOPPED)
        {
            FMOD.Studio.EventInstance eventInstance = new EventInstance(instance);
            NextSegment();
        }
        return result;
    }

    /// <summary>
    /// called on tics
    /// used to find if the current line of dialogue is finished
    /// </summary>
    private void Update()
    {
        if (!IsPlaying(_currentDialogue))
        {
            AudioManager.Instance.StopSound(_currentDialogue);
        }
    }

    /// <summary>
    /// this is used to see if the current audio is still playing
    /// </summary>
    /// <param name="instance"></param> instance of audio
    /// <returns></returns> whether sound is still playing
    private bool IsPlaying(FMOD.Studio.EventInstance instance)
    {
        FMOD.Studio.PLAYBACK_STATE state;
        instance.getPlaybackState(out state);
        return state != FMOD.Studio.PLAYBACK_STATE.STOPPED;
    }

    /// <summary>
    /// called to play currently set up sound
    /// </summary>

    private void NextSegment()
    {
        _currentIndex++;

        //set next soundbyte
        if (_currentIndex < _sentences)
        {
            _subtitleObject.text = _subtitleArray[_currentIndex];
            _currentDialogue.setParameterByName("Sample Sentence", _currentIndex + 1);
            _currentDialogue = AudioManager.Instance.PlaySound(_dialogue);
        }
    }
}
