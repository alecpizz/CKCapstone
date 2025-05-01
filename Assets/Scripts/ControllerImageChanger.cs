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
    [SerializeField] public Sprite ControllerImage;
    [SerializeField] public Sprite KeyboardImage;
    [SerializeField] public Image ChangingImage;

    public void ControllerImageSwap()
    {
        ChangingImage.sprite = ControllerGlyphManager.Instance.ControllerImageDecider();
    }
}
