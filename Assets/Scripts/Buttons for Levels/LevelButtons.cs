/******************************************************************
 *    Author: Josephine Qualls
 *    Contributors: Trinity Hutson, Alex Laubenstein
 *    Date Created: 01/28/2025
 *    Description: Generates buttons for every level in the game.
 *******************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LevelButtons : MonoBehaviour
{
    //Button with levels and cutscenes assigned to them
    [SerializeField] private GameObject _buttonPrefab;

    //the number of levels that a chapter contains
    [FormerlySerializedAs("_numLevels")] [SerializeField]
    private int[] _chapterLevelCount;

    //List of chapters in game
    [FormerlySerializedAs("_levels")] [SerializeField]
    private List<GameObject> _chapters;

    //List of all buttons to each chapter in game
    [SerializeField] private List<Button> _allChapButtons;

    //Prefix word of a level
    [SerializeField] private string _levelPrefix = "Level: ";

    [SerializeField] private string _challengeText = "Challenge";

    //Counter for the levels
    private int _lvlCounter = -1;

    //Stores the true buildIndex and the level select number
    private Dictionary<int, int> _lvlNumberAssignment = new();

    //Call to update the dictionary for UIManager

    /// <summary>
    /// At run-time, buttons equal to the number of scenes are generated.
    /// </summary>
    private void Awake()
    {
        var uiManager = GetComponentInParent<UIManager>();
        foreach (var chapter in _chapters)
        {
            chapter.SetActive(false);
        }

        var levelData = LevelOrderSelection.Instance.SelectedLevelData;
        int counter = 1;
        int challengeCounter = 1;
        int intermissionCounter = 1;
        var currentScene = SceneUtility.GetScenePathByBuildIndex(SceneManager.GetActiveScene().buildIndex);
        bool setLevel = false;
        foreach (var levelDataChapter in levelData.Chapters)
        {
            var idx = levelData.Chapters.IndexOf(levelDataChapter);
            var chapter = _chapters[idx];
            foreach (var level in levelDataChapter.Puzzles)
            {
                var targetTransform = chapter.transform;
                string levelText = $"Level {counter}";
                var sceneName = Path.GetFileNameWithoutExtension(level.ScenePath);
                if (sceneName.Contains("CS"))
                {
                    targetTransform = _chapters[^1].transform;
                    levelText = level.LevelName;
                }
                else if (sceneName[0] == 'I')
                {
                    levelText = $"Intermission {intermissionCounter++}";
                }
                else if (sceneName[0] == 'C')
                {
                    levelText = $"Challenge {challengeCounter++}";
                }
                else
                {
                    counter++;
                }
#if !OVERRIDE_LEVEL
                if (!SaveDataManager.GetLevelCompleted(level.ScenePath))
                {
                    if (uiManager != null && !setLevel)
                    {
                        if (currentScene == level.ScenePath)
                        {
                            uiManager.SetLevelText(levelText);
                            setLevel = true;
                        }
                    }
                    continue;
                }
#endif
                var obj = Instantiate(_buttonPrefab, targetTransform).GetComponent<IndividualButtons>();
                obj.SetIndex(level.ScenePath);
                obj.GetComponentInChildren<TextMeshProUGUI>().text = levelText;
                if (uiManager != null && !setLevel)
                {
                    if (currentScene == level.ScenePath)
                    {
                        uiManager.SetLevelText(levelText);
                        setLevel = true;
                    }
                }
            }

            var outro = levelDataChapter.Outro;
#if !OVERRIDE_LEVEL
            if (!SaveDataManager.GetLevelCompleted(outro.ScenePath))
            {
                continue;
            }
#endif
            var sceneName2 = Path.GetFileNameWithoutExtension(outro.ScenePath);
            if (sceneName2.Contains("CS"))
            {
                var obj = Instantiate(_buttonPrefab, _chapters[^1].transform).GetComponent<IndividualButtons>();
                obj.SetIndex(outro.ScenePath);
                obj.GetComponentInChildren<TextMeshProUGUI>().text = outro.LevelName;
            }
        }

        for (int i = 0; i < _allChapButtons.Count; i++)
        {
            int jCopy = i;
            _allChapButtons[i].onClick.AddListener(() => ActivateButton(jCopy));
        }

        return;
        //variables to track the level number and how many are in the chapter
        int tally = 1;
        int range = _chapterLevelCount[0] + 1;

        //cycles through all chapters and assigns levels/cutscenes (if they exist in build)
        for (int i = 1; i <= _chapters.Count; i++)
        {
            //initially sets the gameObjects to off
            _chapters[i - 1].SetActive(false);

            for (; tally < range; tally++)
            {
                //for keeping the level numbers consistent
                _lvlCounter++;

                //The nicer name assigned to a level and whether they should be assigned to a button
                string name = LevelOrderSelection.Instance.SelectedLevelData.PrettySceneNames[tally].PrettyName;
                bool shown = LevelOrderSelection.Instance.SelectedLevelData.PrettySceneNames[tally].showUp;

                //Gets the path to a scene
                string path = SceneUtility.GetScenePathByBuildIndex(tally);
                //Uses the path to get the full name of the scene in the build
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);

                //loads scene when button is clicked
                if (!shown)
                {
                    continue;
                }

#if !OVERRIDE_LEVEL
                if (!SaveDataManager.GetLevelCompleted(path))
                {
                    continue;
                }
#endif
                //initiate obj variable
                IndividualButtons obj;

                //places buttons in appropriate tab
                if (sceneName.Contains("Cutscene"))
                {
                    //the cutscene tab should always be the last chapter tab
                    obj = Instantiate(_buttonPrefab, _chapters[_chapters.Count - 1].transform)
                        .GetComponent<IndividualButtons>();
                }
                else
                {
                    //the levels/NPC rooms are assigned to the relevant chapters
                    obj = Instantiate(_buttonPrefab, _chapters[i - 1].transform).GetComponent<IndividualButtons>();
                }

                //renames NPC room and cutscenes to official names in Level Select menu
                if (sceneName[0] == 'I' || sceneName.Contains("Cutscene"))
                {
                    obj.GetComponentInChildren<TextMeshProUGUI>().text = name;
                    _lvlCounter--;
                }
                else if (sceneName.Contains("C_"))
                {
                    //Challenges have been changed to just say challenge
                    obj.GetComponentInChildren<TextMeshProUGUI>().text = _challengeText;
                    _lvlCounter--;
                }
                else
                {
                    //Levels are named Level + relevant number
                    obj.GetComponentInChildren<TextMeshProUGUI>().text = _levelPrefix + (_lvlCounter).ToString();
                }

                //The index of a button is set
                // obj.SetIndex(tally);

                //Add to the dictionary for getter method
                _lvlNumberAssignment.Add(tally, _lvlCounter);
            }

            if (i < _chapterLevelCount.Length)
            {
                range += _chapterLevelCount[i];
            }

            //to not include transitions
            _lvlCounter--;
        }


        //loops through buttons to change the respective GameObject
        for (int j = 0; j < _allChapButtons.Count; j++)
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
    /// Returns the current level number
    /// </summary>
    /// <returns></returns>
    public int GetLvlCounter(int index)
    {
        if (_lvlNumberAssignment.ContainsKey(index))
        {
            return _lvlNumberAssignment[index];
        }
        else
        {
            return -1;
        }
    }
}