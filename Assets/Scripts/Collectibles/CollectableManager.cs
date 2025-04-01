using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableManager : MonoBehaviour
{
    public GameObject[] collectables;
    public GameObject[] collectableImages;

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
    //[SerializeField] GameObject agedCardinalPlushImage;
    //[SerializeField] GameObject collegeAcceptanceLetterImage;
    //[SerializeField] GameObject crochetShtuffImage;
    //[SerializeField] GameObject electricBassImage;
    //[SerializeField] GameObject medicineCabinetImage;
    //[SerializeField] GameObject cassetteStack1Image;
    //[SerializeField] GameObject cassetteStack2Image;
    //[SerializeField] GameObject xylophoneImage;
    //[SerializeField] GameObject unfinishedLetterImage;

    public static CollectableManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
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
        //collectableImages[1] = agedCardinalPlushImage;
        //collectableImages[2] = collegeAcceptanceLetterImage;
        //collectableImages[3] = crochetShtuffImage;
        //collectableImages[4] = electricBassImage;
        //collectableImages[5] = medicineCabinetImage;
        //collectableImages[6] = cassetteStack1Image;
        //collectableImages[7] = cassetteStack2Image;
        //collectableImages[8] = xylophoneImage;
        //collectableImages[9] = unfinishedLetterImage;


        for (int i = 0; i <= collectables.Length - 1; i++)
        {
            SaveDataManager.SetCollectableFound(collectables[i].name, false);
        }

        for (int i = 0; i <= collectableImages.Length - 1; i++)
        {
            collectableImages[i].SetActive(false);
        }
    }

    public void UnlockCollectable(string unlockedCollectable)
    {
        for (int i = 0; i <= collectables.Length - 1; i++)
        {
            if (collectables[i].name == unlockedCollectable)
            {
                UnlockCollectableImage(unlockedCollectable);
            }
        }
    }

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
