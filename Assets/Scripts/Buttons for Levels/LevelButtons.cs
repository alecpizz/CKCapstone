/******************************************************************
*    Author: Josephine Qualls
*    Contributors: Trinity Hutson, Alex Laubenstein
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

    //the number of levels that a chapter contains
    [SerializeField] private int _numLevels = 10;

    //List of chapters in game
    [SerializeField] private List<GameObject> _levels;

    //buttons to each chapter in game
    [SerializeField] private Button _lvl1Button;
    [SerializeField] private Button _lvl2Button;
    [SerializeField] private Button _lvl3Button;
    [SerializeField] private Button _lvl4Button;
    [SerializeField] private Button _lvl5Button;

    /// <summary>
    /// At run-time, buttons equal to the number of scenes are generated.
    /// </summary>
    private void Start()
    {
        //variables to help keep the for loops on track
        int tally = 1;
        int lvlIncrease = 10;
        int range = _numLevels + 1;

        //cycles through all chapters and assigns levels (if they exist in build)
        for (int i = 1; i <= 5; i++)
        {
            //initially sets the gameObjects to off
            _levels[i-1].SetActive(false);

            for (; tally < range; tally++)
            {
                //loads scene when button is clicked
                IndividualButtons obj = Instantiate(_buttonPrefab, _levels[i-1].transform).GetComponent<IndividualButtons>();
                obj.GetComponentInChildren<TextMeshProUGUI>().text = "Level " + tally;
                obj.SetIndex(tally);
            }
            range += lvlIncrease;
        }

        //buttons change their respective GameObjects
        _lvl1Button.onClick.AddListener(() => ActivateButton(0));
        _lvl2Button.onClick.AddListener(() => ActivateButton(1));
        _lvl3Button.onClick.AddListener(() => ActivateButton(2));
        _lvl4Button.onClick.AddListener(() => ActivateButton(3));
        _lvl5Button.onClick.AddListener(() => ActivateButton(4));
    }

    /// <summary>
    /// Sets a game object to true if the button is pressed, and everything else stays false.
    /// </summary>
    /// <param name="num"></param>
    private void ActivateButton(int num)
    {
        
        foreach(GameObject obj in _levels)
        {
            if(obj != _levels[num])
            {
                obj.SetActive(false);
            }
            else
            {
                obj.SetActive(true);
            }
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
