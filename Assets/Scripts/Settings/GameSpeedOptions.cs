/******************************************************************
 *    Author: Cole Stranczek
 *    Contributors: 
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

public class GameSpeedOptions : MonoBehaviour
{
    private PlayerControls _playerControls;

    [SerializeField] private bool holdMode;

    [Tooltip("What the timescale is changed to when the game is sped up." +
        "The defualt value is 1, so make sure this is higher than 1 to see an actual change.")]
    [SerializeField] private float _speedUpRate = 2f;

    /// <summary>
    /// Enables the controls to allow the input to work, and ensures that the bools are always
    /// that default speed is always the norm at the start of a scene
    /// </summary>
    private void Start()
    {
        holdMode = true;

        _playerControls.InGame.GameSpeed.performed +=
            ctx =>
        {
            if (ctx.interaction is HoldInteraction && holdMode)
            {
                SpeedChangeHoldStart();
            }
            else if (!holdMode)
            {
                SpeedChange();
            }
        };

        _playerControls.InGame.GameSpeed.canceled +=
            ctx =>
            {
                if (ctx.interaction is HoldInteraction && holdMode)
                {
                    SpeedChangeHoldEnd();
                }
            };


    }

    private void OnEnable()
    {
        _playerControls = new PlayerControls();
        _playerControls.InGame.GameSpeed.Enable();
    }

    private void OnDisable()
    {
        _playerControls.InGame.GameSpeed.Disable();
    }

    public void ToggleChange()
    {
        if (holdMode)
        {
            Debug.Log("Changing to Toggle");
            //_playerControls = new PlayerControls();

            holdMode = false;
        }
        else
        { 
            Debug.Log("Changing to Hold");
            ///_playerControls = new PlayerControls();

            holdMode = true;
        }
    }

    /// <summary>
    /// This function handles how the game's speed changes based on what speed the game is
    /// currently at when the Shift key is hit
    /// </summary>
    private void SpeedChange()
    {
        // Speed up if the game is at normal speed
        if(Time.timeScale == 1f && !holdMode)
        {
            Debug.Log("Speeding Up (Toggle)");
            Time.timeScale = _speedUpRate;
        }
        // Return to normal speed if the game is sped up
        else
        {
            Debug.Log("Back to Normal (Toggle)");
            Time.timeScale = 1f;
        }
    }

    private void SpeedChangeHoldStart()
    {
        if (Time.timeScale == 1f && holdMode)
        {
            Debug.Log("Speeding Up (Held)");
            Time.timeScale = _speedUpRate;
        }
    }

    private void SpeedChangeHoldEnd()
    {
        if (Time.timeScale == _speedUpRate && holdMode)
        {
            Debug.Log("Back to Normal (Released)");
            Time.timeScale = 1f;
        }
    }
}
