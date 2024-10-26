/******************************************************************
*    Author: Josephine Qualls
*    Contributors: Josh Eddy, Alec Pizziferro
*    Date Created: 10/10/2024
*    Description: Switch that moves walls.
*    Only moves registered walls.
*******************************************************************/


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for actions taken once switch is triggered
/// </summary>
public class SwitchTrigger : MonoBehaviour
{
    //on/off variable for switch
    private bool _isTriggered = false;

    //for registering walls to a switch
    [SerializeReference] private List<IParentSwitch> _affectedObjects = new List<IParentSwitch>();


    /// <summary>
    /// Turns the switch on/off everytime the Player steps on it
    /// Moves walls when switch is on and back when it's off
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isTriggered = !_isTriggered;

            for (int i = 0; i < _affectedObjects.Count; i++)
            {
                if (_isTriggered)
                {
                    _affectedObjects[i].SwitchActivation();
                }
                else
                {
                    _affectedObjects[i].SwitchDeactivation();
                }
            }
        }
    }
}


