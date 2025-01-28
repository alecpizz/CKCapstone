/******************************************************************
 *    Author: Cole Stranczek
 *    Contributors: 
 *    Date Created: 1/26/25
 *    Description: Attachment script to allow players to adjust the
 *    speed of the game
 *******************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GameSpeedOptions : MonoBehaviour
{
    private PlayerControls _playerControls;
    private bool _speedUp;
    private bool _speedDown;

    [Tooltip("Default value is 1, so make sure this is greater than 1")]
    [SerializeField] private float _speedUpRate = 2f;

    [Tooltip("Default value is 1, so make sure this is less than 1")]
    [SerializeField] private float _slowDownRate = 0.5f;

    /// <summary>
    /// Enables the controls to allow the input to work, and ensures that the bools are always
    /// that default speed is always the norm at the start of a scene
    /// </summary>
    private void Start()
    { 
        _playerControls = new PlayerControls();
        _playerControls.Enable();
        _playerControls.InGame.GameSpeed.performed += ctx => SpeedChange();

        _speedUp = false;
        _speedDown = false;
    }

    /// <summary>
    /// This function handles how the game's speed changes based on what speed the game is
    /// currently at when the Shift key is hit
    /// </summary>
    private void SpeedChange()
    {
        // Speed up if the game is at normal speed
        if(!_speedUp && !_speedDown)
        {
            Debug.Log("Speeding Up!");
            _speedUp = true;

            Time.timeScale = _speedUpRate;
        }
        // Slow down if the game is sped up
        else if(_speedUp)
        {
            Debug.Log("Slowing Down!");
            _speedUp = false;
            _speedDown = true;

            Time.timeScale = _slowDownRate;
        }
        // Return to normal speed if the game is slowed down
        else
        {
            Debug.Log("Back to Normal!");
            _speedUp = false;
            _speedDown = false;

            Time.timeScale = 1f;
        }
    }


}
