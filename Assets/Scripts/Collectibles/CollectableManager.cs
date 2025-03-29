using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableManager : MonoBehaviour
{
    [SerializeField] GameObject collectable1;

    // Start is called before the first frame update
    void Start()
    {
        SaveDataManager.SetCollectableFound("Collectable1", false);

        collectable1.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(SaveDataManager.GetCollectableFound("Collectable1"))
        {
            collectable1.SetActive(true);
        }
    }
}
