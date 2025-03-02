/******************************************************************
 *    Author: Alec Pizziferro
 *    Contributors: nullptr
 *    Date Created: 2/24/2025
 *    Description: Singleton instance/Abstraction for interacting with
 *    DataBox save data.
 *******************************************************************/

using System;
using System.IO;
using Databox;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton instance/Abstraction for interacting with DataBox save data.
/// </summary>
public static class SaveDataManager
{
    private static DataboxObject _saveDataReference = null;
    private static DataboxObject _persistentDataObject = null;
    private static readonly string PersistentDataPath = Path.Combine(Application.persistentDataPath, "CKSave.json");
    private const string SaveDataResourcesPath = "CKSaveData";
    /// <summary>
    /// Initializes the manager. This will run before scene load.
    /// Creates the save data in persistent data and copies it over. 
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Init()
    {
        //already initialized, return.
        if (_saveDataReference != null && _persistentDataObject != null) return;
        //grab save data reference from resources
        _saveDataReference = Resources.Load<DataboxObject>(SaveDataResourcesPath);
        if (_saveDataReference == null)
        {
            throw new Exception("Save data reference not found in resources!");
        }
        _saveDataReference.LoadDatabase();
        _persistentDataObject = ScriptableObject.CreateInstance<DataboxObject>();

        //check if the file has been copied over, if not write it to disk 
        if (!File.Exists(PersistentDataPath))
        {
            var referenceDataJson = Resources.Load<TextAsset>("CKSave");
            File.WriteAllText(Path.Combine(Application.persistentDataPath, "CKSave.json"),
                referenceDataJson.text);
        }

        _persistentDataObject.LoadDatabase(PersistentDataPath);
    }

    /// <summary>
    /// Save settings to disk.
    /// </summary>
    public static void SaveData()
    {
        _persistentDataObject.SaveDatabase(PersistentDataPath);
        if (_persistentDataObject.errors != DataboxObject.ErrorType.None)
        {
            Debug.Log(_persistentDataObject.errors.ToString());
        }
    }

    /// <summary>
    /// Overwrite save data with initial data.
    /// </summary>
    public static void ResetSave()
    {
        var referenceDataJson = Resources.Load<TextAsset>("CKSave");
        File.WriteAllText(Path.Combine(Application.persistentDataPath, "CKSave.json"),
            referenceDataJson.text);
    }

    /// <summary>
    /// Set a float value for settings.
    /// This will be saved automatically.
    /// See <see cref="_saveDataReference"/> or the databox object in editor for how these are
    /// referenced. Settings values are assumed to already be present in the save data.
    /// </summary>
    /// <param name="entryId">The entry id for the group. Cannot be null.</param>
    /// <param name="valueId">The value id for the group. Cannot be null.</param>
    /// <param name="newValue">Sets the settings value as a float.</param>
    public static void SetSettingFloat(string entryId, string valueId, float newValue)
    {
        _persistentDataObject.SetData<FloatType>(CKSaveData_KEYS.Settings.TableName,
            entryId, valueId, new FloatType(newValue));
        SaveData();
    }

    /// <summary>
    /// Get a float value for settings.
    /// A new value will NOT be created if not present.
    /// Future entries will need to be added in editor on the databox object.
    /// </summary>
    /// <param name="entryId">The entry id for the group. Cannot be null.</param>
    /// <param name="valueId">The value id for the group. Cannot be null.</param>
    /// <returns>0 if no data was found. The saved data value.</returns>
    public static float GetSettingFloat(string entryId, string valueId)
    {
        var dataFound = _persistentDataObject.TryGetData(CKSaveData_KEYS.Settings.TableName,
            entryId, valueId, false, out FloatType data);
        return dataFound ? data.Value : 0f;
    }

