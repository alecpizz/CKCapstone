using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    public static MusicController Instance;

    [SerializeField] private float _quietMusicVolume;
    private float _normalMusicVolume;

    [SerializeField]
    float musicStartDelay = 2;

    [SerializeField]
    float musicEndDelay = 2;

    [SerializeField]
    bool useEnemyMusicScaling = false;

    [SerializeField]
    private List<string> _enemyMusicNames;

    private List<float> enemyMusicStartVolumes = new();

    private int currentEnemyTrackCount = 0;

    private List<string> songsToPlay = new();

    string currentTheme = "";

    private void Awake()
    {
        if (Instance == null && Instance != this)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _normalMusicVolume = AudioManager.Instance.GetAudioFile(AudioManager.Instance.Music, "MainTheme").Source.volume;

        StoreEnemyMusicStartVolumes();

        EstablishMusic();
    }

    private int EnemiesLocated()
    {
        int enemies = 0;
        foreach (EnemyBehavior e in FindObjectsOfType<EnemyBehavior>())
        {
            if (e.isActiveAndEnabled)
            {
                enemies++;
            }
        }
        return enemies;
    }

    public void EstablishMusic()
    {
        // Play music based on enemies
        /*int enemies = EnemiesLocated();

        switch(enemies)
        {
            case 0:
                AudioManager.Instance.AdjustMusicVolume("MainTheme", _normalMusicVolume);
                QueueSong("MainTheme");
                //Debug.Log("No Enemies");
                break;
            default:
                //Debug.Log(enemies + " Enemies");
                currentEnemyTrackCount = 0;
                if (GameplayManagers.Instance.Room.GetRoomSolved())
                    break;

                AudioManager.Instance.AdjustVolumeOverTime("MainTheme", _quietMusicVolume, musicStartDelay / 4);
                AudioManager.Instance.AdjustVolumeOverTime("Player", 0, _normalMusicVolume, musicStartDelay);

                if(!AudioManager.Instance.IsMusicPlaying("Player"))
                    QueueSong("Player");

                foreach(string s in _enemyMusicNames)
                {
                    AudioManager.Instance.AdjustMusicVolume(s, 0);
                    if(!AudioManager.Instance.IsMusicPlaying(s))
                        QueueSong(s);
                }

                currentEnemyTrackCount = GameplayManagers.Instance.Enemy.EnemyCount();
                
                FadeEnemyVolume();
                    
                break;
        }*/

        // Play music based on level
        GameplayScenes currentScene = SaveSceneData.Instance.GetSceneDataOfCurrentScene();
        if(currentScene == null)
        {
            AudioManager.Instance.AdjustMusicVolume("MainTheme", _normalMusicVolume);
            QueueSong("MainTheme");
            PlayMusic();

            return;
        }

        if (!currentScene.completed)
        {
            AudioManager.Instance.AdjustVolumeOverTime("MainTheme", _quietMusicVolume, musicStartDelay / 4);
            AudioManager.Instance.AdjustVolumeOverTime("Player", 0, _normalMusicVolume, musicStartDelay);
        }
        else
        {
            AudioManager.Instance.AdjustMusicVolume("MainTheme", _normalMusicVolume);
            AudioManager.Instance.AdjustVolumeOverTime("Player", 0, 0, 0.1f);
        }
            

        if (currentTheme != null && currentTheme != "")
            StartCoroutine(FadeOutSong(currentTheme, 0, musicStartDelay / 4));

        currentTheme = currentScene.themeMusic;
        AudioManager.Instance.AdjustVolumeOverTime(currentTheme, 0, _normalMusicVolume, musicStartDelay);
        AudioManager.Instance.PlayMusic(currentTheme);
        QueueSong(currentTheme);
        QueueSong("Player");

        PlayMusic();
    }

    private void PlayMusic()
    {
        if (songsToPlay.Count == 0)
            return;

        foreach(string s in songsToPlay)
        {
            if (!AudioManager.Instance.IsMusicPlaying(s))
                AudioManager.Instance.PlayMusic(s);
        }

        songsToPlay.Clear();
    }

    private void QueueSong(string song)
    {
        if(!songsToPlay.Contains(song))
            songsToPlay.Add(song);
    }

    public void OnRoomVictory()
    {
        //AudioManager.Instance.AdjustVolumeOverTime("Player", 0, 0.1f);
        AudioManager.Instance.AdjustVolumeOverTime(currentTheme, 0, 0.1f);
        AdjustEnemyMusicVolume(0f);
        AudioManager.Instance.AdjustVolumeOverTime("MainTheme", _normalMusicVolume, musicEndDelay);
        AudioManager.Instance.AdjustVolumeOverTime("Player", 0, 0.1f);
    }

    /// <summary>
    /// Increases or decreases the number of enemy tracks being played
    /// </summary>
    /// <param name="doAdd">If true, adds to the number of tracks. Otherwise, decreases</param>
    public void UpdateEnemyMusic(bool doAdd)
    {
        if (!useEnemyMusicScaling)
            return;

        int newEnemyTrackCount = currentEnemyTrackCount + (doAdd ? 1 : -1);
        if (newEnemyTrackCount > _enemyMusicNames.Count)
            newEnemyTrackCount = _enemyMusicNames.Count;
        else if (newEnemyTrackCount < 0)
            newEnemyTrackCount = 0;

        // Enabling this causes audio issues, but allows for fading enemy music in vvv
        currentEnemyTrackCount = newEnemyTrackCount;
        //Debug.Log("Updating Enemy Music. Enemy Count: " + currentEnemyTrackCount);
        AdjustEnemyTracksPlaying(currentEnemyTrackCount);
    }

    /// <summary>
    /// Adjusts enemy music based on amount of enemies remaining
    /// </summary>
    /// <param name="amt">Amount of enemies remaining</param>
    private void AdjustEnemyTracksPlaying(int amt)
    {
        if (!useEnemyMusicScaling)
            return;

        //Debug.Log("Current Enemies: " + amt);
        if (amt == 0)
        {
            AdjustEnemyMusicVolume(0f);
            return;
        }

        int i = 0;
        for (; i < amt; i++)
        {
            AudioManager.Instance.AdjustMusicVolume(_enemyMusicNames[i], enemyMusicStartVolumes[i]);
        }
        for (; i < _enemyMusicNames.Count; i++)
        {
            AudioManager.Instance.AdjustMusicVolume(_enemyMusicNames[i], 0);
            //AudioManager.Instance.StopMusic(_enemyMusicNames[i]);
        }
    }

    private void FadeEnemyVolume()
    {
        //Debug.Log("Current Enemies: " + currentEnemyTrackCount);

        int i = 0;
        for (; i < currentEnemyTrackCount; i++)
        {
            AudioManager.Instance.AdjustVolumeOverTime(_enemyMusicNames[i], 0, enemyMusicStartVolumes[i], musicStartDelay);
        }
        for (; i < _enemyMusicNames.Count; i++)
        {
            AudioManager.Instance.AdjustMusicVolume(_enemyMusicNames[i], 0);
            //AudioManager.Instance.StopMusic(_enemyMusicNames[i]);
        }
    }

    private IEnumerator FadeOutSong(string name, float targetVolume, float time)
    {
        AudioManager.Instance.AdjustVolumeOverTime(name, targetVolume, time);
        yield return new WaitForSeconds(time);
        AudioManager.Instance.StopMusic(name);
    }

    private void StoreEnemyMusicStartVolumes()
    {
        foreach (string s in _enemyMusicNames)
            enemyMusicStartVolumes.Add(AudioManager.Instance.GetAudioFile(AudioManager.Instance.Music, s).Volume);
    }

    private void AdjustEnemyMusicVolume(float newVolume)
    {
        foreach (string s in _enemyMusicNames)
            AudioManager.Instance.AdjustMusicVolume(s, newVolume);
    }
}
