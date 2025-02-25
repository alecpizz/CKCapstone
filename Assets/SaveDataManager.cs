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

public static class SaveDataManager
{
    private static DataboxObject _saveInstance = null;
    private const string AssetPath = "Resources/CKSaveData";
    public static Databox.DataboxObject MainSaveData
    {
        get
        {
            if (_saveInstance == null)
            {
                _saveInstance = Resources.FindObjectsOfTypeAll<DataboxObject>()[0];
                Debug.Log($"Save instance {_saveInstance}");
            }

            return _saveInstance;
        }
    }
}
