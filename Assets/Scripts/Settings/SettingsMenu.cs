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

    private List<Resolution> _resolutions;

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

        // Add listeners after loading settings
        _resolutionDropdown.onValueChanged.AddListener(SetResolution);
        _fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        _tooltipsToggle.onValueChanged.AddListener(SetTooltips);
        _subtitlesToggle.onValueChanged.AddListener(SetSubtitles);

        SetupResolutionDropdown();
    }

    /// <summary>
    /// Sets up the resolution dropdown with available resolutions.
    /// </summary>
    private void SetupResolutionDropdown()
    {
        //list of every resolution
        _resolutions = Screen.resolutions.ToList();
        _resolutionDropdown.ClearOptions();
        List<string> options = new();

        Dictionary<Resolution, RefreshRate> resolutionDict = new();//continue with Alec's changes

        int currentResolutionIndex = 0;
        for (int i = 0; i < _resolutions.Count; i++)
        {
            //set the resolution index to the initial screen resolution
            if (_resolutions[i].width == Screen.currentResolution.width &&
                _resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }

            //will add the resolution option as long as it isn't a repeat
            if (i - 1 > 0 && _resolutions[i - 1].width == _resolutions[i].width &&
                _resolutions[i - 1].height == _resolutions[i].height)
            {
                continue;
            }
            else
            {
                string option = _resolutions[i].width + " x " + _resolutions[i].height;
                options.Add(option);
            }
        }

        //removes the first option because it is a repeat (will show up in build but not editor)
        _resolutions.RemoveAt(0);
        options.RemoveAt(0);

        //adds all the options to the dropdown
        _resolutionDropdown.AddOptions(options);
        var resolutionIdx = SaveDataManager.GetSettingInt(ScreenName, Resolution);
        if (resolutionIdx == -1)
        {
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
