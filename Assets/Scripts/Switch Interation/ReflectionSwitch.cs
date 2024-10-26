/******************************************************************
*    Author: Josephine Qualls
*    Contributors: Josh Eddy
*    Date Created: 10/24/2024
*    Description: Controls where the reflection cube reflects the harmony beam.
*******************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//inherits methods from the ParentSwitch script
public class ReflectionSwitch : ParentSwitch
{
    //Assign relevant reflection cube
    [SerializeField] ReflectiveObject mirror;

    /// <summary>
    /// When switch is on, the reflection will face the opposite direction
    /// </summary>
    public override void SwitchActivation()
    {
        //mirror.SetReflection();
    }

    /// <summary>
    /// When switch is off, the reflection will face the original direction
    /// </summary>
    public override void SwitchDeactivation()
    {
        //mirror.SetReflection();
    }
}
