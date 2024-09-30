using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/*
********************************************************************
File name: NPCScript
author: David Henvick
Creation date: 9/30/24
summary: this is the script that is used control an npc and their dialogue
*/
public class NPCScript : MonoBehaviour
{
    //dialogue options
    string[] dialogue = {"Hello! this is testing dialogue", "I wish I were a fish", "They don't know my secret"};
    int currentDialogue = 0;
    //used to tell if player is in adjacent square
    bool occupied;

    [SerializeField] private TMP_Text dialogueBox;

    /// <summary>
    /// is used to advance the current dialogue
    /// is called by player when the interact key is used
    /// </summary>
    public void AdvanceDialogue()
    {
        if (occupied)
        {
            if (currentDialogue < dialogue.Length - 1)
            {
                currentDialogue++;
                dialogueBox.SetText(dialogue[currentDialogue]);
            }
            else
            {
                currentDialogue = 0;
                dialogueBox.SetText(dialogue[currentDialogue]);
            }
        }
    }

    /// <summary>
    /// Start is called before the first frame update
    /// used here to grabe the dialogue ui item and to set the occupied variable
    /// </summary>
    void Start()
    {
        dialogueBox.SetText(dialogue[currentDialogue]);
        dialogueBox.gameObject.SetActive(false);
        occupied = false;
    }

    /// <summary>
    /// activated on collision enter
    /// if the player enters the box collision dialogue is enabled anc can be advanced
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.name == "NPCReader")
        {
            dialogueBox.gameObject.SetActive(true);
            occupied = true;
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
        if (collision.gameObject.name == "NPCReader")
        {
            dialogueBox.gameObject.SetActive(false);
            occupied = false;
        }
    }

}
