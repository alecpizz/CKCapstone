using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Author: Trinity
/// Coconspirator : Ryan :D
/// Description: Manages harmonizable objects
/// </summary>
public class HarmonizableManager : MonoBehaviour
{
    [SerializeField] List<HarmonyTypes> _harmonyTypes;
    Dictionary<HarmonizationType, Material> _associatedMaterialOfWaveType = new();

    Dictionary<IHarmonizable, int> harmonizedObjects = new();

    List<HarmonyWave> harmonyWaves = new();

    [SerializeField] UnityEvent _visualizeAllWaves;

    private void Awake()
    {
        PopulateColorWaveDictionary();
    }

    private void Update()
    {
        foreach(HarmonyWave hw in FindObjectsOfType<HarmonyWave>())
        {
            if (hw.enabled == false)
            {
                Destroy(hw.gameObject);
            }
        }
    }

    private void PopulateColorWaveDictionary()
    {
        foreach (HarmonyTypes type in _harmonyTypes)
        {
            _associatedMaterialOfWaveType[type._harmonizationType] = type._waveColor;
            //Debug.Log("WentThroughLoop");
        }
        //Debug.Log("Dictionary count:" + _associatedColorOfWaveType.Count);
    }

    /// <summary>
    /// Adds a harmonizable to the list of harmonized and sets it as harmonized
    /// </summary>
    /// <param name="harmonizable">Harmonizable to be added</param>
    public void AddHarmonized(IHarmonizable harmonizable, HarmonizationType hType)
    {
        if (harmonizedObjects.ContainsKey(harmonizable))
            harmonizedObjects[harmonizable] += 1;
        else
            harmonizedObjects[harmonizable] = 1;
        harmonizable.SetHarmonized(true, hType);
    }

    /// <summary>
    /// Removes a harmonizable from the list of harmonized and sets it as not harmonized
    /// </summary>
    /// <param name="harmonizable">Harmonizable to be removed</param>
    public void RemoveHarmonized(IHarmonizable harmonizable, HarmonizationType hType)
    {
        if (harmonizable.IsPermaHarmonizable() && harmonizable.IsHarmonized())
            return;
        if (GameplayManagers.Instance.Room.GetRoomSolved())
            return;

        if (harmonizedObjects.ContainsKey(harmonizable))
            harmonizedObjects[harmonizable] -= 1;
        else
            return;

        if (harmonizedObjects[harmonizable] <= 0)
        {
            harmonizedObjects.Remove(harmonizable);
            harmonizable.SetHarmonized(false, hType);
        }
    }

    public bool IsharmonizableHarmonized(IHarmonizable harmonizable)
    {
        return harmonizedObjects.ContainsKey(harmonizable);
    }

    /*
        public void AddStunnedHarmony(IHarmonizable stunnable)
        {

        }

        public void RemoveHarmonized(IHarmonizable harmonizable)
        {

        }

        public bool IsHarmonizableSunned(IHarmonizable harmonizable)
        { 
        }*/
    /// <summary>
    /// Adds a harmony wave to be triggered whenever TriggerAllWaves is called
    /// </summary>
    /// <param name="wave">Wave to add</param>
    public void AddHarmonyWave(HarmonyWave wave)
    {
        harmonyWaves.Add(wave);
    }

    /// <summary>
    /// Deletes a harmony wave
    /// </summary>
    /// <param name="wave"></param>
    public void RemoveHarmonyWave(HarmonyWave wave)
    {
        if (harmonyWaves.Contains(wave))
            harmonyWaves.Remove(wave);
    }

    /// <summary>
    /// Causes all stored waves to trigger their hit detection
    /// </summary>
    public void TriggerAllWaves()
    {
        _visualizeAllWaves?.Invoke();
        CheckAllEnemiesHarmonized();
    }

    /// <summary>
    /// Checks every single enemy to see if they are harmonized
    /// </summary>
    public void CheckAllEnemiesHarmonized()
    {
        if (GameplayManagers.Instance.Enemy.GetEnemyList().Count == 0)
            return;
        foreach (EnemyBehavior enemy in GameplayManagers.Instance.Enemy.GetEnemyList())
        {
            if (!enemy.IsHarmonized())
                return;
        }
        GameplayManagers.Instance.Room.RoomVictory();
    }

    /// <summary>
    /// Returns the dictionary of wave types to materials
    /// </summary>
    /// <returns></returns>
    public Dictionary<HarmonizationType, Material> GetWaveMaterialDictionary()
    {
        return _associatedMaterialOfWaveType;
    }

    public HarmonyTypes GetHarmonyType(HarmonizationType harmonizationType)
    {
        foreach (HarmonyTypes type in _harmonyTypes)
        {
            if (type._harmonizationType == harmonizationType)
                return type;
        }

        return null;
    }

    public UnityEvent GetVisualizeAllWavesEvent()
    {
        return _visualizeAllWaves;
    }
}

[System.Serializable]
public class HarmonyTypes
{
    public HarmonizationType _harmonizationType;
    public Material _waveColor;
    public string _audioName;
}

public enum HarmonizationType
{
    A,
    B,
    C
}

