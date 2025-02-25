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

    public bool IsCycling = false;
    public bool CycleUnset = false;
    private bool _singleEnemy = false;
    private bool _singleEnemyPathOn = false;

    //Arrays to gather the enemy game objects for cycling
    [SerializeField] private GameObject[] _enemyArray;
    [SerializeField] private GameObject[] _sonEnemyArray;
    [SerializeField] private GameObject[] _allEnemyArray;
    [SerializeField] private List<EnemyBehavior> _allEnemyBehaviorList;

    /// <summary>
    /// Fills up the arrays with enemies
    /// </summary>
    private void Awake()
    {
        Instance = this;
        _enemyArray = GameObject.FindGameObjectsWithTag("Enemy");
        _sonEnemyArray = GameObject.FindGameObjectsWithTag("SonEnemy");
        _allEnemyArray = _enemyArray.Concat(_sonEnemyArray).ToArray();
        foreach (var enemy in _allEnemyArray)
        {
            if (enemy.TryGetComponent(out EnemyBehavior behaviour))
            {
                _allEnemyBehaviorList.Add(behaviour);
            }

        }
        if (_allEnemyArray.Length == 1)
        {
            _singleEnemy = true;
        }
    }

    // Start is called before the first frame update
    /// <summary>
    /// enables the controls used in the script
    /// </summary>
    void Start()
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
            for (int i = 0; i < _allEnemyBehaviorList.Count; i++)
            {
                _allEnemyBehaviorList[i].DestPathVFX.SetActive(false);
                _allEnemyBehaviorList[i].DestinationMarker.SetActive(false);
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
        
        if (_allEnemyBehaviorList.Count == 0)
        {
            //do nothing
        }
        else if (_singleEnemy)
        {
            IsCycling = true;
            _cycleNumber = 0;
            _singleEnemyPathOn = !_singleEnemyPathOn;
            _allEnemyBehaviorList[_cycleNumber].DestPathVFX.SetActive(_singleEnemyPathOn);
            _allEnemyBehaviorList[_cycleNumber].DestinationMarker.SetActive(_singleEnemyPathOn);
        }
        else
        {
            IsCycling = true;
            if (_cycleNumber > 0)
            {
                if (_cycleNumber >= _allEnemyBehaviorList.Count)
                {
                    _allEnemyBehaviorList[_allEnemyBehaviorList.Count - 1].DestPathVFX.SetActive(false);
                    _allEnemyBehaviorList[_allEnemyBehaviorList.Count - 1].DestinationMarker.SetActive(false);
                    _cycleNumber = -1;
                }
                else
                {
                    _allEnemyBehaviorList[_cycleNumber - 1].DestPathVFX.SetActive(false);
                    _allEnemyBehaviorList[_cycleNumber - 1].DestinationMarker.SetActive(false);
                    _allEnemyBehaviorList[_cycleNumber].DestPathVFX.SetActive(true);
                    _allEnemyBehaviorList[_cycleNumber].DestinationMarker.SetActive(true);
                    _cycleNumber++;
                }
            }
            else if (_cycleNumber < 0)
            {
                for (int i = 0; i < _allEnemyBehaviorList.Count; i++)
                {
                    _allEnemyBehaviorList[i].DestPathVFX.SetActive(true);
                    _allEnemyBehaviorList[i].DestinationMarker.SetActive(true);
                }
                _cycleNumber++;
            }
            else
            {
                for (int i = 0; i < _allEnemyBehaviorList.Count; i++)
                {
                    _allEnemyBehaviorList[i].DestPathVFX.SetActive(false);
                    _allEnemyBehaviorList[i].DestinationMarker.SetActive(false);
                }
                _allEnemyBehaviorList[_allEnemyBehaviorList.Count - 1].DestPathVFX.SetActive(false);
                _allEnemyBehaviorList[_allEnemyBehaviorList.Count - 1].DestinationMarker.SetActive(false);
                _allEnemyBehaviorList[_cycleNumber].DestPathVFX.SetActive(true);
                _allEnemyBehaviorList[_cycleNumber].DestinationMarker.SetActive(true);
                _cycleNumber++;
            }
        }
    }
}
