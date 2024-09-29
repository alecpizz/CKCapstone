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
    [SerializeField] private int _collectibleNumber;
  
    /// <summary>
    /// This method triggers the Collect method when the
    /// player collides with a collectible.
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player") 
        {
            Collect();
        }        
    }

    /// <summary>
    /// This method debugs when the player collects
    /// a collectible and checks that the player
    /// has won by collecting the right sequence of Notes. 
    /// </summary>
    void Collect() 
    {
        Debug.Log("You got a collectible!");
        WinChecker.CollectedNote?.Invoke(_collectibleNumber);
        Destroy(gameObject);
    }
}