    /// <summary>
    /// Set an int value for settings.
    /// This will be saved automatically.
    /// See <see cref="_saveDataReference"/> or the databox object in editor for how these are
    /// referenced. Settings values are assumed to already be present in the save data.
    /// </summary>
    /// <param name="entryId">The entry id for the group. Cannot be null.</param>
    /// <param name="valueId">The value id for the group. Cannot be null.</param>
    /// <param name="newValue">Sets the settings value as a float.</param>
    public static void SetSettingInt(string entryId, string valueId, int newValue)
    {
        _persistentDataObject.SetData<IntType>(CKSaveData_KEYS.Settings.TableName,
            entryId, valueId, new IntType(newValue));
        SaveData();
    }

    /// <summary>
    /// Get an int value for settings.
    /// A new value will NOT be created if not present.
    /// Future entries will need to be added in editor on the databox object.
    /// </summary>
    /// <param name="entryId">The entry id for the group. Cannot be null.</param>
    /// <param name="valueId">The value id for the group. Cannot be null.</param>
    /// <returns>0 if no data was found. The saved data value.</returns>
    public static int GetSettingInt(string entryId, string valueId)
    {
        var dataFound = _persistentDataObject.TryGetData(CKSaveData_KEYS.Settings.TableName,
            entryId, valueId, false, out IntType data);
        return dataFound ? data.Value : 0;
    }

    /// <summary>
    /// Set a bool value for settings.
    /// This will be saved automatically.
    /// See <see cref="_saveDataReference"/> or the databox object in editor for how these are
    /// referenced. Settings values are assumed to already be present in the save data.
    /// </summary>
    /// <param name="entryId">The entry id for the group. Cannot be null.</param>
    /// <param name="valueId">The value id for the group. Cannot be null.</param>
    /// <param name="newValue">Sets the settings value as a float.</param>
    public static void SetSettingBool(string entryId, string valueId, bool newValue)
    {
        _persistentDataObject.SetData<BoolType>(CKSaveData_KEYS.Settings.TableName,
            entryId, valueId, new BoolType(newValue));
        SaveData();
    }

    /// <summary>
    /// Get a float value for settings.
    /// A new value will NOT be created if not present.
    /// Future entries will need to be added in editor on the databox object.
    /// </summary>
    /// <param name="entryId">The entry id for the group. Cannot be null.</param>
    /// <param name="valueId">The value id for the group. Cannot be null.</param>
    /// <returns>False if no data was found. The saved data value.</returns>
    public static bool GetSettingBool(string entryId, string valueId)
    {
        var dataFound = _persistentDataObject.TryGetData(CKSaveData_KEYS.Settings.TableName,
            entryId, valueId, false, out BoolType data);
        return dataFound && data.Value;
    }

    /// <summary>
    /// Sets NPC progress for the current scene.
    /// </summary>
    /// <param name="progress">The desired counter for dialogue in the scene.</param>
    public static void SetNpcProgressionCurrentScene(int progress) =>
        SetNpcProgression(SceneManager.GetActiveScene().name, progress);
    
    /// <summary>
    /// Sets the NPC progression for the desired scene.
    /// Will add the save data if needed.
    /// </summary>
    /// <param name="sceneName">The scene to set progression on.</param>
    /// <param name="progress">The desired counter for dialogue in the scene.</param>
    public static void SetNpcProgression(string sceneName, int progress)
    {
        if (!_persistentDataObject.SetData<IntType>(CKSaveData_KEYS.Progression.TableName,
                CKSaveData_KEYS.Progression.NPC_Dialogue.EntryName, $"{sceneName} Count",
                new IntType(progress)))
        {
            _persistentDataObject.AddData(CKSaveData_KEYS.Progression.TableName,
                CKSaveData_KEYS.Progression.NPC_Dialogue.EntryName, $"{sceneName} Count",
                new IntType(progress));
        }
        SaveData();
    }

    /// <summary>
    /// Gets the npc progression for the current scene.
    /// </summary>
    /// <returns>The progress of the current scene. Will be 0 if no progress has been made.</returns>
    public static int GetNpcProgressionCurrentScene() => GetNpcProgression(SceneManager.GetActiveScene().name);

