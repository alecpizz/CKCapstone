/******************************************************************
*    Author: Claire Noto
*    Contributors: Claire Noto, Josephine Qualls
*    Date Created: 11/13/2024
*    Description: Handles settings menu tab navigation.
*******************************************************************/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsTabs : MonoBehaviour
{
    // Enum to define each tab for easier management
    public enum Tab
    {
        Display,
        Audio,
        Accessibility,
        Gameplay,
        HowToPlay,
        LevelSelect
    }

    [Header("Tab Panels")]
    [SerializeField] private GameObject _displayPanel;
    [SerializeField] private GameObject _audioPanel;
    [SerializeField] private GameObject _accessibilityPanel;
    [SerializeField] private GameObject _gameplayPanel;
    [SerializeField] private GameObject _howToPlayPanel;
    [SerializeField] private GameObject _levelSelectPanel;

    [Header("Tab Buttons")]
    [SerializeField] private Button _displayButton;
    [SerializeField] private Button _audioButton;
    [SerializeField] private Button _accessibilityButton;
    [SerializeField] private Button _gameplayButton;
    [SerializeField] private Button _howToPlayButton;
    [SerializeField] private Button _levelSelectButton;

    [Header("Button Game Objects")]
    [SerializeField] private GameObject _displayButtonGameObject;
    [SerializeField] private GameObject _audioButtonGameObject;
    [SerializeField] private GameObject _accessibilityButtonGameObject;
    [SerializeField] private GameObject _gameplayButtonGameObject;
    [SerializeField] private GameObject _howToPlayButtonGameObject;
    [SerializeField] private GameObject _levelSelectButtonGameObject;

    [Header("Alternate Button Sprites")]
    [SerializeField] private Sprite _displayImage;
    [SerializeField] private Sprite _audioImage;
    [SerializeField] private Sprite _accessibilityImage;
    [SerializeField] private Sprite _gameplayImage;
    [SerializeField] private Sprite _howToPlayImage;
    [SerializeField] private Sprite _levelSelectImage;

    private Dictionary<Tab, GameObject> _panels;
    private Dictionary<Tab, Button> _buttons;

    private void Start()
    {
        // Initialize the panel dictionary
        _panels = new Dictionary<Tab, GameObject>
        {
            { Tab.Display, _displayPanel },
            { Tab.Audio, _audioPanel },
            { Tab.Accessibility, _accessibilityPanel },
            { Tab.Gameplay, _gameplayPanel },
            { Tab.HowToPlay, _howToPlayPanel },
            { Tab.LevelSelect, _levelSelectPanel }
        };

        // Initialize the button dictionary
        _buttons = new Dictionary<Tab, Button>
        {
            { Tab.Display, _displayButton },
            { Tab.Audio, _audioButton },
            { Tab.Accessibility, _accessibilityButton },
            { Tab.Gameplay, _gameplayButton },
            { Tab.HowToPlay, _howToPlayButton },
            { Tab.LevelSelect, _levelSelectButton }
        };

        //tags for the buttons to decide images later
        _displayButton.tag = "display";
        _audioButton.tag = "audio";
        _accessibilityButton.tag = "accessibility";
        _gameplayButton.tag = "gameplay";
        _howToPlayButton.tag = "how to";
        _levelSelectButton.tag = "level";

        // Assign button click events
        _displayButton.onClick.AddListener(() => OpenTab(Tab.Display));
        _displayButton.onClick.AddListener(() => EventSystem.current.SetSelectedGameObject(_displayButtonGameObject));
        _displayButton.onClick.AddListener(() => ChangeImage(Tab.Display));
        _audioButton.onClick.AddListener(() => OpenTab(Tab.Audio));
        _audioButton.onClick.AddListener(() => EventSystem.current.SetSelectedGameObject(_audioButtonGameObject));
        _audioButton.onClick.AddListener(() => ChangeImage(Tab.Audio));
        _accessibilityButton.onClick.AddListener(() => OpenTab(Tab.Accessibility));
        _accessibilityButton.onClick.AddListener(() => EventSystem.current.SetSelectedGameObject(_accessibilityButtonGameObject));
        _accessibilityButton.onClick.AddListener(() => ChangeImage(Tab.Accessibility));
        _gameplayButton.onClick.AddListener(() => OpenTab(Tab.Gameplay));
        _gameplayButton.onClick.AddListener(() => EventSystem.current.SetSelectedGameObject(_gameplayButtonGameObject));
        _gameplayButton.onClick.AddListener(() => ChangeImage(Tab.Gameplay));
        _howToPlayButton.onClick.AddListener(() => OpenTab(Tab.HowToPlay));
        _howToPlayButton.onClick.AddListener(() => EventSystem.current.SetSelectedGameObject(_howToPlayButtonGameObject));
        _howToPlayButton.onClick.AddListener(() => ChangeImage(Tab.HowToPlay));
        _levelSelectButton.onClick.AddListener(() => OpenTab(Tab.LevelSelect));
        _levelSelectButton.onClick.AddListener(() => EventSystem.current.SetSelectedGameObject(_levelSelectButtonGameObject));
        _levelSelectButton.onClick.AddListener(() => ChangeImage(Tab.LevelSelect));

        // Open the default tab at start
        OpenTab(Tab.Display);
    }

    /// <summary>
    /// Changes the images of the other buttons depending on which is selected.
    /// </summary>
    /// <param name="tab"></param>
    private void ChangeImage(Tab tab)
    {
        foreach(var buttons in _buttons)
        {
            if(buttons.Key != tab && buttons.Value.tag == "display")
            {
                buttons.Value.GetComponent<Image>().sprite = _displayImage;

            }else if(buttons.Key != tab && buttons.Value.tag == "audio")
            {
                buttons.Value.GetComponent<Image>().sprite = _audioImage;

            }
            else if(buttons.Key != tab && buttons.Value.tag == "accessibility")
            {
                buttons.Value.GetComponent<Image>().sprite = _accessibilityImage;

            }
            else if(buttons.Key != tab && buttons.Value.tag == "gameplay")
            {
                buttons.Value.GetComponent<Image>().sprite = _gameplayImage;

            }
            else if(buttons.Key != tab && buttons.Value.tag == "how to")
            {
                buttons.Value.GetComponent<Image>().sprite = _howToPlayImage;

            }
            else if(buttons.Key != tab && buttons.Value.tag == "level")
            {
                buttons.Value.GetComponent<Image>().sprite = _levelSelectImage;

            }
        }
    }

    /// <summary>
    /// Opens a specific tab and hides others.
    /// </summary>
    /// <param name="tab">The tab to open</param>
    private void OpenTab(Tab tab)
    {
        foreach (var panel in _panels)
        {
            panel.Value.SetActive(panel.Key == tab);
        }

        HighlightButton(tab);
    }

    /// <summary>
    /// Highlights the active tab button by disabling its interactability.
    /// </summary>
    /// <param name="tab">The tab to highlight</param>
    private void HighlightButton(Tab tab)
    {
        foreach (var button in _buttons)
        {
            button.Value.interactable = button.Key != tab;  // Disable the button for the active tab
        }
    }
}
