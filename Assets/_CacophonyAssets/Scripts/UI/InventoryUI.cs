using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public TextMeshProUGUI collectibleText;

    // Start is called before the first frame update
    void Start()
    {
        collectibleText = GetComponent<TextMeshProUGUI>();
        
    }

    // Update is called once per frame
    void UpdateCollectibleText(PlayerInventory playerInventory)
    {
        collectibleText.text = playerInventory.collectibleCount.ToString();
        
    }
}
