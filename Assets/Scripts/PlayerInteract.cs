using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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

        _input.InGame.Interact.performed += InteractPerformed;
    }

    /// <summary>
    /// called on collision, is used to log interactables within range
    /// </summary>
    /// <param name="collision"></param>
    void OnCollisionEnter(Collision collision)
    {
        _npc = collision.gameObject.GetComponent<NPCScript>();
        if ( _npc != null)
        {
            _npc.ShowDialogue();
        }
    }

    /// <summary>
    /// called on collision, is used to remove logged interactables when they leave the range
    /// </summary>
    /// <param name="collision"></param>
    void OnCollisionExit(Collision collision)
    {
    {
        _npc = collision.gameObject.GetComponent<NPCScript>();
        if (_npc != null)
        {
            _npc.HideDialogue();
        }
        _npc = null;
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
