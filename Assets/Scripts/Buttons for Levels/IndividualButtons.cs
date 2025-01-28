/******************************************************************
*    Author: Josephine Qualls
*    Contributors: Trinity Hutson
*    Date Created: 01/28/2025
*    Description: 
*******************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IndividualButtons : MonoBehaviour
{
    private int _index = 0;

    public void setIndex(int num)
    {
        _index = num;
    }

    public void changeScene()
    {
        SceneManager.LoadScene(_index);
    }

}
