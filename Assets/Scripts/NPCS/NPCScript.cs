using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/*
* ******************************************************************
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

    [SerializeField] private TMP_Text dialogueBox;
    [SerializeField] private GameObject Player;

    void advanceDialogue()
    {
        if (currentDialogue < dialogue.Length)
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

    // Start is called before the first frame update
    void Start()
    {
        dialogueBox.SetText(dialogue[currentDialogue]);
        dialogueBox.gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        dialogueBox.gameObject.SetActive(true);
    }

    private void OnCollisionStay(Collision collision)
    {
        dialogueBox.gameObject.SetActive(true);
        //if(player presses interact key) {advanceDialogue();}
    }

    private void OnCollisionExit(Collision collision)
    {
        dialogueBox.gameObject.SetActive(false);
    }

}
