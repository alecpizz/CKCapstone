using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class switch_trigger : MonoBehaviour
{
    //varibles
    bool _isTriggered = false;

    [SerializeField] List<moving_wall> movWalls = new List<moving_wall>();


    //switch on/off function
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (_isTriggered)
            {
                _isTriggered = false;
                print("Switch Off");
            }
            else
            {
                _isTriggered = true;
                print("Switch On");
            }
        }

        for(int i = 0; i < movWalls.Count; i++)
        {
            if(_isTriggered)
            {
                movWalls[i].Wall_Is_Moved();
            }
            else
            {
                movWalls[i].Wall_Move_Back();
            }
        }
    }

}
