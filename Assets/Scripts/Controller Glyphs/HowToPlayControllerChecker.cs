/******************************************************************
*    Author: Alex Laubenstein
*    Contributors: Alex Laubenstein
*    Date Created: April 1st, 2025
*    Description: This script is what changes the text in the How To Play
*    section of the settings menu depending on what input device is
*    currently being used
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class HowToPlayControllerChecker : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _howToPlayText;

    [SerializeField] private string _keyboardText;
    [SerializeField] private string _controllerText; //Text for Xbox/Generic Controllers
    [SerializeField] private string _playstationText; //Text for PS Controllers
    [SerializeField] private string _switchText; //Text for Switch Pro Controllers

    // Start is called before the first frame update
    void Start()
    {
        HowToPlayTextChange();
    }

    /// <summary>
    /// Handles changing the tutorial text depending on if a controller is connected or not
    /// </summary>
    public void HowToPlayTextChange()
    {
          _howToPlayText.text = ControllerGlyphManager.Instance.HowToPlayText();
    }
}
