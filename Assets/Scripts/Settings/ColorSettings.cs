/******************************************************************
*    Author: Claire Noto
*    Contributors: Claire Noto, Alec Pizziferro
*    Date Created: 11/10/2024
*    Description: Color Accesibility Settings.
*******************************************************************/
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ColorSettings : MonoBehaviour
{
    [SerializeField] TMP_Dropdown _dropdown;
    public enum ColorBlindMode { Default, Protanopia, Protanomaly, Deuteranopia, Deuteranomaly, 
        Tritanopia, Tritanomaly, Achromatopsia, Achromatomaly }

    private ColorBlindMode _selection;
    private ChannelMixer _channelMixer;

    private const string ColorMode = "Colorblind Mode";
    private const string Settings = "Settings";
    private const string Accessibility = "Accessibility";

    private void Start()
    {
        _dropdown.onValueChanged.AddListener(DropdownValueChanged);
        GameObject temp = new();
        temp.AddComponent<Volume>();
        _channelMixer = temp.GetComponent<Volume>().profile.Add<ChannelMixer>();
        _channelMixer.active = true;
        _channelMixer.redOutRedIn.overrideState = _channelMixer.redOutGreenIn.overrideState = 
            _channelMixer.redOutBlueIn.overrideState = _channelMixer.greenOutRedIn.overrideState = 
            _channelMixer.greenOutGreenIn.overrideState = _channelMixer.greenOutBlueIn.overrideState = 
            _channelMixer.blueOutRedIn.overrideState = _channelMixer.blueOutGreenIn.overrideState = 
            _channelMixer.blueOutBlueIn.overrideState = true;

        // Load saved color mode or set to Normal (0) by default
        int savedMode = SaveDataManager.MainSaveData.GetData<IntType>(Settings, 
            Accessibility, ColorMode).Value;
        _selection = (ColorBlindMode)savedMode;

        // Populate dropdown and set the initial selection
        PopulateDropDownWithEnum(_dropdown);
        _dropdown.value = savedMode;
        _dropdown.RefreshShownValue();

        // Apply the saved color mode
        ChangeColorMode(_selection);
    }

    /// <summary>
    /// Whenever the dropdown is changed, this function is called automatically.
    /// </summary>
    /// <param name="value">Used to save the setting to data.</param>
    private void DropdownValueChanged(int value)
    {
        _selection = (ColorBlindMode)value;
        ChangeColorMode(_selection);

        SaveDataManager.MainSaveData.AddData(Settings, Accessibility, ColorMode, new IntType(value));
    }

    /// <summary>
    /// Data from https://www.alanzucconi.com/2015/12/16/color-blindness/
    /// Adjusts the colors to better assist color blind people.
    /// Colorblindness is hard to quantify, and there is no one correct solution to this,
    /// by allowing so many modes, it allows users to find one best for them.
    /// </summary>
    /// <param name="mode">The type of colorblindness</param>
    private void ChangeColorMode(ColorBlindMode mode)
    {
        switch (mode)
        {
            case ColorBlindMode.Default:
                ChangeVolume(new Vector3(100, 0, 0),
                             new Vector3(0, 100, 0),
                             new Vector3(0, 0, 100));
                break;
            case ColorBlindMode.Protanopia:
                ChangeVolume(new Vector3(56.667f, 43.333f, 0),
                             new Vector3(55.833f, 44.167f, 0),
                             new Vector3(0, 24.167f, 75.833f));
                break;
            case ColorBlindMode.Protanomaly:
                ChangeVolume(new Vector3(81.667f, 18.333f, 0),
                             new Vector3(33.333f, 66.667f, 0),
                             new Vector3(0, 12.5f, 87.5f));
                break;
            case ColorBlindMode.Deuteranopia:
                ChangeVolume(new Vector3(62.5f, 37.5f, 0),
                             new Vector3(70f, 30f, 0),
                             new Vector3(0, 30f, 70f));
                break;
            case ColorBlindMode.Deuteranomaly:
                ChangeVolume(new Vector3(80f, 20f, 0),
                             new Vector3(0, 25.833f, 74.167f),
                             new Vector3(0, 14.167f, 85.833f));
                break;
            case ColorBlindMode.Tritanopia:
                ChangeVolume(new Vector3(95f, 5f, 0),
                             new Vector3(0, 43.333f, 56.667f),
                             new Vector3(0, 47.5f, 52.5f));
                break;
            case ColorBlindMode.Tritanomaly:
                ChangeVolume(new Vector3(96.667f, 3.333f, 0),
                             new Vector3(0, 73.333f, 26.667f),
                             new Vector3(0, 18.333f, 81.667f));
                break;
            case ColorBlindMode.Achromatopsia:
                ChangeVolume(new Vector3(29.9f, 58.7f, 11.4f),
                             new Vector3(29.9f, 58.7f, 11.4f),
                             new Vector3(29.9f, 58.7f, 11.4f));
                break;
            case ColorBlindMode.Achromatomaly:
                ChangeVolume(new Vector3(61.8f, 32f, 6.2f),
                             new Vector3(16.3f, 77.5f, 6.2f),
                             new Vector3(16.3f, 32f, 51.6f));
                break;
        }
    }

    /// <summary>
    /// Changes the PostProcessing Channel Mixer values to filter the rendered color.
    /// </summary>
    /// <param name="red">Red channel</param>
    /// <param name="green">Green channel</param>
    /// <param name="blue">Blue channel</param>
    private void ChangeVolume(Vector3 red, Vector3 green, Vector3 blue)
    {
        if (_channelMixer != null)
        {
            _channelMixer.redOutRedIn.SetValue(new ClampedFloatParameter(red.x, -200f, 200f));
            _channelMixer.redOutGreenIn.SetValue(new ClampedFloatParameter(red.y, -200f, 200f));
            _channelMixer.redOutBlueIn.SetValue(new ClampedFloatParameter(red.z, -200f, 200f));

            _channelMixer.greenOutRedIn.SetValue(new ClampedFloatParameter(green.x, -200f, 200f));
            _channelMixer.greenOutGreenIn.SetValue(new ClampedFloatParameter(green.y, -200f, 200f));
            _channelMixer.greenOutBlueIn.SetValue(new ClampedFloatParameter(green.z, -200f, 200f));

            _channelMixer.blueOutRedIn.SetValue(new ClampedFloatParameter(blue.x, -200f, 200f));
            _channelMixer.blueOutGreenIn.SetValue(new ClampedFloatParameter(blue.y, -200f, 200f));
            _channelMixer.blueOutBlueIn.SetValue(new ClampedFloatParameter(blue.z, -200f, 200f));
        }
        else
        {
            Debug.LogWarning("ChannelMixer is not assigned.");
        }
    }

    /// <summary>
    /// Populates the dropdown with the Enum values automatically.
    /// </summary>
    /// <param name="dropdown">The dropdown being filled</param>
    private void PopulateDropDownWithEnum(TMP_Dropdown dropdown)
    {
        dropdown.ClearOptions();
        List<string> options = new(Enum.GetNames(typeof(ColorBlindMode)));
        dropdown.AddOptions(options);
    }
}