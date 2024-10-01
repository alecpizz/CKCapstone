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
    private NPCScript _npc;
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
        _npc = FindObjectOfType<NPCScript>();

        _input.InGame.Interact.performed += InteractPerformed;
    }


    /// <summary>
    /// activated when the player uses the interact action
    /// is used for npc interaction
    /// </summary>
    /// <param name="context"></param>
    private void InteractPerformed(InputAction.CallbackContext context)
    {
        if (_npc != null)
        {
            _npc.AdvanceDialogue();
        }
    }
}
