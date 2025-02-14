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

    private int _cycleNumber = 0;

    public bool IsCycling = false;
    private bool _singleEnemy = false;

    [SerializeField] private GameObject[] _enemyArray;
    [SerializeField] private GameObject[] _sonEnemyArray;
    [SerializeField] private GameObject[] _allEnemyArray;

    private void Awake()
    {
        Instance = this;
        _enemyArray = GameObject.FindGameObjectsWithTag("Enemy");
        _sonEnemyArray = GameObject.FindGameObjectsWithTag("SonEnemy");
        _allEnemyArray = _enemyArray.Concat(_sonEnemyArray).ToArray();
        Debug.Log(_allEnemyArray.Length);
    }

    // Start is called before the first frame update
    void Start()
    {
        _input = new PlayerControls();
        _input.InGame.Enable();

        _input.InGame.CycleForward.performed += PathingForward;
        _input.InGame.CycleBack.performed += PathingBackwards;
    }

    private void PathingForward(InputAction.CallbackContext context)
    {
        
        if (_allEnemyArray.Length == 0)
        {
            //do nothing
        }
        else if (_allEnemyArray.Length == 1)
        {
            if (_singleEnemy == false)
            {
                Debug.Log(_allEnemyArray[_cycleNumber]);
                _allEnemyArray[_cycleNumber].GetComponent<EnemyBehavior>().DestPathVFX.SetActive(true);
                _allEnemyArray[_cycleNumber].GetComponent<EnemyBehavior>().DestinationMarker.SetActive(true);
                _singleEnemy = true;
            }
            else
            {
                _allEnemyArray[_cycleNumber].GetComponent<EnemyBehavior>().DestPathVFX.SetActive(false);
                _allEnemyArray[_cycleNumber].GetComponent<EnemyBehavior>().DestinationMarker.SetActive(false);
                _singleEnemy = false;
            }
        }
        else
        {
            IsCycling = true;
            if (_cycleNumber > 0)
            {
                _allEnemyArray[_cycleNumber - 1].GetComponent<EnemyBehavior>().DestPathVFX.SetActive(false);
                _allEnemyArray[_cycleNumber - 1].GetComponent<EnemyBehavior>().DestinationMarker.SetActive(false);
                _allEnemyArray[_cycleNumber].GetComponent<EnemyBehavior>().DestPathVFX.SetActive(true);
                _allEnemyArray[_cycleNumber].GetComponent<EnemyBehavior>().DestinationMarker.SetActive(true);
                if (_cycleNumber >= _allEnemyArray.Length - 1)
                {
                    _cycleNumber = 0;
                }
                else
                {
                    _cycleNumber++;
                }
            }
            else
            {
                _allEnemyArray[_allEnemyArray.Length - 1].GetComponent<EnemyBehavior>().DestPathVFX.SetActive(false);
                _allEnemyArray[_allEnemyArray.Length - 1].GetComponent<EnemyBehavior>().DestinationMarker.SetActive(false);
                _allEnemyArray[_cycleNumber].GetComponent<EnemyBehavior>().DestPathVFX.SetActive(true);
                _allEnemyArray[_cycleNumber].GetComponent<EnemyBehavior>().DestinationMarker.SetActive(true);
                _cycleNumber++;
            }
        }
    }

    private void PathingBackwards(InputAction.CallbackContext context)
    {
        if (_allEnemyArray.Length == 0)
        {
            //do nothing
        }
        else if (_allEnemyArray.Length == 1)
        {
            if (_singleEnemy == false)
            {
                Debug.Log(_allEnemyArray[_cycleNumber]);
                _allEnemyArray[_cycleNumber].GetComponent<EnemyBehavior>().DestPathVFX.SetActive(true);
                _allEnemyArray[_cycleNumber].GetComponent<EnemyBehavior>().DestinationMarker.SetActive(true);
                _singleEnemy = true;
            }
            else
            {
                _allEnemyArray[_cycleNumber].GetComponent<EnemyBehavior>().DestPathVFX.SetActive(false);
                _allEnemyArray[_cycleNumber].GetComponent<EnemyBehavior>().DestinationMarker.SetActive(false);
                _singleEnemy = false;
            }
        }
        else
        {
            IsCycling = true;
            if (_cycleNumber > 0)
            {
                Debug.Log(_allEnemyArray[_cycleNumber]);
                _allEnemyArray[_cycleNumber].GetComponent<EnemyBehavior>().DestPathVFX.SetActive(true);
                _allEnemyArray[_cycleNumber].GetComponent<EnemyBehavior>().DestinationMarker.SetActive(true);

                if (_cycleNumber == _allEnemyArray.Length - 1)
                {
                    _allEnemyArray[0].GetComponent<EnemyBehavior>().DestPathVFX.SetActive(false);
                    _allEnemyArray[0].GetComponent<EnemyBehavior>().DestinationMarker.SetActive(false);
                }
                else
                {
                    _allEnemyArray[_cycleNumber + 1].GetComponent<EnemyBehavior>().DestPathVFX.SetActive(false);
                    _allEnemyArray[_cycleNumber + 1].GetComponent<EnemyBehavior>().DestinationMarker.SetActive(false);
                    _cycleNumber--;
                }
            }
            else
            {
                _allEnemyArray[_allEnemyArray.Length + 1].GetComponent<EnemyBehavior>().DestPathVFX.SetActive(false);
                _allEnemyArray[_allEnemyArray.Length + 1].GetComponent<EnemyBehavior>().DestinationMarker.SetActive(false);
                Debug.Log(_allEnemyArray[_cycleNumber]);
                _allEnemyArray[_cycleNumber].GetComponent<EnemyBehavior>().DestPathVFX.SetActive(true);
                _allEnemyArray[_cycleNumber].GetComponent<EnemyBehavior>().DestinationMarker.SetActive(true);
                _cycleNumber = _allEnemyArray.Length - 1;
            }
        }
    }
}
