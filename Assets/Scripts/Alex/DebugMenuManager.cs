/******************************************************************
*    Author: Alex Laubenstein
*    Contributors: Alex Laubenstein
*    Date Created: September 24th, 2024
*    Description: This script is what controls the Debug Menu and its
     tools from turning on the game's frame rate counter to turning
     on and off gameplay mechanics and also other game related tools
*******************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.Rendering.UI;

public class DebugMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject _debugMenuFirst;
    [SerializeField] private GameObject _quitMenuFirst;
    public GameObject debugMenu;
    public GameObject quitMenu;
    public GameObject fpsCounter;
    public GameObject ghostModeReminder;
    public GameObject invincibilityReminder;
    public GameObject EnemyTurnReminder;

    private bool dMenu = false;
    private bool qMenu = false;
    private bool fpsCount = false;
    private bool ghostMode = false;
    private bool invincibility = false;
    private bool enemyTurn = true;

    private int lastFrameIndex;
    private float[] frameDeltaTimeArray;

    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private TextMeshProUGUI fpsButtonText;
    [SerializeField] private TextMeshProUGUI ghostModeButtonText;
    [SerializeField] private TextMeshProUGUI invincibilityButtonText;
    [SerializeField] private TextMeshProUGUI enemyTurnButtonText;

    private DebugtInputActions playerInput;
    private InputAction debugInput;
    private InputAction restartInput;
    private InputAction quitInput;

    /// <summary>
    /// Does the calculation for determining the game's frame rate.
    /// Credit: https://www.youtube.com/shorts/I2r97r9h074
    /// </summary>
    private float FPSCalculation()
    {
        float total = 0f;
        foreach (float deltaTime in frameDeltaTimeArray)
        {
            total += deltaTime;
        }
        return frameDeltaTimeArray.Length / total;
    }

    private void Awake()
    {
        //enables player input
        playerInput = new DebugtInputActions();
        debugInput = playerInput.Player.Debug;
        restartInput = playerInput.Player.Restart;
        quitInput = playerInput.Player.Quit;

        //puts multiple frames into an array to slow down the fps counter instead having it change instantaniously
        frameDeltaTimeArray = new float[60];
    }

    /// <summary>
    /// Turns on the Debug Inputs
    /// </summary>
    private void OnEnable()
    {
        debugInput.Enable();
        restartInput.Enable();
        quitInput.Enable();
    }

    /// <summary>
    /// Turns off the Debug Inputs
    /// </summary>
    private void OnDisable()
    {
        debugInput.Disable();
        restartInput.Disable();
        quitInput.Disable();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        EventSystem.current.SetSelectedGameObject(_debugMenuFirst);
    }

    private void Update()
    {
        //handles opening and closing the debug menu
        if (debugInput.WasPressedThisFrame() && dMenu == false)
        {
            OpenDebugMenu();
        }
        else if (debugInput.WasPressedThisFrame() && dMenu == true)
        {
            CloseDebugMenu();
        }

        //restarts the current scene if the restart input is pressed
        if (restartInput.WasPressedThisFrame())
        {
            RestartLevel();
        }

        //handles opening and closing the quiting menu
        if (quitInput.WasPressedThisFrame() && qMenu == false)
        {
            OpenQuitMenu();
        }
        else if (quitInput.WasPressedThisFrame() && qMenu == true)
        {
            CloseQuitMenu();
        }

        //updates the FPS Counter
        frameDeltaTimeArray[lastFrameIndex] = Time.deltaTime;
        lastFrameIndex = (lastFrameIndex + 1) % frameDeltaTimeArray.Length;
        fpsText.text = (Mathf.RoundToInt(FPSCalculation()).ToString() + " FPS");
    }

    /// <summary>
    /// Method to Open the debug menu
    /// </summary>
    public void OpenDebugMenu()
    {
        debugMenu.SetActive(true);
        dMenu = true;
        //Sets the default option for keyboard and controller navigation
        EventSystem.current.SetSelectedGameObject(_debugMenuFirst);
    }

    /// <summary>
    /// Method to Close the Debug Menu
    /// </summary>
    public void CloseDebugMenu()
    {
        debugMenu.SetActive(false);
        dMenu = false;
    }

    /// <summary>
    /// Method to open the menu to quit the game
    /// </summary>
    public void OpenQuitMenu()
    {
        quitMenu.SetActive(true);
        qMenu = true;
        //Sets the default option for keyboard and controller navigation
        EventSystem.current.SetSelectedGameObject(_quitMenuFirst);
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Method to close the menu to quit the game
    /// </summary>
    public void CloseQuitMenu()
    {
        quitMenu.SetActive(false);
        qMenu = false;
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Toggles viewing the FPS Counter
    /// </summary>
    public void FPSCounterToggle()
    {
        if (fpsCount == false)
        {
            fpsCounter.SetActive(true);
            fpsButtonText.text = "FPS Counter: On";
            fpsCount = true;
        }
        else if (fpsCount == true) 
        {
            fpsCounter.SetActive(false);
            fpsButtonText.text = "FPS Counter: Off";
            fpsCount = false;
        }
    }

    /// <summary>
    /// Will toggle ghost mode, currently just displays a notification saying it's on
    /// </summary>
    public void GhostModeToggle()
    {
        if (ghostMode == false)
        {
            ghostModeReminder.SetActive(true);
            ghostModeButtonText.text = "Ghost Mode: On";
            ghostMode = true;
        }
        else if (ghostMode == true)
        {
            ghostModeReminder.SetActive(false);
            ghostModeButtonText.text = "Ghost Mode: Off";
            ghostMode = false;
        }
    }

    /// <summary>
    /// Will toggle player invincibility, currently just displays a notification saying it's on
    /// </summary>
    public void InvincibilityToggle()
    {
        if (invincibility == false)
        {
            invincibilityReminder.SetActive(true);
            invincibilityButtonText.text = "Invincibility: On";
            invincibility = true;
        }
        else if (invincibility == true)
        {
            invincibilityReminder.SetActive(false);
            invincibilityButtonText.text = "Invincibility: Off";
            invincibility = false;
        }
    }

    /// <summary>
    /// Will toggle turning off enemy turns, currently just displays a notification saying enemy turns are off
    /// </summary>
    public void EnemyTurnToggle()
    {
        if (enemyTurn == true)
        {
            EnemyTurnReminder.SetActive(true);
            enemyTurnButtonText.text = "Enemy Turns: Off";
            enemyTurn = false;
        }
        else if (enemyTurn == false)
        {
            EnemyTurnReminder.SetActive(false);
            enemyTurnButtonText.text = "Enemy Turns: On";
            enemyTurn = true;
        }
    }

    /// <summary>
    /// Restarts the current scene to restart a level
    /// </summary>
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Sets up scene navigation for changing levels or just loading specific scenes.
    /// The sceneID is taken in as an int to allow the code to go to any specified scene
    /// </summary>
    public void SceneChange(int sceneID)
    {
        Time.timeScale = 1f;
        EventSystem.current.SetSelectedGameObject(null);
        SceneManager.LoadScene(sceneID);
    }

    /// <summary>
    /// Quits out of the game
    /// </summary>
    public static void QuitGame()
    {
        EventSystem.current.SetSelectedGameObject(null);
        Application.Quit();
        /*if (Application.isEditor)
        {
         //   UnityEditor.EditorApplication.isPlaying = false;
        }
        else
        {
            Application.Quit();
        }*/

    }
}
