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
    public static DebugMenuManager instance;

    [SerializeField] private GameObject _debugMenuFirst;
    [SerializeField] private GameObject _quitMenuFirst;
    [SerializeField] private GameObject _debugMenu;
    [SerializeField] private GameObject _quitMenu;
    [SerializeField] private GameObject _fpsCounter;
    [SerializeField] private GameObject _ghostModeReminder;
    [SerializeField] private GameObject _invincibilityReminder;
    [SerializeField] private GameObject _EnemyTurnReminder;

    private bool _dMenu = false;
    private bool _qMenu = false;
    private bool _fpsCount = false;
    public bool ghostMode = false;
    private bool _invincibility = false;
    private bool _enemyTurn = true;

    private int _lastFrameIndex;
    private float[] _frameDeltaTimeArray;

    [SerializeField] private TextMeshProUGUI _fpsText;
    [SerializeField] private TextMeshProUGUI _fpsButtonText;
    [SerializeField] private TextMeshProUGUI ghostModeButtonText;
    [SerializeField] private TextMeshProUGUI _invincibilityButtonText;
    [SerializeField] private TextMeshProUGUI _enemyTurnButtonText;

    private DebugInputActions _playerInput;
    private InputAction _debugInput;
    private InputAction _restartInput;
    private InputAction _quitInput;

    /// <summary>
    /// Does the calculation for determining the game's frame rate.
    /// Credit: https://www.youtube.com/shorts/I2r97r9h074
    /// </summary>
    private float FPSCalculation()
    {
        float total = 0f;
        foreach (float deltaTime in _frameDeltaTimeArray)
        {
            total += deltaTime;
        }
        return _frameDeltaTimeArray.Length / total;
    }

    private void Awake()
    {
        //sets the current instance
        instance = this;

        //enables player input
        _playerInput = new DebugInputActions();
        _debugInput = _playerInput.Player.Debug;
        _restartInput = _playerInput.Player.Restart;
        _quitInput = _playerInput.Player.Quit;

        //puts multiple frames into an array to slow down the fps counter instead having it change instantaniously
        _frameDeltaTimeArray = new float[60];
    }

    /// <summary>
    /// Turns on the Debug Inputs
    /// </summary>
    private void OnEnable()
    {
        _debugInput.Enable();
        _restartInput.Enable();
        _quitInput.Enable();
    }

    /// <summary>
    /// Turns off the Debug Inputs
    /// </summary>
    private void OnDisable()
    {
        _debugInput.Disable();
        _restartInput.Disable();
        _quitInput.Disable();
    }

    private void Start()
    {
        //unlocks the cursor if locked
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        //Sets an default game object for the event system to hold on to for menuing
        EventSystem.current.SetSelectedGameObject(_debugMenuFirst);
    }

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

        //handles opening and closing the quiting menu
        if (_quitInput.WasPressedThisFrame())
        {
            ToggleQuitMenu();
        }

        //updates the FPS Counter
        _frameDeltaTimeArray[_lastFrameIndex] = Time.unscaledDeltaTime;
        _lastFrameIndex = (_lastFrameIndex + 1) % _frameDeltaTimeArray.Length;
        _fpsText.text = (Mathf.RoundToInt(FPSCalculation()).ToString() + " FPS");
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
            EventSystem.current.SetSelectedGameObject(_debugMenuFirst);
        }
        else if ( _dMenu == true)
        {
            _debugMenu.SetActive(false);
            _dMenu = false;
        }
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
            //Sets the default option for keyboard and controller navigation
            EventSystem.current.SetSelectedGameObject(_quitMenuFirst);
        }
        else if ( _qMenu == true)
        {
            _quitMenu.SetActive(false);
            _qMenu = false;
        }
    }

    /// <summary>
    /// Toggles viewing the FPS Counter
    /// </summary>
    public void FPSCounterToggle()
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
    public void GhostModeToggle()
    {
        if (ghostMode == false)
        {
            _ghostModeReminder.SetActive(true);
            ghostModeButtonText.text = "Ghost Mode: On";
            ghostMode = true;
        }
        else if (ghostMode == true)
        {
            _ghostModeReminder.SetActive(false);
            ghostModeButtonText.text = "Ghost Mode: Off";
            ghostMode = false;
        }
    }

    /// <summary>
    /// Will toggle player invincibility, currently just displays a notification saying it's on
    /// </summary>
    public void InvincibilityToggle()
    {
        if (_invincibility == false)
        {
            _invincibilityReminder.SetActive(true);
            _invincibilityButtonText.text = "Invincibility: On";
            _invincibility = true;
        }
        else if (_invincibility == true)
        {
            _invincibilityReminder.SetActive(false);
            _invincibilityButtonText.text = "Invincibility: Off";
            _invincibility = false;
        }
    }

    /// <summary>
    /// Will toggle turning off enemy turns, currently just displays a notification saying enemy turns are off
    /// </summary>
    public void EnemyTurnToggle()
    {
        if (_enemyTurn == true)
        {
            _EnemyTurnReminder.SetActive(true);
            _enemyTurnButtonText.text = "Enemy Turns: Off";
            _enemyTurn = false;
        }
        else if (_enemyTurn == false)
        {
            _EnemyTurnReminder.SetActive(false);
            _enemyTurnButtonText.text = "Enemy Turns: On";
            _enemyTurn = true;
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
    }
}
