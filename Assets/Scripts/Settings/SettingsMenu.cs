/******************************************************************
*    Author: Claire Noto
*    Contributors: Claire Noto, Alec Pizziferro, Josephine Qualls
*    Date Created: 11/13/2024
*    Description: Settings Menu for adjusting graphics and accessibility options.
*******************************************************************/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class SettingsMenu : MonoBehaviour
{
    [Header("Graphics Settings")]
    [SerializeField] private TMP_Dropdown _resolutionDropdown;
    [SerializeField] private Toggle _fullscreenToggle;

    [Header("Accessibility Settings")]
    [SerializeField] private Toggle _tooltipsToggle;
    [SerializeField] private Toggle _subtitlesToggle;

    //will hold all of the resolutions
    private List<Resolution> _resolutions = new();

    private const string Settings = "Settings";
    private const string ScreenName = "Screen";
    private const string Volume = "Volume";
    private const string Accessibility = "Accessibility";
    private const string Fullscreen = "Fullscreen";
    private const string Resolution = "Resolution";
    private const string Tooltips = "Tooltips";
    private const string Subtitles = "Subtitles";

    private void Start()
    {
        LoadSettings();

        SetupResolutionDropdown();
        // Add listeners after loading settings
        _resolutionDropdown.onValueChanged.AddListener(SetResolution);
        _fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        _tooltipsToggle.onValueChanged.AddListener(SetTooltips);
        _subtitlesToggle.onValueChanged.AddListener(SetSubtitles);

    }

    /// <summary>
    /// Sets up the resolution dropdown with available resolutions.
    /// </summary>
    private void SetupResolutionDropdown()
    {
        //temporary list of every resolution
        var resolutions = Screen.resolutions.ToList();
        _resolutionDropdown.ClearOptions();
        List<string> options = new();

        //will hold every resolution (by width x height) and refresh rate
        //that'll be filtered and then added to options list
        Dictionary<(int, int), RefreshRate> resolutionDict = new Dictionary<(int, int), RefreshRate>();

        for (var i = 0; i < resolutions.Count; i++)
        {
            //the current resolution and that resolutions width x height
            var currentResolution = resolutions[i];
            (int, int) res = (currentResolution.width, currentResolution.height);

            //Adds resolution if it isn't there
            if (!resolutionDict.ContainsKey(res))
            {
                resolutionDict.Add(res, currentResolution.refreshRateRatio);
            }

            //holds the highest refreshrate
            if (currentResolution.refreshRateRatio.value > resolutionDict[res].value)
            {
                resolutionDict[res] = currentResolution.refreshRateRatio;
            }
        }
        
        //Adds the resultions filtered to the main resolutions list
        foreach (var pair in resolutionDict)
        {
            _resolutions.Add(new Resolution()
            {
                width = pair.Key.Item1,
                height = pair.Key.Item2,
                refreshRateRatio = pair.Value
            });
        }

        int currentResolutionIndex = _resolutions.FindIndex(resolution => 
            resolution.height == Screen.currentResolution.height 
            && resolution.width == Screen.currentResolution.width);
        Debug.Log($"NATIVE RESOLUTION IDX IS {currentResolutionIndex}");
        //goes through the list and adds the dimensions to the options list
        foreach (var resolution in _resolutions)
        {
            string option = resolution.width + " x " + resolution.height;
            options.Add(option);
        }

        //adds all the options to the dropdown
        _resolutionDropdown.AddOptions(options);

        var resolutionIdx = SaveDataManager.GetSettingInt(ScreenName, Resolution);
        if (resolutionIdx == -1)
        {
            Debug.Log($"NO RESOLUTION SAVED, SAVING RESOLUTION AS");
            resolutionIdx = currentResolutionIndex;
            SaveDataManager.SetSettingInt(ScreenName, Resolution, resolutionIdx);
        }
        _resolutionDropdown.value = resolutionIdx;
        _resolutionDropdown.RefreshShownValue();
    }

    /// <summary>
    /// Sets the screen resolution based on the selected index.
    /// </summary>
    /// <param name="resolutionIndex">Index of the selected resolution</param>
    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = _resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        SaveDataManager.SetSettingInt(ScreenName, Resolution, resolutionIndex);        
    }

    /// <summary>
    /// Sets fullscreen mode and saves the setting.
    /// </summary>
    /// <param name="isFullscreen">True if fullscreen is enabled</param>
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        SaveDataManager.SetSettingBool(ScreenName, Fullscreen, isFullscreen); 
    }

    /// <summary>
    /// Enables or disables tooltips based on the toggle.
    /// </summary>
    /// <param name="tooltips">True if tooltips are enabled</param>
    public void SetTooltips(bool tooltips)
    {
        SaveDataManager.SetSettingBool(Accessibility, Tooltips, tooltips);
    }

    /// <summary>
    /// Enables or disables subtitles based on the toggle.
    /// </summary>
    /// <param name="subtitles">True if subtitles are enabled</param>
    public void SetSubtitles(bool subtitles)
    {
        SaveDataManager.SetSettingBool(Accessibility, Subtitles, subtitles);
    }

    /// <summary>
    /// Loads the saved settings and applies them to the UI elements.
    /// </summary>
    public void LoadSettings()
    {
        _resolutionDropdown.value = SaveDataManager.GetSettingInt(ScreenName, Resolution);
        _fullscreenToggle.isOn = SaveDataManager.GetSettingBool(ScreenName, Fullscreen);
        _tooltipsToggle.isOn = SaveDataManager.GetSettingBool(Accessibility, Tooltips);
        _subtitlesToggle.isOn = SaveDataManager.GetSettingBool(Accessibility, Subtitles);
    }
}
