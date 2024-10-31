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
        if (!PlayerPrefs.HasKey("masterVolume"))
        {
            PlayerPrefs.SetFloat("masterVolume", 1);
            PlayerPrefs.SetFloat("SFXVolume", 1);
            PlayerPrefs.SetFloat("musicVolume", 1);
            Load();
        }
        else
        {
            Load();
        }

        _master.getVolume(out float masterVolume);
        _masterSlider.value = masterVolume * _masterSlider.maxValue;
        _bgMusic.getVolume(out float musicVolume);
        _bgMusicSlider.value = musicVolume * _bgMusicSlider.maxValue;
        _SFX.getVolume(out float SFXVolume);
        _SFXSlider.value = SFXVolume * _SFXSlider.maxValue;

        SetMasterVolume();
        SetMusicVolume();
        SetSFXVolume();
    }

    public void SetMasterVolume()
    {
        _master.setVolume(_masterSlider.value / _masterSlider.maxValue);
        PlayerPrefs.SetFloat("masterVolume", _masterSlider.value);
    }

    public void SetMusicVolume()
    {
        _bgMusic.setVolume(_bgMusicSlider.value / _bgMusicSlider.maxValue);
        PlayerPrefs.SetFloat("musicVolume", _bgMusicSlider.value);
    }

    public void SetSFXVolume()
    {
        _SFX.setVolume(_SFXSlider.value / _SFXSlider.maxValue);
        PlayerPrefs.SetFloat("SFXVolume", _SFXSlider.value);
    }

    public void Load()
    {
        _bgMusicSlider.value = PlayerPrefs.GetFloat("musicVolume");
        _SFXSlider.value = PlayerPrefs.GetFloat("SFXVolume");
        _masterSlider.value = PlayerPrefs.GetFloat("masterVolume");
    }
}