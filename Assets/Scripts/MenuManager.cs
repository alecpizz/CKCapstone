/******************************************************************
*    Author: Rider Hagen
*    Contributors: David Galmines
*    Date Created: 9/26/24
*    Description: This is just meant to make menu buttons, when pressed, work.
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject _pauseScreen;
    [SerializeField] private GameObject _tutorialCanvas;
    [SerializeField] private GameObject _optionsScreen;

    private DebugInputActions _inputActions;

    private void Awake()
    {
        _inputActions = new DebugInputActions();
        _inputActions.Enable();
        _inputActions.Player.Quit.performed += ctx => Pause();
    }

    private void OnDisable()
    {
        _inputActions.Disable();
        _inputActions.Player.Quit.performed -= ctx => Pause();
    }

    /// <summary>
    /// Invoked to close the pause menu
    /// </summary>
    public void Unpause()
    {
        _pauseScreen.SetActive(false);
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Invoked to open the options menu
    /// </summary>
    public void Options()
    {
        _optionsScreen.SetActive(true);
    }

    /// <summary>
    /// Invoked to close the options menu
    /// </summary>
    public void OptionsClose()
    {
        _optionsScreen.SetActive(false);
    }

    /// <summary>
    /// Invoked to open the pause menu
    /// </summary>
    public void Pause()
    {
        _pauseScreen.SetActive(true);
        Time.timeScale = 0f;
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

    public void DeactivateTutorialCanvas()
    {
        _tutorialCanvas.SetActive(false);
    }

    /// <summary>
    /// Loads the first scene when a button is pressed
    /// </summary>
    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    /// <summary>
    /// Invoked to close project
    /// </summary>
    public void Quit()
    {
        Application.Quit();
    }
}
