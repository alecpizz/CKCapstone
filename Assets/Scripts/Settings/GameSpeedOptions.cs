/******************************************************************
 *    Author: Cole Stranczek
 *    Contributors: 
 *    Date Created: 1/26/25
 *    Description: Attachment script to allow players to adjust the
 *    speed of the game
 *******************************************************************/

using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Rendering;

public class GameSpeedOptions : MonoBehaviour
{
    private PlayerControls _playerControls;

    private int[] _speedModes = new int[2];
    private int _currentSpeed;
    
    //Event instances for the player movement sounds
    [SerializeField] private EventReference[] _speedSounds;

    [Tooltip("What the timescale is changed to when the game is sped up." +
        "The defualt value is 1, so make sure this is higher than 1 to see an actual change.")]
    [SerializeField] private float _speedUpRate = 2f;

    /// <summary>
    /// Enables the controls to allow the input to work, and ensures that the bools are always
    /// that default speed is always the norm at the start of a scene
    /// </summary>
    private void Start()
    { 
        _playerControls = new PlayerControls();
        _playerControls.Enable();
        _playerControls.InGame.GameSpeed.performed += ctx => SpeedChange();

        _currentSpeed = System.Array.IndexOf(_speedModes, _speedModes[0]);
    }

    /// <summary>
    /// This function handles how the game's speed changes based on what speed the game is
    /// currently at when the Shift key is hit
    /// </summary>
    private void SpeedChange()
    {
        // Speed up if the game is at normal speed
        if (_currentSpeed == 0)
        {
            Time.timeScale = _speedUpRate;
            _currentSpeed++;
            foreach (EventReference sound in _speedSounds)
            {
                AudioManager.Instance.SpeedSound(sound, _currentSpeed + 1);
            }
        }
        // Return to normal speed if the game is sped up
        else
        {
            Time.timeScale = 1f;
            _currentSpeed--;
            foreach (EventReference sound in _speedSounds)
            {
                AudioManager.Instance.SpeedSound(sound, _currentSpeed + 1);
            }
        }
    }
}
