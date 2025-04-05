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

public class CollectableManager : MonoBehaviour
{
    private PlayerControls _playerControls;

    [SerializeField] private GameObject _collectibleMenu;
    [SerializeField] private bool collectibleMenuOn;

    public GameObject[] collectables;
    public GameObject[] collectableImages;

    Dictionary<GameObject, GameObject> collectiblesDict = 
        new Dictionary<GameObject, GameObject>();

    #region Collectible Game Objects
    [SerializeField] GameObject cardinalPlush;
    [SerializeField] GameObject agedCardinalPlush;
    [SerializeField] GameObject collegeAcceptanceLetter;
    [SerializeField] GameObject crochetShtuff;
    [SerializeField] GameObject electricBass;
    [SerializeField] GameObject medicineCabinet;
    [SerializeField] GameObject cassetteStack1;
    [SerializeField] GameObject cassetteStack2;
    [SerializeField] GameObject xylophone;
    [SerializeField] GameObject unfinishedLetter;

    [SerializeField] GameObject cardinalPlushImage;
    [SerializeField] GameObject agedCardinalPlushImage;
    [SerializeField] GameObject collegeAcceptanceLetterImage;
    [SerializeField] GameObject crochetShtuffImage;
    [SerializeField] GameObject electricBassImage;
    [SerializeField] GameObject medicineCabinetImage;
    [SerializeField] GameObject cassetteStack1Image;
    [SerializeField] GameObject cassetteStack2Image;
    [SerializeField] GameObject xylophoneImage;
    [SerializeField] GameObject unfinishedLetterImage;
    #endregion

    public static CollectableManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Initializes and turns on the controls for the player when the scene loads
    /// </summary>
    private void OnEnable()
    {
        _playerControls = new PlayerControls();
        _playerControls.InGame.CollectibleMenu.Enable();
    }

    /// <summary>
    /// Turns off the controls for the player when the scene is no longer active
    /// </summary>
    private void OnDisable()
    {
        _playerControls.InGame.CollectibleMenu.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        collectibleMenuOn = false;

        #region Dictionary Assignment
        collectiblesDict.Add(cardinalPlush, cardinalPlushImage);
        collectiblesDict.Add(agedCardinalPlush, agedCardinalPlushImage);
        collectiblesDict.Add(collegeAcceptanceLetter, collegeAcceptanceLetterImage);
        collectiblesDict.Add(crochetShtuff, crochetShtuffImage);
        collectiblesDict.Add(electricBass, electricBassImage);
        collectiblesDict.Add(medicineCabinet, medicineCabinetImage);
        collectiblesDict.Add(cassetteStack1, cassetteStack1Image);
        collectiblesDict.Add(cassetteStack2, cassetteStack2Image);
        collectiblesDict.Add(xylophone, xylophoneImage);
        collectiblesDict.Add(unfinishedLetter, unfinishedLetterImage);
        #endregion

        #region Array Assignment
        collectables = new GameObject[10];
        collectables[0] = cardinalPlush;
        collectables[1] = agedCardinalPlush;
        collectables[2] = collegeAcceptanceLetter;
        collectables[3] = crochetShtuff;
        collectables[4] = electricBass;
        collectables[5] = medicineCabinet;
        collectables[6] = cassetteStack1;
        collectables[7] = cassetteStack2;
        collectables[8] = xylophone;
        collectables[9] = unfinishedLetter;

        collectableImages = new GameObject[10];
        collectableImages[0] = cardinalPlushImage;
        collectableImages[1] = agedCardinalPlushImage;
        collectableImages[2] = collegeAcceptanceLetterImage;
        collectableImages[3] = crochetShtuffImage;
        collectableImages[4] = electricBassImage;
        collectableImages[5] = medicineCabinetImage;
        collectableImages[6] = cassetteStack1Image;
        collectableImages[7] = cassetteStack2Image;
        collectableImages[8] = xylophoneImage;
        collectableImages[9] = unfinishedLetterImage;
        #endregion

        _playerControls.InGame.CollectibleMenu.performed += ctx => CollectibleMenuToggle();

        for (int i = 0; i <= collectableImages.Length - 1; i++)
        {
            collectableImages[i].SetActive(false);
        }
    }

    public void Collection(GameObject collected)
    {
        Debug.Log("Collecting " + collected.name);

        if (collectiblesDict.ContainsKey(collected))
        {
            Debug.Log(collected + " get!");
            SaveDataManager.SetCollectableFound(collected.name, true);
        }

        //for (int i = 0; i <= collectables.Length - 1; i++)
        //{
        //    if (gameObject.name.Equals(collectables[i].name))
        //    {
        //        Debug.Log("Collectable get!");
        //        SaveDataManager.SetCollectableFound(gameObject.name, true);
        //    }
        //}
    }

    /// <summary>
    /// Handles turning on and off the collectible menu
    /// </summary>
    private void CollectibleMenuToggle()
    {
        if(collectibleMenuOn)
        {
            collectibleMenuOn = false;
            _collectibleMenu.SetActive(false);
        }
        else
        {
            collectibleMenuOn = true;
            _collectibleMenu.SetActive(true);
            SetFoundCollectibles();
        }
    }

    /// <summary>
    /// Finds the right collectible to run through the UnlockCollectableImage function
    /// </summary>
    private void SetFoundCollectibles()
    {
        for (int i = 0; i <= collectables.Length - 1; i++)
        {
            if (SaveDataManager.GetCollectableFound(collectables[i].name))
            {
                UnlockCollectableImage(collectables[i].name);
            }
        }
    }

    /// <summary>
    /// Makes the paramenter text in the menu for the proper collectible appear
    /// </summary>
    /// <param name="unlockedImage"></param>
    private void UnlockCollectableImage(string unlockedImage)
    {
        for (int i = 0; i <= collectableImages.Length - 1; i++)
        {
            if (collectables[i].name == unlockedImage)
            {
                collectableImages[i].SetActive(true);
            }
        }
    }
}
