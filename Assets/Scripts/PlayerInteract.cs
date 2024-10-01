using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/*
********************************************************************
File name: NPCScript
author: David Henvick
Creation date: 9/30/24
summary: this is the script that is used for the interact input
*/
public class PlayerInteract : MonoBehaviour
{
    //npc
    private List<NPCScript> _npcList;
    private PlayerControls _input;

    /// <summary>
    /// Start is called before the first frame update
    /// used to assign needed input variables
    /// </summary>
    void Start()
    {
        // Referencing and setup of the Input Action functions
        _input = new PlayerControls();
        _input.InGame.Enable();
        _npcList = new List<NPCScript>();

        _input.InGame.Interact.performed += InteractPerformed;
    }

    /// <summary>
    /// called by npcs on game start. 
    /// is used to add an npc to the list of npcs that the player has
    /// </summary>
    /// <param name="addedNPC"></param> new npc
    public void AddNPC(NPCScript addedNPC) 
    {
        _npcList.Add(addedNPC);
        return;
    }

    public void RemoveNPC(NPCScript addedNPC)
    {
        _npcList.Remove(addedNPC);
        return;
    }

    /// <summary>
    /// activated when the player uses the interact action
    /// is used for npc interaction
    /// </summary>
    /// <param name="context"></param>
    private void InteractPerformed(InputAction.CallbackContext context)
    {
        if (_npcList[0] != null)
        {
            for(int i = 0; i < _npcList.Count; i++)
            {
                _npcList[i].AdvanceDialogue();
            }
        }
    }
}
