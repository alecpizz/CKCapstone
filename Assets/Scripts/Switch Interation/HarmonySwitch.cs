/******************************************************************
*    Author: Josephine Qualls
*    Contributors: Josh Eddy
*    Date Created: 10/22/2024
*    Description: Controls where the harmony is facing.
*******************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//inherits methods from the ParentSwitch script
public class HarmonySwitch : ParentSwitch
{

    /// <summary>
    /// When switch is on, the harmony beam faces it's inverse direction
    /// </summary>
    public override void SwitchActivation()
    {
        gameObject.transform.eulerAngles = new Vector3(0f, gameObject.transform.eulerAngles.y + 180, 0f);
        print("I work");
    }

    /// <summary>
    /// When switch is off, the harmony beam returns to it's original position
    /// </summary>
    public override void SwitchDeactivation()
    {
        gameObject.transform.eulerAngles = new Vector3(0f, gameObject.transform.eulerAngles.y - 180, 0f);

        print("I also work");
    }


}
