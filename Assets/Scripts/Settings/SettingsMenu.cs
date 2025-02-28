/******************************************************************
*    Author: Claire Noto
*    Contributors: Claire Noto, Alec Pizziferro
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
        var resolutionIdx = SaveDataManager.MainSaveData.GetData<IntType>(Settings,
            ScreenName, Resolution).Value;
        if (resolutionIdx == -1)
        {
            resolutionIdx = currentResolutionIndex;
            SaveDataManager.MainSaveData.SetData<IntType>(Settings, ScreenName, 
                Resolution, new IntType(resolutionIdx));
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
        SaveDataManager.MainSaveData.SetData<IntType>(Settings, ScreenName, 
            Resolution, new IntType(resolutionIndex));
        SaveDataManager.SaveData();
    }

    /// <summary>
    /// Sets fullscreen mode and saves the setting.
    /// </summary>
    /// <param name="isFullscreen">True if fullscreen is enabled</param>
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        SaveDataManager.MainSaveData.SetData<BoolType>(Settings, ScreenName,
            Fullscreen, new BoolType(isFullscreen));
        SaveDataManager.SaveData();
    }

    /// <summary>
    /// Enables or disables tooltips based on the toggle.
    /// </summary>
    /// <param name="tooltips">True if tooltips are enabled</param>
    public void SetTooltips(bool tooltips)
    {
        SaveDataManager.MainSaveData.SetData<BoolType>(Settings, Accessibility, 
            Tooltips, new BoolType(tooltips));
        SaveDataManager.SaveData();
    }

    /// <summary>
    /// Enables or disables subtitles based on the toggle.
    /// </summary>
    /// <param name="subtitles">True if subtitles are enabled</param>
    public void SetSubtitles(bool subtitles)
    {
        SaveDataManager.MainSaveData.SetData<BoolType>(Settings, Accessibility, 
            Subtitles, new BoolType(subtitles));
        SaveDataManager.SaveData();
    }

    /// <summary>
    /// Loads the saved settings and applies them to the UI elements.
    /// </summary>
    public void LoadSettings()
    {
        Debug.Log(SaveDataManager.MainSaveData);
        _resolutionDropdown.value =
            SaveDataManager.MainSaveData.GetData<IntType>(Settings, ScreenName, Resolution).Value;
        _fullscreenToggle.isOn = SaveDataManager.MainSaveData.GetData<BoolType>(Settings,
            ScreenName, Fullscreen).Value;
        _tooltipsToggle.isOn = SaveDataManager.MainSaveData.GetData<BoolType>(Settings,
            Accessibility, Tooltips).Value;
        _subtitlesToggle.isOn =
            SaveDataManager.MainSaveData.GetData<BoolType>(Settings,
                Accessibility, Subtitles).Value;
    }
}
