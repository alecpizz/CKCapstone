using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    /******************************************************************
*    Author: Rider Hagen
*    Contributors: [people who edited file, add your name if you edit]
*    Date Created: 9/26/24
*    Description: This is just meant to make menu buttons, when pressed, work.
*******************************************************************/

    public GameObject PauseScreen;

    public void Unpause()
    {
        PauseScreen.SetActive(false);
    }

    public void Pause()
    {
        PauseScreen.SetActive(true);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
