/******************************************************************
*    Author: Josephine Qualls
*    Contributors: Josh Eddy
*    Date Created: 10/24/2024
*    Description: Controls where the reflection cube reflects the harmony beam.
*******************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//inherits methods from IParentSwitch
public class ReflectionSwitch : MonoBehaviour, IParentSwitch
{
    //Assign relevant reflection cube
    [SerializeField] ReflectiveObject mirror;

    /// <summary>
    /// When switch is on, the reflection will face the opposite direction
    /// </summary>
    public void SwitchActivation()
    {
        mirror.ChangeDirection(false);
    }

    /// <summary>
    /// When switch is off, the reflection will face the original direction
    /// </summary>
    public void SwitchDeactivation()
    {
        mirror.ChangeDirection(true);
    }
}
