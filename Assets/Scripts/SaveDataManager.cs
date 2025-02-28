/******************************************************************
 *    Author: Alec Pizziferro
 *    Contributors: nullptr
 *    Date Created: 2/24/2025
 *    Description: Singleton instance/Abstraction for interacting with
 *    DataBox save data.
 *******************************************************************/

using System.IO;
using Databox;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveDataManager
{
    private static DataboxObject _saveDataReference = null;
    private static DataboxObject _persistentDataObject = null;
    private static readonly string PersistentDataPath = Path.Combine(Application.persistentDataPath, "CKSave.json");
    private static string GetNpcProgressString(string sceneName) => $"{sceneName} Count";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        if (_saveDataReference == null || _persistentDataObject == null)
        {
            _saveDataReference = Resources.FindObjectsOfTypeAll<DataboxObject>()[0];
            _saveDataReference.LoadDatabase();
            _persistentDataObject = ScriptableObject.CreateInstance<DataboxObject>();

            if (!File.Exists(PersistentDataPath))
            {
                var referenceDataJson = Resources.Load<TextAsset>("CKSave");
                File.WriteAllText(Path.Combine(Application.persistentDataPath, "CKSave.json"),
                    referenceDataJson.text);
            }

            _persistentDataObject.LoadDatabase(PersistentDataPath);
        }
    }

    public static void SaveSetting(DataboxType dataBoxType, string entry, string value)
    {
        var type = dataBoxType.ToString();
    }

    public static void SaveData()
    {
        _persistentDataObject.SaveDatabase(PersistentDataPath);
        if (_persistentDataObject.errors != DataboxObject.ErrorType.None)
        {
            Debug.Log(_persistentDataObject.errors.ToString());
        }
    }

    public static void ResetSave()
    {
        var referenceDataJson = Resources.Load<TextAsset>("CKSave");
        File.WriteAllText(Path.Combine(Application.persistentDataPath, "CKSave.json"),
            referenceDataJson.text);
    }

    public static void SetSettingFloat(string entryId, string valueId, float newValue)
    {
        _persistentDataObject.SetData<FloatType>(CKSaveData_KEYS.Settings.TableName,
            entryId, valueId, new FloatType(newValue));
        SaveData();
    }

    public static float GetSettingFloat(string entryId, string valueId)
    {
        var dataFound = _persistentDataObject.TryGetData(CKSaveData_KEYS.Settings.TableName, 
            entryId, valueId, false, out FloatType data);
        return dataFound ? data.Value : 0f;
    }
    
    public static void SetSettingInt(string entryId, string valueId, int newValue)
    {
        _persistentDataObject.SetData<IntType>(CKSaveData_KEYS.Settings.TableName,
            entryId, valueId, new IntType(newValue));
        SaveData();
    }
    
    public static int GetSettingInt(string entryId, string valueId)
    {
        var dataFound = _persistentDataObject.TryGetData(CKSaveData_KEYS.Settings.TableName, 
            entryId, valueId, false, out IntType data);
        return dataFound ? data.Value : 0;
    }
    
    public static void SetSettingBool(string entryId, string valueId, bool newValue)
    {
        _persistentDataObject.SetData<BoolType>(CKSaveData_KEYS.Settings.TableName,
            entryId, valueId, new BoolType(newValue));
        SaveData();
    }
    
    public static bool GetSettingBool(string entryId, string valueId)
    {
        var dataFound = _persistentDataObject.TryGetData(CKSaveData_KEYS.Settings.TableName, 
            entryId, valueId, false, out BoolType data);
        return dataFound && data.Value;
    }

    public static void SetNpcProgressionCurrentScene(int progress) =>
        SetNpcProgression(SceneManager.GetActiveScene().name, progress);

    public static void SetNpcProgression(string sceneName, int progress)
    {
        _persistentDataObject.SetData<IntType>(CKSaveData_KEYS.Progression.TableName,
            CKSaveData_KEYS.Progression.NPC_Dialogue.EntryName, GetNpcProgressString(sceneName),
            new IntType(progress));
        SaveData();
    }

    public static int GetNpcProgressionCurrentScene() => GetNpcProgression(SceneManager.GetActiveScene().name); 

    public static int GetNpcProgression(string sceneName)
    {
        var dataFound = _persistentDataObject.TryGetData(CKSaveData_KEYS.Progression.TableName,
            CKSaveData_KEYS.Progression.NPC_Dialogue.EntryName,
            GetNpcProgressString(sceneName), true, out IntType progress);
        return dataFound ? progress.Value : 0;
    }
}