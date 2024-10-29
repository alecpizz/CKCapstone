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

    private string[] _subtitleArray;
    private static int _currentIndex;
    private EventReference _currentDialogue;

    private int _characterIndex;
    private char _currentCharacter;
    private string _currentString;

    /// <summary>
    /// called on scene begin. used to set variables
    /// </summary>
    void Start()
    {

        //setting up for string division;
        _subtitleArray = new string[_sentences];
        _characterIndex = 0;
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
                UnityEngine.Debug.Log(_currentString);
                _currentString = "";
            }
            else
            {
                _currentString = _currentString + _currentCharacter;
            }
        }

        _subtitleObject.text = _subtitleArray[0];
        PlaySegment(0);

        for (int i = 1; i < _sentences; i++)
        {
            
        }

       
    }

    /// <summary>
    /// called to play currently set up sound
    /// </summary>
    void PlaySegment(int index)
    {
        //_currentDialogue = PlaySound(_dialogue);
    }

    void NextSegment()
    {
        _currentIndex++;
        //set next soundbyte
        _subtitleObject.text = _subtitleArray[_currentIndex];
        PlaySegment(_currentIndex);
    }
}
