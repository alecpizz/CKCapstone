/******************************************************************
 *    Author: Alex Laubenstein
 *    Contributors: Alex Laubenstein
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
    public bool CycleUnset = false;
    private bool _singleEnemy = false;
    private bool _singleEnemyPathOn = false;

    //Arrays to gather the enemy game objects for cycling
    private GameObject[] _enemyArray;
    private GameObject[] _sonEnemyArray;
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
        Debug.Log(_cycleNumber);
    }

    /// <summary>
    /// Stops the enemy cycling when the toggle button is pressed
    /// </summary>
    private void Update()
    {
        if (CycleUnset)
        {
            for (int i = 0; i < _allEnemyArray.Length; i++)
            {
                _allEnemyArray[i].DestPathVFX.SetActive(false);
                _allEnemyArray[i].DestinationMarker.SetActive(false);
            }
            CycleUnset = false;
            IsCycling = false;
            _cycleNumber = -1;
        }
    }

    /// <summary>
    /// 
    /// Allows someone with a controller to cycle through the enemy paths in the order of the array
    /// </summary>
    /// <param name="context"></param>
    private void PathingForward(InputAction.CallbackContext context)
    {
        
        if (_allEnemyArray.Length == 0)
        {
            return;
        }
        if (_singleEnemy)
        {
            IsCycling = true;
            _cycleNumber = 0;
            _singleEnemyPathOn = !_singleEnemyPathOn;
            _allEnemyArray[_cycleNumber].DestPathVFX.SetActive(_singleEnemyPathOn);
            _allEnemyArray[_cycleNumber].DestinationMarker.SetActive(_singleEnemyPathOn);
        }
        else
        {
            IsCycling = true;
            if (_cycleNumber > 0)
            {
                if (_cycleNumber >= _allEnemyArray.Length)
                {
                    _allEnemyArray[_allEnemyArray.Length - 1].DestPathVFX.SetActive(false);
                    _allEnemyArray[_allEnemyArray.Length - 1].DestinationMarker.SetActive(false);
                    _cycleNumber = -1;
                }
                else
                {
                    _allEnemyArray[_cycleNumber - 1].DestPathVFX.SetActive(false);
                    _allEnemyArray[_cycleNumber - 1].DestinationMarker.SetActive(false);
                    _allEnemyArray[_cycleNumber].DestPathVFX.SetActive(true);
                    _allEnemyArray[_cycleNumber].DestinationMarker.SetActive(true);
                    _cycleNumber++;
                }
            }
            else if (_cycleNumber < 0)
            {
                for (int i = 0; i < _allEnemyArray.Length; i++)
                {
                    _allEnemyArray[i].DestPathVFX.SetActive(true);
                    _allEnemyArray[i].DestinationMarker.SetActive(true);
                }
                _cycleNumber++;
            }
            else
            {
                for (int i = 0; i < _allEnemyArray.Length; i++)
                {
                    _allEnemyArray[i].DestPathVFX.SetActive(false);
                    _allEnemyArray[i].DestinationMarker.SetActive(false);
                }
                _allEnemyArray[_allEnemyArray.Length - 1].DestPathVFX.SetActive(false);
                _allEnemyArray[_allEnemyArray.Length - 1].DestinationMarker.SetActive(false);
                _allEnemyArray[_cycleNumber].DestPathVFX.SetActive(true);
                _allEnemyArray[_cycleNumber].DestinationMarker.SetActive(true);
                _cycleNumber++;
            }
        }
    }
}
