using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerInventory : MonoBehaviour
{
    public int collectibleCount = 0;
    
   public void IncreaseCount() 
   {
    collectibleCount++;   
   }
}
