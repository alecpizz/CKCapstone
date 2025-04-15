/******************************************************************
*    Author: Alec Pizziferro
*    Contributors: nullptr
*    Date Created: 4/15/2025
*    Description: Tool to capture screenshots. Hit G!
*******************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ScreenShotter : MonoBehaviour
{
    [SerializeField] private int _captureScale = 4;
    [SerializeField] private Key _captureKey = Key.G;
    /// <summary>
    /// Poll keyboard inputs and capture a screenshot.
    /// Doesn't use input system as this should be unusued in a build.
    /// </summary>
    private void Update()
    {
        if (!Keyboard.current[_captureKey].wasPressedThisFrame) return;
        string captureName = "Screenshot_" +
                             System.DateTime.Now.ToString(
                                 "dd-MM-yyyy-HH-mm-ss") +
                             ".png";
        ScreenCapture.CaptureScreenshot(captureName, _captureScale);
    }
}