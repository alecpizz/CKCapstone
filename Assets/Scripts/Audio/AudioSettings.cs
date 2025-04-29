/******************************************************************
 *    Author: Claire Noto
 *    Contributors: Claire Noto, Alec Pizziferro
 *    Date Created: 10/31/2024
 *    Description: System that handles the AudioSettings in the
 *                 settings menu.
 *******************************************************************/

using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEngine.EventSystems;

public class AudioSettings : MonoBehaviour
{
    [SerializeField] private Slider _masterSlider;
    [SerializeField] private Slider _bgMusicSlider;
    [SerializeField] private Slider _SFXSlider;
    [Range (0, 10)]
    [SerializeField] private float _voiceVolume;
    [SerializeField] private EventReference _menuClicks;

    private Bus _master;
    private Bus _bgMusic;
    private Bus _SFX;
    private Bus _voice; 
    
    private const string Volume = "Volume";
    private const string MasterVolume = "Master";
    private const string MusicVolume = "Music";
    private const string SFXVolume = "SFX";
    private const string VoiceVolume = "Voice Clips";

    private void Awake()
    {
        _bgMusic = RuntimeManager.GetBus("bus:/Music");
        _SFX = RuntimeManager.GetBus("bus:/SFX");
        _master = RuntimeManager.GetBus("bus:/");
        _voice = RuntimeManager.GetBus("bus:/Voice Clips");
    }

    private void Start()
    {
        Load();
        _voice.setVolume(_voiceVolume);
    }

    /// <summary>
    /// Sets the master volume to the master slider value
    /// </summary>
    public void SetMasterVolume()
    {
        _master.setVolume(_masterSlider.value);
        SaveDataManager.SetSettingFloat(Volume, MasterVolume, _masterSlider.value);
        AudioManager.Instance.PlaySound(_menuClicks);
    }

    /// <summary>
    /// Sets the music volume to the music slider value
    /// </summary>
    public void SetMusicVolume()
    {
        _bgMusic.setVolume(_bgMusicSlider.value);
        SaveDataManager.SetSettingFloat(Volume, MusicVolume, _bgMusicSlider.value);
        AudioManager.Instance.PlaySound(_menuClicks);
    }

    /// <summary>
    /// Sets the SFX volume to the SFX slider value
    /// </summary>
    public void SetSFXVolume()
    {
        _SFX.setVolume(_SFXSlider.value);
        SaveDataManager.SetSettingFloat(Volume, SFXVolume, _SFXSlider.value);
        AudioManager.Instance.PlaySound(_menuClicks);
    }

    /// <summary>
    /// Loads the values from Save Data and adjusts the sliders accordingly
    /// </summary>
    private void Load()
    {
        _bgMusicSlider.value = SaveDataManager.GetSettingFloat(Volume, MusicVolume);
        _bgMusic.setVolume(_bgMusicSlider.value);
        _SFXSlider.value = SaveDataManager.GetSettingFloat(Volume, SFXVolume);
        _SFX.setVolume(_SFXSlider.value);
        _masterSlider.value = SaveDataManager.GetSettingFloat(Volume, MasterVolume);
        _master.setVolume(_masterSlider.value);
    }
}