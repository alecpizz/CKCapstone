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

    [SerializeField] private string _challengePrefix = "Challenge";

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
                string levelText = $"{_levelPrefix} {counter}";
                var sceneName = Path.GetFileNameWithoutExtension(level.ScenePath);
                if (sceneName.Contains("CS"))
                {
                    targetTransform = _chapters[^1].transform;
                    levelText = level.LevelName;
                }
                else if (sceneName[0] == 'I' || sceneName[0] == 'N')
                {
                    levelText = $"Intermission {intermissionCounter++}";
                }
                else if (sceneName[0] == 'C')
                {
                    levelText = $"{_challengePrefix} {challengeCounter++}";
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