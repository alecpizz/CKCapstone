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

public class AudioSettings : MonoBehaviour
{
    [SerializeField] private Slider _masterSlider;
    [SerializeField] private Slider _bgMusicSlider;
    [SerializeField] private Slider _SFXSlider;

    private Bus _master;
    private Bus _bgMusic;
    private Bus _SFX;
    
    private const string Settings = "Settings";
    private const string Volume = "Volume";
    private const string MasterVolume = "Master";
    private const string MusicVolume = "Music";
    private const string SFXVolume = "SFX";

    private void Awake()
    {
        _bgMusic = RuntimeManager.GetBus("bus:/Music");
        _SFX = RuntimeManager.GetBus("bus:/SFX");
        _master = RuntimeManager.GetBus("bus:/");
    }

    private void Start()
    {
        Load();
    }

    /// <summary>
    /// Sets the master volume to the master slider value
    /// </summary>
    public void SetMasterVolume()
    {
        _master.setVolume(_masterSlider.value);
        SaveDataManager.MainSaveData.SetData<FloatType>(Settings, Volume, MasterVolume, 
            new FloatType(_masterSlider.value));
        SaveDataManager.SaveData();
    }

    /// <summary>
    /// Sets the music volume to the music slider value
    /// </summary>
    public void SetMusicVolume()
    {
        _bgMusic.setVolume(_bgMusicSlider.value);
        SaveDataManager.MainSaveData.SetData<FloatType>(Settings, Volume, MusicVolume, 
            new FloatType(_bgMusicSlider.value));
        SaveDataManager.SaveData();
    }

    /// <summary>
    /// Sets the SFX volume to the SFX slider value
    /// </summary>
    public void SetSFXVolume()
    {
        _SFX.setVolume(_SFXSlider.value);
        SaveDataManager.MainSaveData.SetData<FloatType>(Settings, Volume, SFXVolume, 
            new FloatType(_SFXSlider.value));
        SaveDataManager.SaveData();
    }

    /// <summary>
    /// Loads the values from Save Data and adjusts the sliders accordingly
    /// </summary>
    private void Load()
    {
        _bgMusicSlider.value = SaveDataManager.MainSaveData.GetData<FloatType>(Settings, Volume,
            MusicVolume).Value;
        _bgMusic.setVolume(_bgMusicSlider.value);
        _SFXSlider.value = SaveDataManager.MainSaveData.GetData<FloatType>(Settings, Volume,
            SFXVolume).Value;;
        _SFX.setVolume(_SFXSlider.value);
        _masterSlider.value = SaveDataManager.MainSaveData.GetData<FloatType>(Settings, Volume,
            MasterVolume).Value;;
        _master.setVolume(_masterSlider.value);
    }
}