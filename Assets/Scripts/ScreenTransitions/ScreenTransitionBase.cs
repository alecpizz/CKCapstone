/******************************************************************
*    Author: Alec Pizziferro
*    Contributors: Zayden Joyner.
*    Date Created: 4/29/25
*    Description: Base class for screen transitions.
 *   Provides a base method for fading in/out.
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ScreenTransitionBase : MonoBehaviour
{
    [Header("Test Controls")]
    [Tooltip("Use this to test fading the screen in")]
    [SerializeField] private bool _testFadeIn = false;
    [Tooltip("Use this to test fading the screen out")]
    [SerializeField] private bool _testFadeOut = false;
    
    
    #if UNITY_EDITOR
    /// <summary>
    /// Allows the user to test all fade transitions in editor
    /// </summary>
    private void Update()
    {
        // Test fading in
        if (_testFadeIn)
        {
            FadeIn();
            _testFadeIn = false;
        }

        // Test fading out
        if (_testFadeOut)
        {
            FadeOut();
            _testFadeOut = false;
        }
    }
    #endif
    /// <summary>
    /// Fade in screen effeects from nothing.  Call this when the screen should fade in.
    /// </summary>
    public abstract void FadeIn();
    
    /// <summary>
    /// Fade out screen effects.  Call this when the screen should fade out.
    /// </summary>
    public abstract void FadeOut();
}
