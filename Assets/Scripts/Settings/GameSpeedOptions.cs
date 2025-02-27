/******************************************************************
 *    Author: Cole Stranczek
 *    Contributors: Rider Hagen
 *    Date Created: 1/26/25
 *    Description: Attachment script to allow players to adjust the
 *    speed of the game
 *******************************************************************/

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class GameSpeedOptions : MonoBehaviour
{
    private PlayerControls _playerControls;
    [SerializeField] private GameObject _speedUI;

    //[SerializeField] private GameObject _toggleMenu;
    //private Toggle _speedToggle;
    // private bool _speedToggle;

    [Tooltip("What the timescale is changed to when the game is sped up." +
        "The defualt value is 1, so make sure this is higher than 1 to see an actual change.")]
    [SerializeField] private float _speedUpRate = 2f;

    /// <summary>
    /// Enables the controls to allow the input to work, and ensures that the bools are always
    /// that default speed is always the norm at the start of a scene
    /// </summary>
    private void Start()
    {
        // _speedToggle = false;//_toggleMenu.GetComponent<Toggle>();

        _playerControls.InGame.GameSpeed.performed +=
            ctx =>
        {
            // /*if (ctx.interaction is HoldInteraction && _speedToggle.isOn)
            // {
            //     SpeedChangeHoldStart();
            // }
            // else*/ if (!_speedToggle/*.isOn*/)
            {
                SpeedChange();
            }
        };

        /*_playerControls.InGame.GameSpeed.canceled +=
            ctx =>
            {
                if (ctx.interaction is HoldInteraction && _speedToggle.isOn)
                {
                    SpeedChangeHoldEnd();
                }
            };*/


    }
    /// <summary>
    /// Initializes and turns on the controls for the player when the scene loads
    /// </summary>
    private void OnEnable()
    {
        _playerControls = new PlayerControls();
        _playerControls.InGame.GameSpeed.Enable();
    }

    /// <summary>
    /// Turns off the controls for the player when the scene is no longer active
    /// </summary>
    private void OnDisable()
    {
        _playerControls.InGame.GameSpeed.Disable();
    }

    /// <summary>
    /// The function called when you click the "Hold to Speed Up" button in the menu.
    /// Uses the "isOn" feature of the Toggle component to change the controls
    /// </summary>
    /*public void ToggleChange()
    {
        if(_speedToggle == null)
        {
            _speedToggle = _toggleMenu.GetComponent<Toggle>();
        }

        if (_speedToggle.isOn)
        {
            Debug.Log("Changing to Toggle");

            _speedToggle.isOn = false;
        }
        else
        { 
            Debug.Log("Changing to Hold");

            _speedToggle.isOn = false;
        }
    }*/

    /// <summary>
    /// This function handles how the game's speed changes based on what speed the game is
    /// currently at when the Shift key is hit
    /// </summary>
    private void SpeedChange()
    {
        // Speed up if the game is at normal speed. Activates the speed ui
        if (Mathf.Approximately(Time.timeScale, 1f)) //&& !_speedToggle/*.isOn*/)
        {
            Debug.Log("Speeding Up (Toggle)");
            Time.timeScale = _speedUpRate;
            _speedUI.SetActive(true);
        }
        // Return to normal speed if the game is sped up. Deactivates the speed ui
        else
        {
            Debug.Log("Back to Normal (Toggle)");
            Time.timeScale = 1f;
            _speedUI.SetActive(false);
        }
    }

    /// <summary>
    /// Increases the speed of the game in repsonse to the player holding down Shift
    /// </summary>
    /*private void SpeedChangeHoldStart()
    {
        if (Time.timeScale == 1f && _speedToggle.isOn)
        {
            Debug.Log("Speeding Up (Held)");
            Time.timeScale = _speedUpRate;
        }
    }*/

    /// <summary>
    /// Reverts the speed of the game back to normal in response to the player letting go of shift
    /// </summary>
    /*private void SpeedChangeHoldEnd()
    {
        if (Time.timeScale == _speedUpRate && _speedToggle.isOn)
        {
            Debug.Log("Back to Normal (Released)");
            Time.timeScale = 1f;
        }
    }*/
}
