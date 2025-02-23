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
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LevelButtons : MonoBehaviour
{
    //Button in charge of levels
    [SerializeField] private GameObject _buttonPrefab;

    //the number of levels that a chapter contains
    [FormerlySerializedAs("_numLevels")]
    [SerializeField] private int[] _chapterLevelCount;

    //List of chapters in game
    [FormerlySerializedAs("_levels")]
    [SerializeField] private List<GameObject> _chapters;

    //buttons to each chapter in game
    [SerializeField] private List<Button> _allChapButtons;

    /// <summary>
    /// At run-time, buttons equal to the number of scenes are generated.
    /// </summary>
    private void Start()
    {
        //variables to help keep the for loops on track
        int tally = 1;
        int range = _chapterLevelCount[0] + 1;

        //cycles through all chapters and assigns levels (if they exist in build)
        for (int i = 1; i <= _chapters.Count; i++)
        {
            //initially sets the gameObjects to off
            _chapters[i-1].SetActive(false);

            for (; tally < range; tally++)
            {
                string name = LevelOrderSelection.Instance.SelectedLevelData.PrettySceneNames[tally].PrettyName;
                bool shown = LevelOrderSelection.Instance.SelectedLevelData.PrettySceneNames[tally].showUp;

                //loads scene when button is clicked
                if (!shown)
                {
                    continue;
                }
                
                IndividualButtons obj = Instantiate(_buttonPrefab, _chapters[i-1].transform).GetComponent<IndividualButtons>();

                Debug.Log(SceneManager.GetSceneByBuildIndex(tally).name);

                if (SceneManager.GetSceneByBuildIndex(tally).name[0] == 'I')
                {
                    obj.GetComponentInChildren<TextMeshProUGUI>().text = name;
                    obj.SetIndex(tally);
                }
                else
                {
                    obj.GetComponentInChildren<TextMeshProUGUI>().text = "Level: " + tally.ToString();
                    obj.SetIndex(tally);
                }      
                
            }

            if(i < _chapterLevelCount.Length)
                range += _chapterLevelCount[i];
        }

        //loops through buttons to change the respective GameObject
        for(int j = 0; j < _allChapButtons.Count; j++)
        {
            int jCopy = j;
            _allChapButtons[j].onClick.AddListener(() => ActivateButton(jCopy));
        }
    }

    /// <summary>
    /// Sets a game object to true if the button is pressed, and everything else stays false.
    /// </summary>
    /// <param name="num"></param>
    private void ActivateButton(int num)
    {
        for (int i = 0; i < _chapters.Count; i++)
        {
            _chapters[i].SetActive(i == num);
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
