/******************************************************************
*    Author: Claire Noto
*    Contributors: Claire Noto
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

    public static AudioSettings Instance;
    private const string MasterVolume = "MasterVolume";
    private const string MusicVolume = "MusicVolume";
    private const string SFXVolume = "SFXVolume";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } 
        else if (Instance != this)
        {
            Destroy(this);
        }    
        _bgMusic = RuntimeManager.GetBus("bus:/Music");
        _SFX = RuntimeManager.GetBus("bus:/SFX");
        _master = RuntimeManager.GetBus("bus:/");
    }

    private void Start()
    {
        if (!PlayerPrefs.HasKey(MasterVolume))
        {
            Debug.Log("Data not found");
            _masterSlider.value = 1;
            _bgMusicSlider.value = 1;
            _SFXSlider.value = 1;

            PlayerPrefs.SetFloat(MasterVolume, 1f);
            PlayerPrefs.SetFloat(MusicVolume, 1f);
            PlayerPrefs.SetFloat(SFXVolume, 1f);

            _master.setVolume(_masterSlider.value);
            _bgMusic.setVolume(_bgMusicSlider.value);
            _SFX.setVolume(_SFXSlider.value);

        }
        else
        {
            Debug.Log("Data found");
            Load();
            SetMasterVolume();
            SetMusicVolume();
            SetSFXVolume();
        }
    }

    /// <summary>
    /// Sets the master volume to the master slider value
    /// </summary>
    public void SetMasterVolume()
    {
        _master.setVolume(_masterSlider.value);
        PlayerPrefs.SetFloat(MasterVolume, _masterSlider.value);
    }

    /// <summary>
    /// Sets the music volume to the music slider value
    /// </summary>
    public void SetMusicVolume()
    {
        _bgMusic.setVolume(_bgMusicSlider.value);
        PlayerPrefs.SetFloat(MusicVolume, _bgMusicSlider.value);
    }

    /// <summary>
    /// Sets the SFX volume to the SFX slider value
    /// </summary>
    public void SetSFXVolume()
    {
        _SFX.setVolume(_SFXSlider.value);
        PlayerPrefs.SetFloat(SFXVolume, _SFXSlider.value);
    }

    /// <summary>
    /// Loads the values from PlayerPrefs and adjusts the sliders accordingly
    /// </summary>
    private void Load()
    {
        _bgMusicSlider.value = PlayerPrefs.GetFloat(MusicVolume);
        _SFXSlider.value = PlayerPrefs.GetFloat(SFXVolume);
        _masterSlider.value = PlayerPrefs.GetFloat(MasterVolume);
    }
}