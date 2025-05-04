/******************************************************************
*    Author: Claire Noto
*    Contributors: Claire Noto, Josephine Qualls, Mitchell Young
*    Date Created: 11/13/2024
*    Description: Handles settings menu tab navigation.
*******************************************************************/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
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
        LevelSelect,
        Cutscenes,
        CutscenesText,
        Collectables
    }

    [Header("Tab Panels")]
    [SerializeField] private GameObject _displayPanel;
    [SerializeField] private GameObject _audioPanel;
    [SerializeField] private GameObject _accessibilityPanel;
    [SerializeField] private GameObject _gameplayPanel;
    [SerializeField] private GameObject _howToPlayPanel;
    [SerializeField] private GameObject _levelSelectPanel;
    [SerializeField] private GameObject _cutscenesPanel;
    [SerializeField] private GameObject _cutscenesTextPanel;
    [SerializeField] private GameObject _collectablesPanel;

    [Header("Tab Buttons")]
    [SerializeField] private Button _displayButton;
    [SerializeField] private Button _audioButton;
    [SerializeField] private Button _accessibilityButton;
    [SerializeField] private Button _gameplayButton;
    [SerializeField] private Button _howToPlayButton;
    [SerializeField] private Button _levelSelectButton;
    [SerializeField] private Button _cutscenesButton;
    [SerializeField] private Button _collectablesButton;

    [Header("Button Game Objects")]
    [SerializeField] private GameObject _displayButtonGameObject;
    [SerializeField] private GameObject _audioButtonGameObject;
    [SerializeField] private GameObject _accessibilityButtonGameObject;
    [SerializeField] private GameObject _gameplayButtonGameObject;
    [SerializeField] private GameObject _howToPlayButtonGameObject;
    [SerializeField] private GameObject _levelSelectButtonGameObject;
    [SerializeField] private GameObject _cutscenesButtonGameObject;
    [SerializeField] private GameObject _collectablesButtonGameObject;

    [Header("Original Button Sprites")]
    [SerializeField] private Sprite _ogDisplayImage;
    [SerializeField] private Sprite _ogAudioImage;
    [SerializeField] private Sprite _ogAccessibilityImage;
    [SerializeField] private Sprite _ogGameplayImage;
    [SerializeField] private Sprite _ogHowToPlayImage;
    [SerializeField] private Sprite _ogLevelSelectImage;
    [SerializeField] private Sprite _ogCutscenesImage;
    [SerializeField] private Sprite _ogCollectablesImage;

    [Header("Alternate Button Sprites")]
    [FormerlySerializedAs("_displayImage")]
    [SerializeField] private Sprite _altDisplayImage;

    [FormerlySerializedAs("_audioImage")]
    [SerializeField] private Sprite _altAudioImage;

    [FormerlySerializedAs("_accessibilityImage")]
    [SerializeField] private Sprite _altAccessibilityImage;

    [FormerlySerializedAs("_gameplayImage")]
    [SerializeField] private Sprite _altGameplayImage;

    [FormerlySerializedAs("_howToPlayImage")]
    [SerializeField] private Sprite _altHowToPlayImage;

    [FormerlySerializedAs("_levelSelectImage")]
    [SerializeField] private Sprite _altLevelSelectImage;

    [SerializeField] private Sprite _altCutscenesImage;
    [SerializeField] private Sprite _altCollectablesImage;

    //const strings for the tags
    const string DISPLAY = "display";
    const string AUDIO = "audio";
    const string ACCESSIBILITY = "accessibility";
    const string GAMEPLAY = "gameplay";
    const string HOWTOPLAY = "how to";
    const string LEVELSELECT = "level";
    const string CUTSCENES = "cutscenes";
    const string COLLECTABLES = "collectables";

    private Dictionary<Tab, GameObject> _panels;
    private Dictionary<Tab, Button> _buttons;
    private Dictionary<Button, Sprite> _ogSprites;
    private Dictionary<Button, Sprite> _altSprites;

    /// <summary>
    /// Initializes the dictionary for the panels and buttons.
    /// Also assigns tags and Listener events related to the panels and buttons.
    /// </summary>
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
            { Tab.LevelSelect, _levelSelectPanel },
            { Tab.Cutscenes, _cutscenesPanel },
            { Tab.CutscenesText, _cutscenesTextPanel },
            { Tab.Collectables, _collectablesPanel }
        };

        // Initialize the button dictionary
        _buttons = new Dictionary<Tab, Button>
        {
            { Tab.Display, _displayButton },
            { Tab.Audio , _audioButton },
            { Tab.Accessibility , _accessibilityButton },
            { Tab.Gameplay , _gameplayButton },
            { Tab.HowToPlay , _howToPlayButton },
            { Tab.LevelSelect , _levelSelectButton },
            { Tab.Cutscenes, _cutscenesButton },
            { Tab.Collectables, _collectablesButton }
        };

        // Initialize the alt images dictionary
        _ogSprites = new Dictionary<Button, Sprite>
        {
            {_displayButton, _ogDisplayImage },
            {_audioButton, _ogAudioImage },
            {_accessibilityButton, _ogAccessibilityImage },
            {_gameplayButton, _ogGameplayImage },
            {_howToPlayButton, _ogHowToPlayImage },
            {_levelSelectButton, _ogLevelSelectImage },
            //Placeholder
            {_cutscenesButton, _ogCutscenesImage },
            {_collectablesButton, _ogCollectablesImage }

            //For implementation
            //{_cutscenesButton, _ogCutscenesImage },
            //{_collectablesButton, _ogCollectablesImage }
        };

        _altSprites = new Dictionary<Button, Sprite>
        {
            {_displayButton, _altDisplayImage },
            {_audioButton, _altAudioImage },
            {_accessibilityButton, _altAccessibilityImage },
            {_gameplayButton, _altGameplayImage },
            {_howToPlayButton, _altHowToPlayImage },
            {_levelSelectButton, _altLevelSelectImage },
            //Placeholder
            {_cutscenesButton,  _altCutscenesImage },
            {_collectablesButton, _altCollectablesImage }

            //For implementation
            //{_cutscenesButton, _altCutscenesImage },
            //{_collectablesButton, _altCollectablesImage }
        };

        //tags for the buttons so the right images will be shown
        _displayButton.tag = DISPLAY;
        _audioButton.tag = AUDIO;
        _accessibilityButton.tag = ACCESSIBILITY;
        _gameplayButton.tag = GAMEPLAY;
        _howToPlayButton.tag = HOWTOPLAY;
        _levelSelectButton.tag = LEVELSELECT;
        _cutscenesButton.tag = CUTSCENES;
        _collectablesButton.tag = COLLECTABLES;

        // Assign button click events
        //display events
        _displayButton.onClick.AddListener(() => OpenTab(Tab.Display));
        _displayButton.onClick.AddListener(() => EventSystem.current.SetSelectedGameObject(_displayButtonGameObject));
        _displayButton.onClick.AddListener(() => ChangeImage(_displayButton));

        //audio events
        _audioButton.onClick.AddListener(() => OpenTab(Tab.Audio));
        _audioButton.onClick.AddListener(() => EventSystem.current.SetSelectedGameObject(_audioButtonGameObject));
        _audioButton.onClick.AddListener(() => ChangeImage(_audioButton));

        //accessibility events
        _accessibilityButton.onClick.AddListener(() => OpenTab(Tab.Accessibility));
        _accessibilityButton.onClick.AddListener(() => EventSystem.current.SetSelectedGameObject(_accessibilityButtonGameObject));
        _accessibilityButton.onClick.AddListener(() => ChangeImage(_accessibilityButton));


        //gameplay events
        _gameplayButton.onClick.AddListener(() => OpenTab(Tab.Gameplay));
        _gameplayButton.onClick.AddListener(() => EventSystem.current.SetSelectedGameObject(_gameplayButtonGameObject));
        _gameplayButton.onClick.AddListener(() => ChangeImage(_gameplayButton));

        //how to play events
        _howToPlayButton.onClick.AddListener(() => OpenTab(Tab.HowToPlay));
        _howToPlayButton.onClick.AddListener(() => EventSystem.current.SetSelectedGameObject(_howToPlayButtonGameObject));
        _howToPlayButton.onClick.AddListener(() => ChangeImage(_howToPlayButton));

        //level select events
        _levelSelectButton.onClick.AddListener(() => OpenTab(Tab.LevelSelect));
        _levelSelectButton.onClick.AddListener(() => EventSystem.current.SetSelectedGameObject(_levelSelectButtonGameObject));
        _levelSelectButton.onClick.AddListener(() => ChangeImage(_levelSelectButton));

        //cutscenes events
        _cutscenesButton.onClick.AddListener(() => OpenTab(Tab.Cutscenes));
        //_cutscenesButton.onClick.AddListener(() => OpenTab(Tab.CutscenesText));
        _cutscenesButton.onClick.AddListener(() => EventSystem.current.SetSelectedGameObject(_cutscenesButtonGameObject));
        _cutscenesButton.onClick.AddListener(() => ChangeImage(_cutscenesButton));

        //collectables events
        _collectablesButton.onClick.AddListener(() => OpenTab(Tab.Collectables));
        _collectablesButton.onClick.AddListener(() => EventSystem.current.SetSelectedGameObject(_collectablesButtonGameObject));
        _collectablesButton.onClick.AddListener(() => ChangeImage(_collectablesButton));
        _collectablesButton.onClick.AddListener(() => CollectableManager.Instance.SetFoundCollectibles());

        // Open the default tab at start
        OpenTab(Tab.Display);
        ChangeImage(_displayButton);
    }

    /// <summary>
    /// Changes the images of the other buttons depending on which is selected.
    /// </summary>
    /// <param name="tab"></param>
    private void ChangeImage(Button button)
    {
        button.GetComponent<Image>().sprite = _ogSprites[button];
    }

    public void OpenLevelSelect()
    {
        _levelSelectButton.onClick?.Invoke();
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
    /// Highlights and displays the full image of the active tab button
    /// plus disables its interactability.
    /// </summary>
    /// <param name="tab">The tab to highlight</param>
    private void HighlightButton(Tab tab)
    {

        foreach (var altImages in _altSprites)
        {
            //figures out which button will show full image
            altImages.Key.GetComponent<Image>().sprite = altImages.Value;

        }

        foreach(var button in _buttons)
        {
            button.Value.interactable = button.Key != tab;  // Disable the button for the active tab
        }
    }
}

