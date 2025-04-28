/******************************************************************
*    Author: Rider Hagen
*    Contributors: David Galmines, Alec Pizziferro
*    Date Created: 9/26/24
*    Description: This is just meant to make menu buttons, when pressed, work.
*******************************************************************/
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using SaintsField;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.DebugUI;
using System;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject _pauseScreen;
    [SerializeField] private GameObject _tutorialCanvas;
    [SerializeField] private GameObject _optionsScreen;
    [SerializeField] private GameObject _confirmQuit;
    [SerializeField] private GameObject _mainMenu;
    [Scene] [SerializeField] private int _firstLevelIndex = 1;
    [SerializeField] private GameObject _mainMenuFirst;
    [SerializeField] private GameObject _settingsMenuFirst;
    [SerializeField] private GameObject _mainMenuStart;
    [SerializeField] private GameObject _mainMenuSettings;
    [SerializeField] private GameObject _mainMenuQuit;
    [SerializeField] private GameObject _restartButton;
    [SerializeField] private GameObject _tempCredits;
    
    [SerializeField] private EventReference _buttonPress;

    private DebugInputActions _inputActions;

    [SerializeField] private CursorManager _cursorManager;

    [FormerlySerializedAs("_skipWhilePaused")]
    [SerializeField] private GameObject _skipPromptInPause;
    private bool _skipInPause;

    private bool _pauseInvoked = false;
    private bool _isMainMenu;
    /// <summary>
    /// Enables player input for opening the pause menu
    /// </summary>
    private void Awake()
    {
        _inputActions = new DebugInputActions();
        _inputActions.Enable();
        _inputActions.Player.Quit.performed += ctx => Pause();
        _inputActions.UI.Back.performed += BackPerformed;

        //Gets rid of restart button if it's a cutscene
        string path = SceneManager.GetActiveScene().path;
        if (path.Contains("CS"))
        {
            _restartButton.SetActive(false);
        }

        //TODO: adjust this whenever we fix duplicate main menu in settings..
        _isMainMenu = SceneManager.GetActiveScene().buildIndex == 0 
                      || SceneManager.GetActiveScene().buildIndex == SceneManager.sceneCountInBuildSettings - 1;
    }

    /// <summary>
    /// Disables player input for opening the pause menu
    /// </summary>
    private void OnDisable()
    {
        _inputActions.Disable();
        _inputActions.Player.Quit.performed -= ctx => Pause();
        _inputActions.UI.Back.performed -= BackPerformed;
    }

    /// <summary>
    /// Invoked to close the pause menu
    /// </summary>
    public void Unpause()
    {
        DebugMenuManager.Instance.PauseMenu = false;
        _pauseScreen.SetActive(false);

        //gets rid of restart icon in cutscenes
        string path = SceneManager.GetActiveScene().path;
        if (path.Contains("CS"))
        {
            _restartButton.SetActive(false);
        }
        else
        {
            _restartButton.SetActive(true);
        }
        _cursorManager.OnPointerExit();
        Time.timeScale = 1f;
        _pauseInvoked = false;
    }

    /// <summary>
    /// Invoked to open the options menu in the main menu
    /// </summary>
    public void OptionsMainMenu()
    {
        _optionsScreen.SetActive(true);
        _mainMenu.SetActive(false);
        CollectableManager.Instance.SetFoundCollectibles();
        EventSystem.current.SetSelectedGameObject(_settingsMenuFirst);
    }

    /// <summary>
    /// Invoked to close the options menu on the main menu
    /// </summary>
    public void OptionsMainMenuClose()
    {
        _optionsScreen.SetActive(false);
        _mainMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(_mainMenuFirst);
    }

    /// <summary>
    /// Invoked to open the options menu in the pause menu
    /// </summary>
    public void Options()
    {
        _optionsScreen.SetActive(true);
        _mainMenuStart.SetActive(false);
        _mainMenuSettings.SetActive(false);
        _mainMenuQuit.SetActive(false);
        CollectableManager.Instance.SetFoundCollectibles();
        EventSystem.current.SetSelectedGameObject(_settingsMenuFirst);
    }


    /// <summary>
    /// Invoked to close the options menu
    /// </summary>
    public void OptionsClose()
    {
        _optionsScreen.SetActive(false);
        _mainMenuStart.SetActive(true);
        _mainMenuSettings.SetActive(true);
        _mainMenuQuit.SetActive(true);
        EventSystem.current.SetSelectedGameObject(_mainMenuFirst);
    }

    /// <summary>
    /// Invoked to open the pause menu
    /// </summary>
    public void Pause()
    {
        if (_pauseScreen != null && !_pauseScreen.activeInHierarchy)
        {
            if (SceneController.Instance == null || SceneController.Instance.Transitioning) return;
            DebugMenuManager.Instance.PauseMenu = true;
            _pauseScreen.SetActive(true);
            _restartButton.SetActive(false);
            EventSystem.current.SetSelectedGameObject(_mainMenuFirst);
            Time.timeScale = 0f;

            _pauseInvoked = true;

            //Gets the path to a scene
            string path = SceneManager.GetActiveScene().path;

            if (path.Contains("CS"))
            {
                _skipPromptInPause.SetActive(true);
            }
        }
        else if (_optionsScreen != null && _optionsScreen.activeInHierarchy)
        {
            OptionsClose();
            _mainMenu.SetActive(true);
        }
        else if (_pauseScreen != null && _pauseScreen.activeInHierarchy)
        {
            Unpause();
        }
    }

    /// <summary>
    /// Back button based exiting pause menu.
    /// </summary>
    /// <param name="ctx">Callback ctx. Unused.</param>
    private void BackPerformed(InputAction.CallbackContext ctx)
    {
        //options menu is open, let's go back to the pause screen rather than the game for clarity
        if (_optionsScreen != null && _optionsScreen.activeSelf)
        {
            //Kinda dumb that we have two methods for this...
            if (_isMainMenu)
            {
                OptionsMainMenuClose();
            }
            else
            {
                OptionsClose();
            }
        }
        //pause menu is open, go back to the game.
        else if (_pauseScreen != null && _pauseScreen.activeInHierarchy)
        {
            Unpause();
        }
    }

    /// <summary>
    /// Getter method to tell if the game is paused for FMOD audio
    /// </summary>
    /// <returns></returns>
    public bool GetPauseInvoked()
    {
        return _pauseInvoked;
    }

    /// <summary>
    /// Invoked for pause menu to return to the base main menu scene
    /// </summary>
    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// Clicking the start button on the main menu activates the tutorial canvas.
    /// </summary>
    public void ActivateTutorialCanvas()
    {
         _tutorialCanvas.SetActive(true);
    }

    /// <summary>
    /// Clicking the button on the tutorial screen deactivates the tutorial canvas.
    /// </summary>
    public void DeactivateTutorialCanvas()
    {
        _tutorialCanvas.SetActive(false);
    }

    /// <summary>
    /// Loads the first scene when a button is pressed
    /// </summary>
    public void StartGame()
    {
        SceneManager.LoadScene(_firstLevelIndex);
        SaveDataManager.SetLastFinishedLevel(SceneUtility.GetScenePathByBuildIndex(_firstLevelIndex));
    }

    /// <summary>
    /// Attempts to load the last saved puzzle.
    /// </summary>
    public void ContinueGame()
    {
        string level = SaveDataManager.GetLastFinishedLevel();
        Debug.Log($"trying to continue {level}");
        if (string.IsNullOrEmpty(level))
        {
            Debug.Log($"Last Level not saved");
            StartGame();
            return;
        }

        int idx = SceneUtility.GetBuildIndexByScenePath(level);
        SceneManager.LoadScene(idx);
    }

    /// <summary>
    /// Prompts the user with a quit confirm selection after pressing the quit button
    /// </summary>
    public void QuitConfirm()
    {
        _confirmQuit.SetActive(true);
    }

    /// <summary>
    /// Closes quit confirm
    /// </summary>
    public void QuitDecline()
    {
        _confirmQuit.SetActive(false);
    }

    /// <summary>
    /// Invoked to close project
    /// </summary>
    public void Quit()
    {
        Application.Quit();
    }

    /// <summary>
    /// Opens credits
    /// </summary>
    public void Credits()
    {
        _tempCredits.SetActive(true);

        //SceneManager.LoadScene("Credits");
    }

    /// <summary>
    /// closes credits
    /// </summary>
    public void CreditsClose()
    {
        _tempCredits.SetActive(false);
    }
}
