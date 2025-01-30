/******************************************************************
*    Author: Josephine Qualls
*    Contributors: Trinity Hutson
*    Date Created: 01/28/2025
*    Description: Generates buttons for every level in the game.
*******************************************************************/

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelButtons : MonoBehaviour
{
    //Button in charge of levels
    [SerializeField] private GameObject _buttonPrefab;
    

    /// <summary>
    /// At run-time, buttons equal to the number of scenes are generated.
    /// </summary>
    void Start()
    {
        //cycles through all loaded scenes except main menu
        for (int i = 1; i < sceneNum(); i++)
        {
            //loads scene when button is clicked
            IndividualButtons obj = Instantiate(_buttonPrefab, transform).GetComponent<IndividualButtons>();
            obj.GetComponentInChildren<TextMeshProUGUI>().text = "Level " + i;
            obj.setIndex(i);
        }
        
    }

    /// <summary>
    /// Gives the number of scenes in the build.
    /// </summary>
    /// <returns></returns>
    private int sceneNum()
    {
        return SceneManager.sceneCountInBuildSettings;
    }

}
