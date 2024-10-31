/******************************************************************
*    Author: Josephine Qualls
*    Contributors: Josh Eddy, Alec Pizziferro
*    Date Created: 10/10/2024
*    Description: Switch that moves mechanics.
*    Only moves registered mechanics.
*******************************************************************/


using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

/// <summary>
/// Class for actions taken once switch is triggered
/// </summary>
public class SwitchTrigger : MonoBehaviour
{
    //on/off variable for switch
    private bool _isTriggered = false;

    //for registering mechanics to a switch
    [SerializeReference] private List<MovingWall> _affectedWalls = new List<MovingWall>();
    [SerializeReference] private List<ReflectionSwitch> _affectedReflectors = new List<ReflectionSwitch>();
    [SerializeReference] private List<HarmonySwitch> _affectedBeams = new List<HarmonySwitch>();


    /// <summary>
    /// Turns the switch on/off everytime the Player steps on it
    /// Moves walls when switch is on and back when it's off
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement playerMovement;
            if (other.gameObject.TryGetComponent<PlayerMovement>(out playerMovement))
            {
                playerMovement.ForceTurnEnd();
            }

            _isTriggered = !_isTriggered;

            //changes the walls
            for (int i = 0; i < _affectedWalls.Count; i++)
            {
                if (_isTriggered)
                {
                    _affectedWalls[i].SwitchActivation();
                }
                else
                {
                    _affectedWalls[i].SwitchDeactivation();
                }
            }

            //changes the reflection cubes
            for (int i = 0; i < _affectedReflectors.Count; i++)
            {
                if (_isTriggered)
                {
                    _affectedReflectors[i].SwitchActivation();
                }
                else
                {
                    _affectedReflectors[i].SwitchDeactivation();
                }
            }

            //changes the harmony beams
            for (int i = 0; i < _affectedBeams.Count; i++)
            {
                if (_isTriggered)
                {
                    _affectedBeams[i].SwitchActivation();
                }
                else
                {
                    _affectedBeams[i].SwitchDeactivation();
                }
            }
        }
    }
}


