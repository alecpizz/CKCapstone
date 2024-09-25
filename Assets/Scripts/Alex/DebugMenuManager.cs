using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class DebugMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject _debugMenuFirst;
    public GameObject debugMenu;

    private bool dMenu = false;

    private DebugtInputActions playerInput;
    private InputAction debugInput;

    private void Awake()
    {
        playerInput = new DebugtInputActions();
        debugInput = playerInput.Player.Debug;
    }

    private void OnEnable()
    {

        debugInput.Enable();
    }

    private void OnDisable()
    {
        debugInput.Disable();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        EventSystem.current.SetSelectedGameObject(_debugMenuFirst);
    }

    private void Update()
    {
        if (debugInput.WasPressedThisFrame() && dMenu == false)
        {
            OpenDebugMenu();
        }
        else if (debugInput.WasPressedThisFrame() && dMenu == true)
        {
            CloseDebugMenu();
        }
    }

    public void OpenDebugMenu()
    {
        debugMenu.SetActive(true);
        dMenu = true;
        EventSystem.current.SetSelectedGameObject(_debugMenuFirst);
        Time.timeScale = 0f;
    }

    public void CloseDebugMenu()
    {
        debugMenu.SetActive(false);
        dMenu = false;
        Time.timeScale = 1f;
    }

    public void SceneChange(int sceneID)//sets up scene changing
    {
        Time.timeScale = 1f;
        EventSystem.current.SetSelectedGameObject(null);
        SceneManager.LoadScene(sceneID);
    }

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
