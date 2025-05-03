/******************************************************************
*    Author: Alex Laubenstein
*    Contributors: Alex Laubenstein
*    Date Created: April 29th, 2025
*    Description: This script changes the controls graphic in the
*    how to play section of the settings menu to change depending on
*    what device is being used to control the game
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControllerImageChanger : MonoBehaviour
{
    [SerializeField] private Sprite ControllerImage;
    [SerializeField] private Sprite KeyboardImage;
    [SerializeField] private Image ChangingImage;
    
    /// <summary>
    /// Changes the controls image in how to play based off of what controller is being used
    /// </summary>
    public void ControllerImageSwap()
    {
        if (ControllerGlyphManager.Instance.UsingController)
        {
            ChangingImage.sprite = ControllerImage;
        }
        else
        {
            ChangingImage.sprite = KeyboardImage;
        }
    }
}
