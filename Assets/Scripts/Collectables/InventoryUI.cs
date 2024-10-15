/******************************************************************
*    Author: Taylor Sims
*    Contributors: None
*    Date Created: 09-24-24
*    Description: This script updates the texts of how many collectibles you have.
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    // variables
    [SerializeField] private TextMeshProUGUI _collectibleText;

    /// <summary>
    /// The start function allows you to see the text
    /// when the game is started.
    /// </summary>
    void Start()
    {
        _collectibleText = GetComponent<TextMeshProUGUI>();
    }
}
