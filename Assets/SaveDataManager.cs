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

public class SaveDataManager : MonoBehaviour
{
   [SerializeField] private DataboxObject _dataObject;
   private static SaveDataManager _instance;
   public static Databox.DataboxObject MainSaveData => _instance._dataObject;

   /// <summary>
   /// Initialize the singleton.
   /// </summary>
   private void Awake()
   {
      _instance = this;
   }
}
