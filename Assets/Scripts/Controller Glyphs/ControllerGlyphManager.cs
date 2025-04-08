using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.Switch;
using UnityEngine.UI;

public class ControllerGlyphManager : MonoBehaviour
{
    public static ControllerGlyphManager Instance { get; private set; }
    public bool KeyboardAndMouse { get; private set; } = false;
    public bool PlayStationController { get; private set; } = false;
    public bool SwitchController { get; private set; } = false;
    public bool XboxController { get; private set; } = false;

    private bool _areInteractables = false;
    private bool _areTextBlurbs = false;
    private bool _areSettings = false;

    private DebugInputActions _playerInput;
    private PlayerControls _playerControls;
    private DefaultInputActions _defaultControls;

    private NpcDialogueController[] _interactableArray;
    private ControllerTextChecker[] _textBlurbArray;
    private HowToPlayControllerChecker[] _htpArray;

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

        _interactableArray = GameObject.FindObjectsOfType<NpcDialogueController>();
        _textBlurbArray = GameObject.FindObjectsOfType<ControllerTextChecker>();
        _htpArray = GameObject.FindObjectsOfType<HowToPlayControllerChecker>();
    }

    /// <summary>
    /// Turns on the Debug Inputs and input checks
    /// </summary>
    private void OnEnable()
    {
        _playerControls.Enable();
        //_defaultControls.Enable();
        _playerControls.InGame.Movement.performed += DetectInputType;
        _playerControls.InGame.Toggle.performed += DetectInputType;
        _playerControls.InGame.Interact.performed += DetectInputType;
        _defaultControls.UI.Point.performed += DetectInputType;
        _defaultControls.UI.Navigate.performed += DetectInputType;
    }

    /// <summary>
    /// Turns off the Debug Inputs
    /// </summary>
    private void OnDisable()
    {
        _playerControls.InGame.Movement.performed -= DetectInputType;
        _playerControls.InGame.Toggle.performed -= DetectInputType;
        _playerControls.InGame.Interact.performed -= DetectInputType;
        _defaultControls.UI.Point.performed -= DetectInputType;
        _defaultControls.UI.Navigate.performed -= DetectInputType;
        _playerControls.Disable();
        //_defaultControls.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        //changes input related text depending on current input device
        if (_interactableArray != null)
        {
            foreach (var interactable in _interactableArray)
            {
                interactable.ControllerText();
            }
            _areInteractables = true;
        }
        if (_textBlurbArray != null)
        {
            foreach (var blurb in _textBlurbArray)
            {
                blurb.TutorialTextChange();
            }
            _areTextBlurbs = true;
        }
        //The settings should be in about every scene but this is here just in case
        if (_htpArray != null)
        {
            foreach (var htp in _htpArray)
            {
                htp.HowToPlayTextChange();
            }
            _areSettings = true;
        }
    }


    /// <summary>
    /// Handles detecting if a different input device is being used when multiple input devices
    /// are connected to the computer
    /// </summary>
    /// <param name="context"></param>
    private void DetectInputType(InputAction.CallbackContext context)
    {
        string controllerName = context.control.device.displayName.ToLower();
        if (controllerName.Contains("keyboard"))
        {
            PlayStationController = false;
            SwitchController = false;
            XboxController = false;
            KeyboardAndMouse = true;
        }
        else if (controllerName.Contains("dualshock") || controllerName.Contains("dualsense") ||
            controllerName.Contains("playstation") || controllerName.Contains("wireless controller"))
        {
            PlayStationController = true;
            SwitchController = false;
            XboxController = false;
            KeyboardAndMouse = false;
        }
        else if (controllerName.Contains("pro controller") || controllerName.Contains("switch") ||
            controllerName.Contains("nintendo"))
        {
            PlayStationController = false;
            SwitchController = true;
            XboxController = false;
            KeyboardAndMouse = false;
        }
        else
        {
            PlayStationController = false;
            SwitchController = false;
            XboxController = true;
            KeyboardAndMouse = false;
        }
        //changes input related text depending on current input device
        if (_areInteractables)
        {
            foreach (var interactable in _interactableArray)
            {
                interactable.ControllerText();
            }
        }
        if (_areTextBlurbs)
        {
            foreach (var blurb in _textBlurbArray)
            {
                blurb.TutorialTextChange();
            }
        }
        if (_areSettings)
        {
            foreach (var htp in _htpArray)
            {
                htp.HowToPlayTextChange();
            }
        }
    }
}