    /// <summary>
    /// Gets the npc progression for the desired scene.
    /// </summary>
    /// <param name="sceneName">The scene to get the progression from.</param>
    /// <returns>The progress of the scene. Will be 0 if no progress has been made.</returns>
    public static int GetNpcProgression(string sceneName)
    {
        var dataFound = _persistentDataObject.TryGetData(CKSaveData_KEYS.Progression.TableName,
            CKSaveData_KEYS.Progression.NPC_Dialogue.EntryName,
            $"{sceneName} Count", true, out IntType progress);
        return dataFound ? progress.Value : 0;
    }

    /// <summary>
    /// Marks a level as complete. Will add the scene to save data if needed.
    /// </summary>
    /// <param name="sceneName">The scene name to add data to.</param>
    /// <param name="completed">Level completed status.</param>
    public static void SetLevelCompleted(string sceneName, bool completed = true)
    {
        if (!_persistentDataObject.SetData<BoolType>(CKSaveData_KEYS.Progression.TableName,
                CKSaveData_KEYS.Progression.Completed_Levels.EntryName,
                sceneName, new BoolType(completed)))
        {
            _persistentDataObject.AddData(CKSaveData_KEYS.Progression.TableName,
                CKSaveData_KEYS.Progression.Completed_Levels.EntryName,
                sceneName, new BoolType(completed));
        }

        SaveData();
    }

    /// <summary>
    /// Gets whether a level has been completed.
    /// </summary>
    /// <param name="sceneName">The scene name that has been completed.</param>
    /// <returns>True if the level was completed.</returns>
    public static bool GetLevelCompleted(string sceneName)
    {
        var foundData = _persistentDataObject.TryGetData(CKSaveData_KEYS.Progression.TableName,
            CKSaveData_KEYS.Progression.Completed_Levels.EntryName,
            sceneName, false, out BoolType data);
        return foundData && data.Value;
    }

    /// <summary>
    /// Sets the last known finished level to the scene.
    /// </summary>
    /// <param name="sceneName">The name of the scene that was finished.</param>
    public static void SetLastFinishedLevel(string sceneName)
    {
        _persistentDataObject.SetData<StringType>(CKSaveData_KEYS.Progression.TableName,
            CKSaveData_KEYS.Progression.Last_Level_Completed.EntryName,
            CKSaveData_KEYS.Progression.Last_Level_Completed._Scene_Name, new StringType(sceneName));
        SaveData();
    }

    /// <summary>
    /// Gets the name of the last known finished scene.
    /// </summary>
    /// <returns>The name of the last finished scene.</returns>
    public static string GetLastFinishedLevel()
    {
        bool foundData = _persistentDataObject.TryGetData(CKSaveData_KEYS.Progression.TableName,
            CKSaveData_KEYS.Progression.Last_Level_Completed.EntryName,
            CKSaveData_KEYS.Progression.Last_Level_Completed._Scene_Name, out StringType data);
        return foundData ? data.Value : string.Empty;
    }

    /// <summary>
    /// Marks if a collectable has been found.
    /// Will create a new entry if needed.
    /// </summary>
    /// <param name="collectableName">The name of the collectable.</param>
    /// <param name="found">Whether the collectable was found.</param>
    public static void SetCollectableFound(string collectableName, bool found = true)
    {
        if (!_persistentDataObject.SetData<BoolType>(CKSaveData_KEYS.Progression.TableName,
                CKSaveData_KEYS.Progression.Collectables.EntryName,
                collectableName, new BoolType(found)))
        {
            _persistentDataObject.AddData(CKSaveData_KEYS.Progression.TableName,
                CKSaveData_KEYS.Progression.Completed_Levels.EntryName,
                collectableName, new BoolType(found));
        }

        SaveData();
    }

    /// <summary>
    /// Gets whether a collectable has been found.
    /// </summary>
    /// <param name="collectableName">The name of the collectable.</param>
    /// <returns>Whether the collectable has been found.</returns>
    public static bool GetCollectableFound(string collectableName)
    {
        bool foundData = _persistentDataObject.TryGetData(CKSaveData_KEYS.Progression.TableName,
            CKSaveData_KEYS.Progression.Collectables.EntryName,
            collectableName, out BoolType data);
        return foundData && data.Value;
    }
}