/******************************************************************
*    Author: Mitchell Young
*    Contributors: Mitchell Young
*    Date Created: 1/28/25
*    Description: Manager object script that enemies reference
*    to check whether or not the canvas toggle button was pressed.
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathingToggleButtonManager : MonoBehaviour
{
    public bool buttonToggled = false;
    public bool pressed = false;
    public static PathingToggleButtonManager Instance;

    /// <summary>
    /// Sets instance upon awake.
    /// </summary>
    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Sets buttonToggle bool to opposite of
    /// current state and starts the PressedButton
    /// Coroutine.
    /// </summary>
    public void ButtonToggle()
    {
        buttonToggled = !buttonToggled;
        StartCoroutine(PressedButton());
    }

    /// <summary>
    /// Sets pressed to true for a second
    /// before setting back to false.
    /// </summary>
    private IEnumerator PressedButton()
    {
        pressed = true;
        yield return new WaitForSeconds(1f);
        pressed = false;
    }
}
