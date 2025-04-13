/******************************************************************
*    Author: Cole Stranczek
*    Contributors: 
*    Date Created: 3/25/25
*    Description: Script that handles the collectible menu
*******************************************************************/

using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaintsField.Playa;

public class CollectableManager : MonoBehaviour
{
    private PlayerControls _playerControls;

    private Dictionary<string, GameObject> collectiblesDict = 
        new Dictionary<string, GameObject>();

    #region Collectible Game Objects
    [SerializeField] private GameObject cardinalPlush;
    [SerializeField] private GameObject agedCardinalPlush;
    [SerializeField] private GameObject collegeAcceptanceLetter;
    [SerializeField] private GameObject crochetShtuff;
    [SerializeField] private GameObject electricBass;
    [SerializeField] private GameObject medicineCabinet;
    [SerializeField] private GameObject cassetteStack1;
    [SerializeField] private GameObject cassetteStack2;
    [SerializeField] private GameObject xylophone;
    [SerializeField] private GameObject unfinishedLetter;

    [SerializeField] private GameObject cardinalPlushImage;
    [SerializeField] private GameObject agedCardinalPlushImage;
    [SerializeField] private GameObject collegeAcceptanceLetterImage;
    [SerializeField] private GameObject crochetShtuffImage;
    [SerializeField] private GameObject electricBassImage;
    [SerializeField] private GameObject medicineCabinetImage;
    [SerializeField] private GameObject cassetteStack1Image;
    [SerializeField] private GameObject cassetteStack2Image;
    [SerializeField] private GameObject xylophoneImage;
    [SerializeField] private GameObject unfinishedLetterImage;
    #endregion

    public static CollectableManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    private void Start()
    {
        #region Dictionary Assignment
        collectiblesDict.Add("Cardinal Plush", cardinalPlushImage);
        collectiblesDict.Add("Aged Cardinal Plush", agedCardinalPlushImage);
        collectiblesDict.Add("College Acceptance Letter", collegeAcceptanceLetterImage);
        collectiblesDict.Add("Crochet Shtuff", crochetShtuffImage);
        collectiblesDict.Add("Electric Bass", electricBassImage);
        collectiblesDict.Add("Medicine Cabinet", medicineCabinetImage);
        collectiblesDict.Add("Stack of Cassette Tapes", cassetteStack1Image);
        collectiblesDict.Add("Stack of Cassette Tapes 2", cassetteStack2Image);
        collectiblesDict.Add("Toy Xylophone", xylophoneImage);
        collectiblesDict.Add("Unfinished Letter", unfinishedLetterImage);
        #endregion

        SetFoundCollectibles();
    }

    /// <summary>
    /// An inspector button to clear the collectible menu 
    /// that I've been using for debugging purposes
    /// </summary>
    [Button]
    private void ClearData()
    {
        foreach (string data in collectiblesDict.Keys)
        {
            if (SaveDataManager.GetCollectableFound(data))
            {
                SaveDataManager.SetCollectableFound(data, false);
                collectiblesDict[data].SetActive(false);
            }
        }
    }

    /// <summary>
    /// Logs the save data for the collectible that's just been interacted with
    /// </summary>
    /// <param name="collected"></param>
    public void Collection(GameObject collected)
    {
        if (collectiblesDict.ContainsKey(collected.name))
        {
            SaveDataManager.SetCollectableFound(collected.name, true);
        }
    }

    /// <summary>
    /// Finds the right collectible to run through the UnlockCollectableImage function
    /// </summary>
    public void SetFoundCollectibles()
    {
        foreach(string data in collectiblesDict.Keys)
        {
            if(SaveDataManager.GetCollectableFound(data))
            {
                collectiblesDict[data].SetActive(true);
            }
            else
            {
                collectiblesDict[data].SetActive(false);
            }
        }
    }
}
