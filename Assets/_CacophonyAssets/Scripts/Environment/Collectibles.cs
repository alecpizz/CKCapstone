using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class Collectibles : MonoBehaviour
{
    [MinValue(0), MaxValue(10)]
    [SerializeField] private int _collectibleNumber;
  
    // Start is called before the first frame update
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player") 
        {
            Collect();
            
        }
        
    }

    void Collect() 
    {
        Debug.Log("You got a collectible!");
        WinChecker.CollectedNote?.Invoke(_collectibleNumber);
        Destroy(gameObject);
    }

}
