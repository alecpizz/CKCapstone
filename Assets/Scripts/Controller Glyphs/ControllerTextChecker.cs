/******************************************************************
*    Author: Alex Laubenstein
*    Contributors: Alex Laubenstein
*    Date Created: April 1st, 2025
*    Description: This script is what changes the text in levels of 
*    the game depending on what input device is currently being used
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ControllerTextChecker : MonoBehaviour
{
    private Scene currentScene;
    [SerializeField] private TextMeshProUGUI _tutorialText;
    [SerializeField] private string _keyboardText;
    [SerializeField] private string _controllerText; //Text for Xbox/Generic Controllers
    [SerializeField] private string _playstationText; //Text for PS Controllers
    [SerializeField] private string _switchText; //Text for Switch Pro Controllers

    /// <summary>
    /// Start is called before the first frame update
    /// grabs the current scene as a reference and changes the text in the scene if needed
    /// </summary>
    private void Start()
    {
        currentScene = SceneManager.GetActiveScene();
        TutorialTextChange();
    }

    /// <summary>
    /// Handles changing the tutorial text depending on if a controller is connected or not
    /// </summary>
    public void TutorialTextChange()
    {
        if (currentScene.name == "T_FirstDay") //Movement tutorial
        {
            if (ControllerGlyphManager.Instance.KeyboardAndMouse)
            {
                _tutorialText.text = _keyboardText;
            }
            else
            {
                _tutorialText.text = _controllerText;
            }
        }
        if (currentScene.name == "T_SteerClear_Sp") // Enemy path toggle tutorial
        {
            if (ControllerGlyphManager.Instance.KeyboardAndMouse)
            {
                _tutorialText.text = _keyboardText;
            }
            else if (ControllerGlyphManager.Instance.SwitchController)
            {
                _tutorialText.text = _switchText;
            }
            else if (ControllerGlyphManager.Instance.PlayStationController)
            {
                _tutorialText.text = _playstationText;
            }
            else
            {
                _tutorialText.text = _controllerText;
            }
        }
        else
        {
            return;
        }
    }
}
