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

[Serializable]
public struct DialogueEntry 
{
    public EventReference _sound;
    [TextArea] public string _text;
}

public class NPCScript : MonoBehaviour
{
    [SerializeField] private List<DialogueEntry> _dialogueEntries;

    //dialogue options
    private int _currentDialogue = 0;
    //used to tell if player is in adjacent square
    private bool _occupied;

    [SerializeField] private TMP_Text _dialogueBox;

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


        if (_currentDialogue < _dialogueEntries.Count - 1)
        {
            _currentDialogue++;             
            _dialogueBox.SetText(_dialogueEntries[_currentDialogue]._text);
        }
        else
        {
            _currentDialogue = 0;
            _dialogueBox.SetText(_dialogueEntries[_currentDialogue]._text);
        }
    }

    /// <summary>
    /// Start is called before the first frame update
    /// used here to grabe the dialogue ui item and to set the occupied variable
    /// </summary>
    void Start()
    {

        if(CheckForEntries())
            _dialogueBox.SetText(_dialogueEntries[_currentDialogue]._text);
        _dialogueBox.gameObject.SetActive(false);
        _occupied = false;
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

    /// <summary>
    /// use this to show the dialogue of the npc
    /// </summary>
    public void ShowDialogue()
    {
        if (CheckForEntries())
            _dialogueBox.SetText(_dialogueEntries[_currentDialogue]._text);

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
    }

    
}
