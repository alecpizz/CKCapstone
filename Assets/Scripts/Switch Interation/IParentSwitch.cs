/******************************************************************
*    Author: Josephine Qualls
*    Contributors: Josh Eddy
*    Date Created: 10/24/2024
*    Description: Interface that holds the methods used
*    by all objects affected by a switch.
*******************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IParentSwitch
{
    /// <summary>
    /// Empty method that will be used for anything affected by the switch turning on
    /// </summary>
    public void SwitchActivation()
    {

    }
}
