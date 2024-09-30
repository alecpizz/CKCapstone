using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCScript : MonoBehaviour
{
    //dialogue options
    string[] dialogue = {"Hello! this is testing dialogue", "I wish I were a fish", "They don't know my secret"};
    int currentDialogue = 0;

    void advanceDialogue()
    {
        if (currentDialogue < dialogue.Length)
        {
            currentDialogue++;
        }
        else
        {
            currentDialogue = 0;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }




}
