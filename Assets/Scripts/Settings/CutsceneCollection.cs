/******************************************************************
*    Author: Mitchell Young
*    Contributors: Josephine Qualls
*    Date Created: 3/25/25
*    Description: Loads cutscenes based on button pressed. Button
*    cannot be pressed if the cutscene has not been seen before.
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.EventSystems;
using TMPro;

public class CutsceneCollection : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private bool unlocked = false;
    [SerializeField] private string cutsceneName = "";
    [SerializeField] private GameObject _nameOfCutscene;

    /// <summary>
    /// Sets unlocked to true if the cutscene has been seen
    /// </summary>
    private void Start()
    {
        if (SaveDataManager.GetLevelCompleted(cutsceneName))
        {
            unlocked = true;
        }
        if (unlocked)
        {
            gameObject.GetComponent<Image>().color = new Color(1, 1, 1, 1);
        }
        else
        {
            gameObject.GetComponent<Image>().color = new Color(0, 0, 0, 1);
        }
        
        //For the beginning of the hover text behavior
        TextMeshProUGUI cutsceneNameText = _nameOfCutscene.GetComponent<TextMeshProUGUI>();
        cutsceneNameText.text = LevelName();

        if (_nameOfCutscene != null)
        {
            _nameOfCutscene.SetActive(false);
        }

    }

    /// <summary>
    /// Provides the correct name for the cutscenes
    /// </summary>
    /// <returns></returns>
    private string LevelName()
    {
        var levelData = LevelOrderSelection.Instance.SelectedLevelData;

        string name = "";

        foreach (var levelDataChapter in levelData.Chapters)
        {
            foreach (var level in levelDataChapter.Puzzles)
            {
                var sceneName = Path.GetFileNameWithoutExtension(level.ScenePath);
                if (sceneName.Equals(cutsceneName))
                {
                    name = level.LevelName;
                }
            }

            var outroName = Path.GetFileNameWithoutExtension(levelDataChapter.Outro.ScenePath);
            if (outroName.Equals(cutsceneName))
            {
                name = levelDataChapter.Outro.LevelName;
                
            }

        }

        if (Path.GetFileNameWithoutExtension(levelData.MainMenuScene.name).Equals(cutsceneName))
        {
            name = "Intro";
        }

        return name;
    }

    /// <summary>
    /// Loads the cutscene based on build index value assigned to the button
    /// </summary>
    public void LoadCutscene()
    {
        if (!unlocked) 
        {
            return;
        }
        SaveDataManager.SetLoadedFromPause(true);
        SaveDataManager.SetSceneLoadedFrom(SceneManager.GetActiveScene().name);

        SceneManager.LoadScene(cutsceneName);
    }

    /// <summary>
    /// Activates the hover text
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_nameOfCutscene != null)
        {
            _nameOfCutscene.SetActive(true);
        }
    }

    /// <summary>
    /// Deactivates the hover text
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (_nameOfCutscene != null)
        {
            _nameOfCutscene.SetActive(false);
        }
    }

    /// <summary>
    /// Activates hover text for controllers
    /// </summary>
    /// <param name="eventData"></param>
    public void OnSelect(BaseEventData eventData)
    {
        if(EventSystem.current.currentSelectedGameObject != null)
        {
            _nameOfCutscene.SetActive(true);
        }
    }

    /// <summary>
    /// Deactivates hover text for controllers
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDeselect(BaseEventData eventData)
    {
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            _nameOfCutscene.SetActive(false);
        }
    }
}
