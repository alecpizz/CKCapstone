/******************************************************************
 *    Author: Alec Pizziferro
 *    Contributors: nullptr
 *    Date Created: 2/24/2025
 *    Description: Singleton instance/Abstraction for interacting with
 *    DataBox save data.
 *******************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Databox;
using UnityEngine;

public enum DataType
{
    Float,
    String,
    Int,
    Bool
}

public static class SaveDataManager
{
    private static DataboxObject _saveDataReference = null;
    private static DataboxObject _persistentDataObject = null;
    private const string SettingsTableName = "Settings";
    private const string ProgressionTableName = "Progression";
    private static readonly string PersistentDataPath = Path.Combine(Application.persistentDataPath, "CKSave.json");

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        if (_saveDataReference == null || _persistentDataObject == null)
        {
            _saveDataReference  = Resources.FindObjectsOfTypeAll<DataboxObject>()[0];
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
    public static Databox.DataboxObject MainSaveData => _persistentDataObject;

    public static void SaveSetting(DataboxType dataBoxType, string entry, string value)
    {
        var type = dataBoxType.ToString();
    }

    public static void SetDirty()
    {
        _persistentDataObject.SaveDatabase(PersistentDataPath);
        Debug.Log(_persistentDataObject.errors.ToString());
    }

    public static void ResetSave()
    {
        _persistentDataObject.ResetToInitValues(SettingsTableName);
        _persistentDataObject.ResetToInitValues(ProgressionTableName);
    }
}
