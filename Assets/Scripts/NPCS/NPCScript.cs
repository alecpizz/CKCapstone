using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;



/*
********************************************************************
File name: NPCScript
author: David Henvick
Creation date: 9/30/24
summary: this is the script that is used control an npc and their dialogue
*/


public class NPCScript : MonoBehaviour
{
    [SerializeField] private List<EventReference>  _dialogueSound = default;
    [SerializeField] private string[] _dialogueText;
    private EventInstance _eventInstance;

    private PlayerInteract _player;
    //dialogue options
    int _currentDialogue = 0;
    //used to tell if player is in adjacent square
    bool _occupied;

    [SerializeField] private TMP_Text _dialogueBox;

    /// <summary>
    /// is used to advance the current dialogue
    /// is called by player when the interact key is used
    /// </summary>
    public void AdvanceDialogue()
    {
        Debug.Log(_currentDialogue + "out of occupied");
        if (_occupied)
        {
            Debug.Log(_dialogueText[_currentDialogue]);
            //AudioManager.Instance.StopSound(_eventInstance);

            if (_currentDialogue < _dialogueText.Length-1)
            {
                _currentDialogue++;
                _dialogueBox.SetText(_dialogueText[_currentDialogue]);
               // _eventInstance = AudioManager.Instance.PlaySound(_dialogueSound[_currentDialogue]);
            }
            else
            {
                _currentDialogue = 0;
                _dialogueBox.SetText(_dialogueText[_currentDialogue]);
                // _eventInstance = AudioManager.Instance.PlaySound(_dialogueSound[_currentDialogue]);
            }


        }
    }

    /// <summary>
    /// Start is called before the first frame update
    /// used here to grabe the dialogue ui item and to set the occupied variable
    /// </summary>
    void Start()
    {
        _player = FindObjectOfType<PlayerInteract>();
        _player.AddNPC(this);

        _dialogueBox.SetText(_dialogueText[_currentDialogue]);
        _dialogueBox.gameObject.SetActive(false);
        _occupied = false;

        _dialogueSound = new List<EventReference>();

    }

    /// <summary>
    /// activated on collision enter
    /// if the player enters the box collision dialogue is enabled anc can be advanced
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.tag);
        if(collision.gameObject.tag == "NPCReadable")
        {
           // _eventInstance = AudioManager.Instance.PlaySound(_dialogueSound[_currentDialogue]);
            _dialogueBox.gameObject.SetActive(true);
            _occupied = true;
        }
    }

    /// <summary>
    /// activated on collision exit
    /// if the player leaves the space adjacent to the npc, the dialogue is disabled 
    /// and the dialogue can no longer be advanced
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "NPCReadable")
        {

            _dialogueBox.gameObject.SetActive(false);
            _occupied = false;
            //AudioManager.Instance.StopSound(_eventInstance);
        }
    }

}
