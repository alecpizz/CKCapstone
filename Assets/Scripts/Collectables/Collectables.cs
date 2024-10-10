/******************************************************************
*    Author: Taylor Sims
*    Contributors: None
*    Date Created: 09-24-24
*    Description: This script is the collection system for Notes.
*******************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class Collectables : MonoBehaviour
{
    // variables
    [MinValue(0), MaxValue(10)]
    [SerializeField] private int _collectableNumber;

    /// <summary>
    /// This method triggers the Collect method when the
    /// player collides with a collectable.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && WinChecker.Instance.CheckForCollection(_collectableNumber)) 
        {
            Collect();
        }        
    }

    /// <summary>
    /// This method debugs when the player collects
    /// a collectable and checks that the player
    /// has won by collecting the right sequence of Notes. 
    /// </summary>
    private void Collect() 
    {
        Debug.Log("You got a collectable!");
        WinChecker.CollectedNote?.Invoke(_collectableNumber);
        Destroy(gameObject);
    }
}
