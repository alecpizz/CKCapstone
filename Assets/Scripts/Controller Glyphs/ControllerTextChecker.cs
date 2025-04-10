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
using UnityEngine.InputSystem.XR;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore;

public class ControllerTextChecker : MonoBehaviour
{
    private Scene currentScene;
    [SerializeField] private TextMeshProUGUI _tutorialText;
    [SerializeField] private TextMeshProUGUI _howToPlayText;
    [SerializeField] public string _keyboardText;
    [SerializeField] public string _controllerText; //Text for Xbox/Generic Controllers
    [SerializeField] public string _playstationText; //Text for PS Controllers
    [SerializeField] public string _switchText; //Text for Switch Pro Controllers

    /// <summary>
    /// Start is called before the first frame update
    /// grabs the current scene as a reference and changes the text in the scene if needed
    /// </summary>
    private void Start()
    {
        currentScene = SceneManager.GetActiveScene();
        TutorialTextChange();
        HowToPlayTextChange();
    }

    /// <summary>
    /// Handles changing the tutorial text depending on if a controller is connected or not
    /// </summary>
    public void TutorialTextChange()
    {
        _tutorialText.text = ControllerGlyphManager.Instance.TutorialText();
    }
    public void HowToPlayTextChange()
    {
        _howToPlayText.text = ControllerGlyphManager.Instance.HowToPlayText();
    }
}
