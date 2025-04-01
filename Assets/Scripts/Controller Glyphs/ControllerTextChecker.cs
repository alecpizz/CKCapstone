
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ControllerTextChecker : MonoBehaviour
{
    public static ControllerTextChecker Instance { get; private set; }

    private Scene currentScene;
    [SerializeField] private TextMeshProUGUI _tutorialText;
    [SerializeField] private string _keyboardText;
    [SerializeField] private string _controllerText; //Text for Xbox/Generic Controllers
    [SerializeField] private string _playstationText; //Text for PS Controllers
    [SerializeField] private string _switchText; //Text for Switch Pro Controllers

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        currentScene = SceneManager.GetActiveScene();
        TutorialTextChange();
    }

    /// <summary>
    /// Handles changing the tutorial text depending on if a controller is connected or not
    /// </summary>
    public void TutorialTextChange()
    {
        if (currentScene.name == "T_FirstDay")
        {
            if (DebugMenuManager.Instance.KeyboardAndMouse)
            {
                _tutorialText.text = _keyboardText;
            }
            else
            {
                _tutorialText.text = _controllerText;
            }
        }
        if (currentScene.name == "T_SteerClear_Sp")
        {
            if (DebugMenuManager.Instance.KeyboardAndMouse)
            {
                _tutorialText.text = _keyboardText;
            }
            else if (DebugMenuManager.Instance.SwitchController)
            {
                _tutorialText.text = _switchText;
            }
            else if (DebugMenuManager.Instance.PlayStationController)
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
