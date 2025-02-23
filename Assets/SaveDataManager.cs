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

   private void Awake()
   {
      _instance = this;
   }
}
