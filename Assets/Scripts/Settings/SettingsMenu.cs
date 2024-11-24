/******************************************************************
*    Author: Claire Noto
*    Contributors: Claire Noto
*    Date Created: 11/13/2024
*    Description: Settings Menu for adjusting graphics and accessibility options.
*******************************************************************/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    [Header("Graphics Settings")]
    [SerializeField] private TMP_Dropdown _resolutionDropdown;
    [SerializeField] private Toggle _fullscreenToggle;

    [Header("Accessibility Settings")]
    [SerializeField] private Toggle _tooltipsToggle;
    [SerializeField] private Toggle _subtitlesToggle;

    private Resolution[] _resolutions;

    public const string Fullscreen = "Fullscreen";
    public const string Resolution = "Resolution";
    public const string Tooltips = "Tooltips";
    public const string Subtitles = "Subtitles";

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
        _resolutions = Screen.resolutions;
        _resolutionDropdown.ClearOptions();
        List<string> options = new();

        int currentResolutionIndex = 0;
        for (int i = 0; i < _resolutions.Length; i++)
        {
            string option = _resolutions[i].width + " x " + _resolutions[i].height;
            options.Add(option);
            if (_resolutions[i].width == Screen.currentResolution.width &&
                _resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        _resolutionDropdown.AddOptions(options);
        _resolutionDropdown.value = PlayerPrefs.GetInt(Resolution, currentResolutionIndex);
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
        PlayerPrefs.SetInt(Resolution, resolutionIndex);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Sets fullscreen mode and saves the setting.
    /// </summary>
    /// <param name="isFullscreen">True if fullscreen is enabled</param>
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt(Fullscreen, isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Enables or disables tooltips based on the toggle.
    /// </summary>
    /// <param name="tooltips">True if tooltips are enabled</param>
    public void SetTooltips(bool tooltips)
    {
        PlayerPrefs.SetInt(Tooltips, tooltips ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Enables or disables subtitles based on the toggle.
    /// </summary>
    /// <param name="subtitles">True if subtitles are enabled</param>
    public void SetSubtitles(bool subtitles)
    {
        PlayerPrefs.SetInt(Subtitles, subtitles ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads the saved settings and applies them to the UI elements.
    /// </summary>
    public void LoadSettings()
    {
        _resolutionDropdown.value = PlayerPrefs.GetInt(Resolution, 0);
        _fullscreenToggle.isOn = PlayerPrefs.GetInt(Fullscreen, 1) == 1;
        _tooltipsToggle.isOn = PlayerPrefs.GetInt(Tooltips, 1) == 1;
        _subtitlesToggle.isOn = PlayerPrefs.GetInt(Subtitles, 1) == 1;
    }
}
