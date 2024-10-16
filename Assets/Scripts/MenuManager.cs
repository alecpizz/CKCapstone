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

    /// <summary>
    /// Invoked to close the pause menu
    /// </summary>
    public void Unpause()
    {
        _pauseScreen.SetActive(false);
    }

    /// <summary>
    /// Invoked to open the pause menu
    /// </summary>
    public void Pause()
    {
        _pauseScreen.SetActive(true);
    }

    /// <summary>
    /// Invoked for pause menu to return to the base main menu scene
    /// </summary>
    public void BackToMainMenu()
    {
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
