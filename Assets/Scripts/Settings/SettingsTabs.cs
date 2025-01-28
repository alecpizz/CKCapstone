/******************************************************************
*    Author: Claire Noto
*    Contributors: Claire Noto
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

        // Assign button click events
        _displayButton.onClick.AddListener(() => OpenTab(Tab.Display));
        _displayButton.onClick.AddListener(() => EventSystem.current.SetSelectedGameObject(_displayButtonGameObject));
        _audioButton.onClick.AddListener(() => OpenTab(Tab.Audio));
        _audioButton.onClick.AddListener(() => EventSystem.current.SetSelectedGameObject(_audioButtonGameObject));
        _accessibilityButton.onClick.AddListener(() => OpenTab(Tab.Accessibility));
        _accessibilityButton.onClick.AddListener(() => EventSystem.current.SetSelectedGameObject(_accessibilityButtonGameObject));
        _gameplayButton.onClick.AddListener(() => OpenTab(Tab.Gameplay));
        _gameplayButton.onClick.AddListener(() => EventSystem.current.SetSelectedGameObject(_gameplayButtonGameObject));
        _howToPlayButton.onClick.AddListener(() => OpenTab(Tab.HowToPlay));
        _howToPlayButton.onClick.AddListener(() => EventSystem.current.SetSelectedGameObject(_howToPlayButtonGameObject));
        _levelSelectButton.onClick.AddListener(() => OpenTab(Tab.LevelSelect));
        _levelSelectButton.onClick.AddListener(() => EventSystem.current.SetSelectedGameObject(_levelSelectButtonGameObject));

        // Open the default tab at start
        OpenTab(Tab.Display);
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
