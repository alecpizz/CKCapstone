using SaintsField;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
  // ****************************************************************
//*    Author: Nate Mitchell
//* Date Created: February 1st, 2025
//*******************************************************************/// <summary>
/// 
/// The purpose of this script is to take the inputed numbers as time, wait for a the specified amount of seconds, then provide the framework to switch from a cutscene
/// the next level. As of now there is no scene to load into. 

    
    public int CsMin;
    public int CsSec;
    
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(WaitForTenSecondsCoroutine());
       

    }
    IEnumerator WaitForTenSecondsCoroutine()
    {
        yield return new WaitForSeconds((CsMin * 60) + CsSec);
        //Translates minutes into seconds and waits for alloted time
       
        //SceneManager.LoadScene();
        

        Debug.Log((CsMin * 60) + CsSec);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
