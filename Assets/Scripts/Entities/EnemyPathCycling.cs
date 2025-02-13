using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class EnemyPathCycling : MonoBehaviour
{
    private PlayerControls _input;

    private int _cycleNumber = 0;

    [SerializeField] private GameObject[] _enemyArray;
    [SerializeField] private GameObject[] _sonEnemyArray;
    [SerializeField] private GameObject[] _allEnemyArray;

    private void Awake()
    {
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
            EnemyBehavior.Instance.SinglePathingToggle(_allEnemyArray[_cycleNumber]);
        }
        else
        {
            if (_cycleNumber > 0)
            {
                Debug.Log(_cycleNumber);
                EnemyBehavior.Instance.SinglePathingToggle(_allEnemyArray[_cycleNumber - 1]);
                EnemyBehavior.Instance.SinglePathingToggle(_allEnemyArray[_cycleNumber]);
                if (_cycleNumber >= _allEnemyArray.Length - 1)
                {
                    EnemyBehavior.Instance.SinglePathingToggle(_allEnemyArray[_cycleNumber]);
                    _cycleNumber = 0;
                }
                else
                {
                    _cycleNumber++;
                }
            }
            else
            {
                Debug.Log(_cycleNumber);
                EnemyBehavior.Instance.SinglePathingToggle(_allEnemyArray[_cycleNumber]);
                _cycleNumber++;
            }
        }
    }

    private void PathingBackwards(InputAction.CallbackContext context)
    {
        int temp = 0;
        EnemyBehavior.Instance.SinglePathingToggle(_allEnemyArray[temp]);
        if (temp <= _allEnemyArray.Length - 1)
        {
            temp++;
        }
        else
        {
            temp = 0;
        }
    }
}
