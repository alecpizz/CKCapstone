/******************************************************************
*    Author: Alex Laubenstein
*    Contributors: Alex Laubenstein
*    Date Created: September 24th, 2024
*    Description: This script is what controls the Debug Menu and its
     tools from turning on the game's frame rate counter to turning
     on and off gameplay mechanics and also other game related tools
*******************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using TMPro;
using UnityEngine.Rendering.UI;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Switch;
using UnityEngine.InputSystem.UI;

public class DebugMenuManager : MonoBehaviour
{
    public static DebugMenuManager Instance { get; private set; }

    public bool GhostMode { get; private set; } = false;
    public bool Invincibility { get; private set; } = false;
    public bool PauseMenu { get; set; } = false;

    [SerializeField] private GameObject _debugMenuFirst;
    [SerializeField] private GameObject _mainMenuFirst;
    [SerializeField] private GameObject _quitMenuFirst;
    [SerializeField] private GameObject _puzzleSelectFirst;
    [SerializeField] private GameObject _settingsFirst;
    [SerializeField] private GameObject _debugMenu;
    [SerializeField] private GameObject _quitMenu;
    [SerializeField] private GameObject _puzzleSelectMenu;
    [SerializeField] private GameObject _fpsCounter;
    //reminder variables show the player that debug functions are on in the top right corner
    [SerializeField] private GameObject _ghostModeReminder;
    [SerializeField] private GameObject _invincibilityReminder;

    [SerializeField] private TextMeshProUGUI _fpsText;
    [SerializeField] private TextMeshProUGUI _fpsButtonText;
    [SerializeField] private TextMeshProUGUI _ghostModeButtonText;
    [SerializeField] private TextMeshProUGUI _invincibilityButtonText;

    private bool _dMenu = false;
    private bool _qMenu = false;
    private bool _pMenu = false;
    private bool _fpsCount = false;
    private bool _sceneNameVisible = false;

    private const string MainMenuSceneName = "MainMenutest";

    private int _lastFrameIndex;
    private float[] _frameDeltaTimeArray;

    private DebugInputActions _playerInput;
    private PlayerControls _playerControls;
    private DefaultInputActions _defaultControls;
    private InputAction _debugInput;
    private InputAction _restartInput;


    /// <summary>
    /// Does the calculation for determining the game's frame rate.
    /// Credit: https://www.youtube.com/shorts/I2r97r9h074
    /// </summary>
    private float FpsCalculation()
    {
        float total = 0f;
        foreach (float deltaTime in _frameDeltaTimeArray)
        {
            total += deltaTime;
        }
        return _frameDeltaTimeArray.Length / total;
    }

    /// <summary>
    /// sets up variables when first possible
    /// </summary>
    private void Awake()
    {
        //sets the current instance
        Instance = this;

        //enables player input
        _playerInput = new DebugInputActions();
        _playerControls = new PlayerControls();
        _defaultControls = new DefaultInputActions();
        _debugInput = _playerInput.Player.Debug;
        _restartInput = _playerInput.Player.Restart;

        //puts multiple frames into an array to slow down the fps counter instead having it change instantaniously
        _frameDeltaTimeArray = new float[60];
    }

    /// <summary>
    /// Unity callback. Draws scene view text
    /// </summary>
    private void OnGUI()
    {
        if (_sceneNameVisible)
        {
            GUI.skin.label.fontSize = 32;
            GUI.color = Color.red;
            GUI.Label(new Rect(20f, 400f, 800f, 200f), 
                $"SCENE NAME: {SceneManager.GetActiveScene().name}");
        }
    }

    /// <summary>
    /// Turns on the Debug Inputs and input checks
    /// </summary>
    private void OnEnable()
    {
        _debugInput.Enable();
        _restartInput.Enable();
        _playerInput.Player.SceneView.Enable();
        _playerInput.Player.SceneView.performed += ToggleSceneName;
        _playerControls.Enable();
    }

    /// <summary>
    /// Turns off the Debug Inputs
    /// </summary>
    private void OnDisable()
    {
        _debugInput.Disable();
        _restartInput.Disable();
        _playerInput.Player.SceneView.Disable();
        _playerInput.Player.SceneView.performed -= ToggleSceneName;
        _playerControls.Disable();
    }

    /// <summary>
    /// Sets up pointers for code functionality and makes sure the cursor is unlocked if it is ever hidden.
    /// Also does the intial controller check for a level
    /// </summary>
    private void Start()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        //unlocks the cursor if locked
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        //Sets an default game object for the event system to hold on to for menuing     
    }


    /// <summary>
    /// Updates the frame rate counter and makes sure debug unputs execute their code when pressed
    /// </summary>
    private void Update()
    {   
        //handles opening and closing the debug menu
        if (_debugInput.WasPressedThisFrame() && Debug.isDebugBuild == true)
        {
            ToggleDebugMenu();
        }       

        //restarts the current scene if the restart input is pressed
        if (_restartInput.WasPressedThisFrame())
        {
            RestartLevel();
        }

        //updates the FPS Counter
        // _frameDeltaTimeArray[_lastFrameIndex] = Time.unscaledDeltaTime;
        // _lastFrameIndex = (_lastFrameIndex + 1) % _frameDeltaTimeArray.Length;
        // _fpsText.text = (Mathf.RoundToInt(FpsCalculation()).ToString() + " FPS");
    }

    /// <summary>
    /// Method to open and close the debug menu
    /// </summary>
    public void ToggleDebugMenu()
    {
        if (_dMenu == false)
        {
            _debugMenu.SetActive(true);
            _dMenu = true;
            //Sets the default option for keyboard and controller navigation
        }
        else if ( _dMenu == true)
        {
            _debugMenu.SetActive(false);
            _puzzleSelectMenu.SetActive(false);
            _dMenu = false;
            _pMenu = false;
        }
    }

    private void ToggleSceneName(InputAction.CallbackContext ctx)
    {
        _sceneNameVisible = !_sceneNameVisible;
    }

    /// <summary>
    /// Method to open and close the menu to quit the game
    /// </summary>
    public void ToggleQuitMenu()
    {
        if (_qMenu == false)
        {
            _quitMenu.SetActive(true);
            _qMenu = true;
        }
        else if ( _qMenu == true)
        {
            _quitMenu.SetActive(false);
            _qMenu = false;
        }
    }

    /// <summary>
    /// Method to open and close the puzzle select menu
    /// </summary>
    public void TogglePuzzleSelectMenu()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if (_pMenu == false)
        {
            _puzzleSelectMenu.SetActive(true);
            _debugMenu.SetActive(false);
            _pMenu = true;
        }
        else if (_pMenu == true)
        {
            _puzzleSelectMenu.SetActive(false);
            if (PauseMenu || currentScene.name == MainMenuSceneName)
            {
                //doesn't go back to the dubug menu
                _pMenu = false;
            }
            else
            {
                _debugMenu.SetActive(true);
                _pMenu = false;
            }            
        }
    }

    /// <summary>
    /// Toggles viewing the FPS Counter
    /// </summary>
    public void ToggleFpsCounter()
    {
        if (_fpsCount == false)
        {
            _fpsCounter.SetActive(true);
            _fpsButtonText.text = "FPS Counter: On";
            _fpsCount = true;
        }
        else if (_fpsCount == true) 
        {
            _fpsCounter.SetActive(false);
            _fpsButtonText.text = "FPS Counter: Off";
            _fpsCount = false;
        }
    }

    /// <summary>
    /// Will toggle ghost mode
    /// </summary>
    public void ToggleGhostMode()
    {
        if (GhostMode == false)
        {
            _ghostModeReminder.SetActive(true);
            _ghostModeButtonText.text = "Ghost Mode: On";
            GhostMode = true;
        }
        else if (GhostMode == true)
        {
            _ghostModeReminder.SetActive(false);
            _ghostModeButtonText.text = "Ghost Mode: Off";
            GhostMode = false;
        }
    }

    /// <summary>
    /// Will toggle player invincibility.
    /// </summary>
    public void ToggleInvincibility()
    {
        if (Invincibility == false)
        {
            _invincibilityReminder.SetActive(true);
            _invincibilityButtonText.text = "Invincibility: On";
            Invincibility = true;
        }
        else if (Invincibility == true)
        {
            _invincibilityReminder.SetActive(false);
            _invincibilityButtonText.text = "Invincibility: Off";
            Invincibility = false;
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
    /// The sceneId is taken in as an int to allow the code to go to any specified scene
    /// </summary>
    public void SceneChange(int sceneId)
    {
        if(sceneId >= SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            Time.timeScale = 1f;
            EventSystem.current.SetSelectedGameObject(null);
            SceneManager.LoadScene(sceneId);
        }   
    }

    /// <summary>
    /// Quits out of the game
    /// </summary>
    public static void QuitGame()
    {
        EventSystem.current.SetSelectedGameObject(null);
        Application.Quit();
    }
}
