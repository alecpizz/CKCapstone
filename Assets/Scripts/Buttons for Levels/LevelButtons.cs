/******************************************************************
*    Author: Josephine Qualls
*    Contributors: Trinity Hutson
*    Date Created: 01/28/2025
*    Description: Generates buttons for every level in the game.
*******************************************************************/

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelButtons : MonoBehaviour
{
    //Button in charge of levels
    [SerializeField] private GameObject _buttonPrefab;
    [SerializeField] private int _numLevels = 11;

    
    /// <summary>
    /// At run-time, buttons equal to the number of scenes are generated.
    /// </summary>
    private void Start()
    {
        int tally = 1;
        int lvlIncrease = 10;
        int range = _numLevels;

        //cycles through all loaded scenes except main menu
        for (int i = 1; i <= 5; i++)
        {
            for (; tally < range; tally++)
            {
                //loads scene when button is clicked
                IndividualButtons obj = Instantiate(_buttonPrefab, transform).GetComponent<IndividualButtons>();
                obj.GetComponentInChildren<TextMeshProUGUI>().text = "Level " + tally;
                obj.SetIndex(tally);
                Debug.Log(tally);
            }
            range += lvlIncrease;
        }
    }

    /// <summary>
    /// Gives the number of scenes in the build.
    /// </summary>
    /// <returns></returns>
    private int SceneNum()
    {
        return SceneManager.sceneCountInBuildSettings;
    }
}
