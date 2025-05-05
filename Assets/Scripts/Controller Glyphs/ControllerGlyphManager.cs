/******************************************************************
*    Author: Alex Laubenstein
*    Contributors: Alex Laubenstein
*    Date Created: April 7th, 2025
*    Description: This script is what handles controller detection and
     the variables needed to change in game text to match the input device
     that is currently in use
*******************************************************************/
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
    //sets up variables to be used in the script
    public static ControllerGlyphManager Instance { get; private set; }
    public bool KeyboardAndMouse { get; private set; } = false;
    public bool PlayStationController { get; private set; } = false;
    public bool SwitchController { get; private set; } = false;
    public bool XboxController { get; private set; } = false;
    public bool UsingController { get; private set; } = false;

    private bool _areInteractables = false;
    private bool _areTextBlurbs = false;
    private bool _areControllerGraphics = false;
    private bool _isCutscene = false;

    private DebugInputActions _playerInput;
    private PlayerControls _playerControls;
    private DefaultInputActions _defaultControls;

    private NpcDialogueController[] _interactableArray;
    private ControllerTextChecker[] _textBlurbArray;
    private CutsceneFramework[] _cutsceneIconArray;
    private ControllerImageChanger[] _ControllerGraphicArray;

    [SerializeField] private Image _skipIcon;
    [SerializeField] private Image _skipCompletingIcon;

    [SerializeField] private Sprite _keyboardSkipButton;
    [SerializeField] private Sprite _playstationSkipButton;
    [SerializeField] private Sprite _switchSkipButton;
    [SerializeField] private Sprite _xboxSkipButton;
    [SerializeField] private Sprite _completionCircle;
    [SerializeField] private Sprite _completionSquare;

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
        _cutsceneIconArray = GameObject.FindObjectsOfType<CutsceneFramework>();
        _ControllerGraphicArray = GameObject.FindObjectsOfType<ControllerImageChanger>();
    }

    /// <summary>
    /// Turns on the Debug Inputs and input checks
    /// </summary>
    private void OnEnable()
    {
        _playerInput.Enable();
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
        _playerInput.Disable();
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
        if (_ControllerGraphicArray != null)
        {
            foreach (var graphic in _ControllerGraphicArray)
            {
                graphic.ControllerImageSwap();
            }
            _areControllerGraphics = true;
        }
        if (_cutsceneIconArray != null)
        {
            GetIcon();
            _isCutscene = true;
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
            UsingController = false;
        }
        else if (controllerName.Contains("dualshock") || controllerName.Contains("dualsense") ||
            controllerName.Contains("playstation") || controllerName.Contains("wireless controller"))
        {
            PlayStationController = true;
            SwitchController = false;
            XboxController = false;
            KeyboardAndMouse = false;
            UsingController = true;
        }
        else if (controllerName.Contains("pro controller") || controllerName.Contains("switch") ||
            controllerName.Contains("nintendo"))
        {
            PlayStationController = false;
            SwitchController = true;
            XboxController = false;
            KeyboardAndMouse = false;
            UsingController = true;
        }
        else
        {
            PlayStationController = false;
            SwitchController = false;
            XboxController = true;
            KeyboardAndMouse = false;
            UsingController = true ;
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
        if (_areControllerGraphics)
        {
            foreach (var graphic in _ControllerGraphicArray)
            {
                graphic.ControllerImageSwap();
            }
        }
        if (_isCutscene)
        {
            GetIcon();
        }
    }

    /// <summary>
    /// Sets up the text for prompting an interaction
    /// </summary>
    /// <returns></returns>
    public string GetGlyph()
    {
        string buttonText = "";
        string talkorInteract = "";
        foreach (var interactable in _interactableArray)
        {
            if (PlayStationController)
            {
                interactable._eKey.sprite = interactable._playstationButtonPrompt;
                buttonText = "X";
            }
            else if (SwitchController || XboxController)
            {
                interactable._eKey.sprite = interactable._letterButtonPrompt;
                buttonText = "A";
            }
            else
            {
                interactable._eKey.sprite = interactable._keyboardPrompt;
                buttonText = "E";
            }
            if (interactable.isNpc) //mother or son
            {
                talkorInteract = "Talk";
            }
            else
            {
                talkorInteract = "Interact";
            }
        }
        return "Press " + buttonText + " to " + talkorInteract;
    }

    /// <summary>
    /// Sets up controller specific text relating to the device controlling the game
    /// </summary>
    /// <returns></returns>
    public string TutorialText()
    {
        string tutorialText = "";
        if (PlayStationController)
        {
            foreach (var blurb in _textBlurbArray)
            {
                tutorialText = blurb._playstationText;
            }

        }
        else if (SwitchController)
        {
            foreach (var blurb in _textBlurbArray)
            {
                tutorialText = blurb._playstationText;
            }
        }
        else if (XboxController)
        {
            foreach (var blurb in _textBlurbArray)
            {
                tutorialText = blurb._controllerText;
            }
        }
        else
        {
            foreach (var blurb in _textBlurbArray)
            {
                tutorialText = blurb._keyboardText;
            }
        }
        return tutorialText;
    }

    /// <summary>
    /// Sets up changing the skip icon in cutscenes depending on what input device is being used
    /// </summary>
    public void GetIcon()
    {
        if (_skipIcon.sprite == null || _skipCompletingIcon.sprite)
            return; 

        if (PlayStationController)
        {
            _skipIcon.sprite = _playstationSkipButton;
            _skipCompletingIcon.sprite = _completionCircle;
        }
        else if (SwitchController)
        {
            _skipIcon.sprite = _switchSkipButton;
            _skipCompletingIcon.sprite = _completionCircle;
        }
        else if (XboxController)
        {
            _skipIcon.sprite = _xboxSkipButton;
            _skipCompletingIcon.sprite = _completionCircle;
        }
        else
        {
            _skipIcon.sprite = _keyboardSkipButton;
            _skipCompletingIcon.sprite = _completionSquare;
        }
    }

}
