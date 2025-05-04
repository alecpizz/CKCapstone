/******************************************************************
 *    Author: Alex Laubenstein
 *    Contributors: Alex Laubenstein, Nick Grinstead
 *    Date Created: 2/13/25
 *    Description: Script that handles the ability to cycle through
 *                 individual enemy paths on controller
 *******************************************************************/

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class EnemyPathCycling : MonoBehaviour
{
    public static EnemyPathCycling Instance { get; private set; }

    private PlayerControls _input;

    private int _cycleNumber = -1;

    public bool IsCycling { get; private set; } = false;
    private static bool _enemyToggleState = false;
    private bool _singleEnemy = false;
    private bool _singleEnemyPathOn = false;

    //Array to gather the enemy game objects for cycling
    private EnemyBehavior[] _allEnemyArray;

    /// <summary>
    /// Fills up the arrays with enemies
    /// </summary>          
    private void Awake()
    {
        Instance = this;
        _allEnemyArray = GameObject.FindObjectsOfType<EnemyBehavior>();
        if (_allEnemyArray.Length == 1)
        {
            _singleEnemy = true;
        }
    }

    // Start is called before the first frame update
    /// <summary>
    /// enables the controls used in the script
    /// </summary>
    private void Start()
    {
        _input = new PlayerControls();
        _input.InGame.Enable();

        _input.InGame.CycleForward.performed += PathingForward;
        _input.InGame.Toggle.performed += ToggleEnemyPaths;
        if (!_enemyToggleState) return;
        if (_allEnemyArray.Length == 0)
        {
            return;
        }
        foreach (var enemy in _allEnemyArray)
        {
            enemy.PathingToggle(IsCycling);
        }
    }

    /// <summary>
    /// Unregisters from inputs
    /// </summary>
    private void OnDisable()
    {
        _input.InGame.Disable();
        _input.InGame.CycleForward.performed -= PathingForward;
        _input.InGame.Toggle.performed -= ToggleEnemyPaths;
    }

    /// <summary>
    /// Allows someone with a controller to cycle through the enemy paths in the order of the array
    /// </summary>
    /// <param name="context"></param>
    private void PathingForward(InputAction.CallbackContext context)
    {
        //if there are no enemies return early
        if (_allEnemyArray.Length == 0)
        {
            return;
        }

        //Makes it so it just turns on and off the enemy path if there is a single enemy
        //since there is no need for it to get turned on multiple times
        if (_singleEnemy)
        {
            IsCycling = true;
            _cycleNumber = 0;
            _singleEnemyPathOn = !_singleEnemyPathOn;
            _allEnemyArray[_cycleNumber].PathingCycle();
        }
        //handles cycling between multiple enemies
        else
        {
            IsCycling = true;
            if (_cycleNumber > 0)
            {
                //resets the cycle back to the start
                if (_cycleNumber >= _allEnemyArray.Length)
                {
                    _allEnemyArray[_allEnemyArray.Length - 1].PathingCycle();
                    _cycleNumber = -1;
                }
                //turns on the next enemy path while turning off the previous enemy path
                else
                {
                    _allEnemyArray[_cycleNumber - 1].PathingCycle();
                    _allEnemyArray[_cycleNumber].PathingCycle();
                    _cycleNumber++;
                }
            }
            //if at the start of the cycle turn on all enemy paths
            else if (_cycleNumber < 0)
            {
                for (int i = 0; i < _allEnemyArray.Length; i++)
                {
                    _allEnemyArray[i].PathingCycle();
                }

                _cycleNumber++;
            }
            //turns off all enemy paths and starts singular enemy cycling
            else
            {
                for (int i = 0; i < _allEnemyArray.Length; i++)
                {
                    _allEnemyArray[i].PathingCycle();
                }

                _allEnemyArray[_cycleNumber].PathingCycle();
                _cycleNumber++;
            }
        }
    }

    /// <summary>
    /// Toggles the path VFX for all enemies
    /// </summary>
    /// <param name="context">Input context</param>
    private void ToggleEnemyPaths(InputAction.CallbackContext context)
    {
        if(MenuManager.Instance.GetPauseInvoked() == true)
        {
            return;
        }

        if (_allEnemyArray.Length == 0)
        {
            return;
        }
        foreach (var enemy in _allEnemyArray)
        {
            enemy.PathingToggle(IsCycling);
        }

        _enemyToggleState = !_enemyToggleState;

        if (IsCycling)
        {
            IsCycling = false;
            _cycleNumber = -1;
        }
    }
}