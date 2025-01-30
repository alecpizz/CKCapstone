/******************************************************************
*    Author: Josephine Qualls
*    Contributors: Trinity Hutson
*    Date Created: 01/28/2025
*    Description: Sets the index and loads the scene for the buttons that'll be made.
*******************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IndividualButtons : MonoBehaviour
{
    private int _index = 0;

    /// <summary>
    /// The index of the scene is set for the individual buttons
    /// </summary>
    /// <param name="num"></param>
    public void SetIndex(int num)
    {
        _index = num;
    }

    /// <summary>
    /// Loads the scene for the relevant button
    /// </summary>
    public void ChangeScene()
    {
        SceneManager.LoadScene(_index);
    }

}
