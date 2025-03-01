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
                //Make more thorough comments
                string name = LevelOrderSelection.Instance.SelectedLevelData.PrettySceneNames[tally].PrettyName;
                bool shown = LevelOrderSelection.Instance.SelectedLevelData.PrettySceneNames[tally].showUp;

                //getting the scene name through its path
                string path = SceneUtility.GetScenePathByBuildIndex(tally);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);

                //loads scene when button is clicked
                if (!shown)
                {
                    continue;
                }

                //initiate obj variable
                IndividualButtons obj;

                //places buttons in appropriate tab
                if (sceneName.Contains("Cutscene"))
                {
                    obj = Instantiate(_buttonPrefab, _chapters[_chapters.Count-1].transform).GetComponent<IndividualButtons>();
                }
                else
                {
                    obj = Instantiate(_buttonPrefab, _chapters[i - 1].transform).GetComponent<IndividualButtons>();
                }
                
                //renames NPC room if needed
                if (sceneName[0] == 'I' || sceneName.Contains("Cutscene"))
                {
                    obj.GetComponentInChildren<TextMeshProUGUI>().text = name;
                    obj.SetIndex(tally);
                }
                else
                {
                    obj.GetComponentInChildren<TextMeshProUGUI>().text = "Level: " + (tally-1).ToString();
                    obj.SetIndex(tally);
                }
                //make more thorough comments
            }

            if (i < _chapterLevelCount.Length)
            {
                range += _chapterLevelCount[i];
            }
                
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
