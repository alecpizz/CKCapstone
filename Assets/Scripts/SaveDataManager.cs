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
    private const string SettingsTableName = "Settings";
    private const string ProgressionTableName = "Progression";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        if (_saveDataReference == null)
        {
            _saveDataReference = Resources.FindObjectsOfTypeAll<DataboxObject>()[0];
            _saveDataReference.LoadDatabase();
        }
    }
    public static Databox.DataboxObject MainSaveData => _saveDataReference;

    public static void SaveSetting(DataboxType dataBoxType, string entry, string value)
    {
        var type = dataBoxType.ToString();
    }

    public static void SetDirty()
    {
        _saveDataReference.SaveDatabase();
    }

    public static void ResetSave()
    {
        _saveDataReference.ResetToInitValues(SettingsTableName);
        _saveDataReference.ResetToInitValues(ProgressionTableName);
    }
}
