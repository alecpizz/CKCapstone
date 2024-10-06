/******************************************************************
*    Author: David Henvick
*    Contributors: Claire Noto
*    Date Created: 09/30/2024
*    Description: this is the script that is used control an npc 
*    and their dialogue
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System;
using NaughtyAttributes;

[Serializable]
public struct DialogueEntry 
{
    public EventReference _sound;
    [TextArea] public string _text;
    [InfoBox("This adjusts the speed of the text. A value of -5 slows it down while a value of 5 speeds it up", EInfoBoxType.Normal)]
    [Range(-5f, 5f)] public float _adjustTypingSpeed;
}

public class NPCScript : MonoBehaviour
{
    [SerializeField] private TMP_Text _dialogueBox;
    [InfoBox("This adjusts the base typing speed. 2 is the slowest, 10 is the fastest", EInfoBoxType.Normal)]
    [Range(2f, 10f)] [SerializeField] private float _typingSpeed = 5f;
    [SerializeField] private List<DialogueEntry> _dialogueEntries;

    //dialogue options
    private int _currentDialogue = 0;
    private float _currentTypingSpeed;
    //used to tell if player is in adjacent square
    private bool _occupied;
    private Coroutine _typingCoroutine;
    private bool _isTyping = false;
    private string _currentFullText;

    // waiting until new AudioManager update gets pushed
    //private EventInstance _currentInstance;

    /// <summary>
    /// Start is called before the first frame update
    /// used here to grabe the dialogue ui item and to set the occupied variable
    /// </summary>
    void Start()
    {
        if (CheckForEntries())
            _dialogueBox.SetText(_dialogueEntries[_currentDialogue]._text);
        _dialogueBox.gameObject.SetActive(false);
        _occupied = false;
        _currentTypingSpeed = Mathf.Clamp(_typingSpeed - _dialogueEntries[_currentDialogue]._adjustTypingSpeed, 2f, 15f) / 100f;
    }

    /// <summary>
    /// is used to advance the current dialogue
    /// is called by player when the interact key is used
    /// </summary>
    public void AdvanceDialogue()
    {
        if (!_occupied)
        {
            return;
        }

        if (!CheckForEntries())
        {
            return;
        } 

        // skips the typing to show complete text
        if (_isTyping)
        {
            FinishTyping();
            return;
        }

        if (_currentDialogue < _dialogueEntries.Count - 1)
        {
            _currentDialogue++;             
        }
        else
        {
            _currentDialogue = 0;
        }

        if (_typingCoroutine != null)
        {
            //AudioManager.Instance.StopSound(_currentInstance);
            StopCoroutine(_typingCoroutine);
        }

        // adjusts typing speed on a per-entry basis
        _currentTypingSpeed = Mathf.Clamp(_typingSpeed - _dialogueEntries[_currentDialogue]._adjustTypingSpeed, 2f, 15f) / 100f;

        //_currentInstance = AudioManager.Instance.PlaySound(_dialogueEntries[_currentDialogue]._sound);

        _typingCoroutine = StartCoroutine(TypeDialogue(_dialogueEntries[_currentDialogue]._text));
    }

    /// <summary>
    /// use this to show the dialogue of the npc
    /// </summary>
    public void ShowDialogue()
    {
        if (CheckForEntries())
        {
            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
            }
            _typingCoroutine = StartCoroutine(TypeDialogue(_dialogueEntries[_currentDialogue]._text));
        }

        _dialogueBox.gameObject.SetActive(true);
        _occupied = true;
    }

    /// <summary>
    /// use this to hide the dialogue of the npc
    /// </summary>
    public void HideDialogue()
    {
        _dialogueBox.gameObject.SetActive(false);
        _occupied = false;

        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }
    }

    /// <summary>
    /// Immediately show the entire text if it's currently being typed
    /// </summary>
    private void FinishTyping()
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }

        _dialogueBox.SetText(_currentFullText);
        _isTyping = false;
    }

    /// <summary>
    /// Coroutine to display the dialogue one letter at a time
    /// </summary>
    /// <param name="dialogue"></param>
    /// <returns></returns>
    private IEnumerator TypeDialogue(string dialogue)
    {
        _isTyping = true;
        _currentFullText = dialogue;
        _dialogueBox.SetText("");
        foreach (char letter in dialogue.ToCharArray())
        {
            _dialogueBox.text += letter;

            switch (letter)
            {             
                case '?':
                case '!':
                case '.':
                    yield return new WaitForSeconds(_currentTypingSpeed * 3f);
                    break;
                case ',':
                    yield return new WaitForSeconds(_currentTypingSpeed * 1.5f);
                    break;
            }

            yield return new WaitForSeconds(_currentTypingSpeed);
        }

        _isTyping = false;
    }

    /// <summary>
    /// Makes sure the NPC has dialogue entries
    /// </summary>
    /// <returns>true if it does, false if it does not</returns>
    private bool CheckForEntries()
    {
        if (_dialogueEntries.Count == 0)
        {
            Debug.LogWarning("No entries in " + gameObject.name + ", please add some.");
            _dialogueBox.SetText("No Entries in NPC.");
            return false;
        }
        return true;
    }
}
