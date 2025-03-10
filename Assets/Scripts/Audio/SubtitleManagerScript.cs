/******************************************************************
*    Author: David Henvick
*    Contributors: 
*    Date Created: 10/28/2024
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
using EventInstance = FMOD.Studio.EventInstance;

public class SubtitleManager : MonoBehaviour
{
    //code set up
    [SerializeField] private EventReference _dialogue;
    [SerializeField, TextArea(6, 12)] private string _subtitleText;
    [SerializeField] private int _sentences;
    [SerializeField] private float _sentenceDelay = 1;

    //visual set up
    [SerializeField] private TMP_Text _subtitleObject;
    
    //variables for string splitting
    private string[] _subtitleArray;
    private char _currentCharacter;
    private string _currentString;

    //FMOD stuffs
    private static int _currentIndex;
    private EventInstance _currentDialogue;

    CutsceneFramework _cutsceneFramework;



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


        //splitting string to sentence array
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

        //playing first segment
        _subtitleObject.text = _subtitleArray[0];

        StartCoroutine(SubtitleSequence());
        _currentDialogue = AudioManager.Instance.PlaySound(_dialogue);
    }

    /// <summary>
    /// called on tics
    /// used to find if the current line of dialogue is finished
    /// </summary>
    private void Update()
    {
        // Disable subtitles
        /*if (PlayerPrefs.GetInt("Subtitles") == 0)
            return;*/

        /*
        if (!IsPlaying(_currentDialogue))
        {
            AudioManager.Instance.StopSound(_currentDialogue);
            NextSegment();
        }*/
    }

    IEnumerator SubtitleSequence()
    {
        while(_currentIndex < _sentences)
        {
            yield return new WaitForSeconds(_sentenceDelay);
            NextSegment();
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
            //_currentDialogue = AudioManager.Instance.PlaySound(_dialogue);
        }
    }
}
