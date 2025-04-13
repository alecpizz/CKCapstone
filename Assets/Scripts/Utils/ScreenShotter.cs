using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ScreenShotter : MonoBehaviour
{
    [SerializeField] private int _captureScale = 4;

    private void Update()
    {
        if (Keyboard.current[Key.G].wasPressedThisFrame)
        {
            string captureName = "Screenshot_" +
                                 System.DateTime.Now.ToString(
                                     "dd-MM-yyyy-HH-mm-ss") +
                                 ".png";
            ScreenCapture.CaptureScreenshot(captureName, _captureScale);
            Debug.Log("Captured");
        }
    }
}