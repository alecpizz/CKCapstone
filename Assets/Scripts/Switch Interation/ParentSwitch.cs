/******************************************************************
*    Author: Josephine Qualls
*    Contributors: Josh Eddy
*    Date Created: 10/24/2024
*    Description: Parent script that holds the virtual methods overridden
*    by all objects affected by the switch.
*******************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentSwitch : MonoBehaviour
{
    /// <summary>
    /// Empty method that will be overridden for anything affected by the switch turning on
    /// </summary>
    public virtual void SwitchActivation()
    {

    }

    /// <summary>
    /// Empty method that will be overridden for anything affected by the switch turning off
    /// </summary>
    public virtual void SwitchDeactivation()
    {

    }
}
