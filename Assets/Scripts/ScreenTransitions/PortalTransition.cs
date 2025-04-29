/******************************************************************
*    Author: Zayden Joyner
*    Contributors: 
*    Date Created: 4/23/25
*    Description: Fades the portal screen in, then fades to white.
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class PortalTransition : ScreenTransitionBase
{
    [Header("Adjustable Values")]
    [Tooltip("How long the white screen should take to fade in")]
    [SerializeField] private float _whiteFadeIn = 1.5f;
    [Tooltip("How long the white screen should take to fade out")]
    [SerializeField] private float _whiteFadeOut = 1.5f;
    [Tooltip("How long the central yellow glow image should take to fade in")]
    [SerializeField] private float _glowFade = 1.5f;
    [Tooltip("How long the sparkles in the corners of the screen should take to fade in")]
    [SerializeField] private float _sparklesFadeIn = 1.5f;
    [Tooltip("How long the sparkles in the corners of the screen should take to fade out")]
    [SerializeField] private float _sparklesFadeOut = 1.5f;

    [Header("Object References")]
    [Tooltip("The white screen UI image")]
    [SerializeField] private GameObject _whiteObj;
    [Tooltip("The central yellow glow UI image")]
    [SerializeField] private GameObject _glowObj;
    [Tooltip("The corner sparkles UI image")]
    [SerializeField] private GameObject _sparklesObj;

    // Internal references
    private Image _white;
    private Image _glow;
    private Image _sparkles;

    /// <summary>
    /// Assign references
    /// </summary>
    private void Start()
    {
        _white = _whiteObj.GetComponent<Image>();
        _glow = _glowObj.GetComponent<Image>();
        _sparkles = _sparklesObj.GetComponent<Image>();
    }

    /// <summary>
    /// Fade in screen effeects from nothing.  Call this when the screen should fade in.
    /// </summary>
    public override void FadeIn()
    {
        // Reset all color properties
        _white.color = new Color(_white.color.r, _white.color.b, _white.color.b, 0f);
        _glow.color = new Color(_glow.color.r, _glow.color.b, _glow.color.b, 0f);
        _sparkles.color = new Color(_sparkles.color.r, _sparkles.color.b, _sparkles.color.b, 0f);

        // Turn on the glow image
        _glowObj.SetActive(true);

        // Fade each effect in over different durations of time
        StartCoroutine(LerpEffects(_white, _whiteFadeIn, 0, 1));
        StartCoroutine(LerpEffects(_glow, _glowFade, 0, 1));
        StartCoroutine(LerpEffects(_sparkles, _sparklesFadeIn, 0, 1));
    }

    /// <summary>
    /// Fade out screen effects.  Call this when the screen should fade out.
    /// </summary>
    public override void FadeOut()
    {
        _glow.color = new Color(_glow.color.r, _glow.color.b, _glow.color.b, 0f);
        _glowObj.SetActive(false);
        StartCoroutine(LerpEffects(_white, _whiteFadeOut, 1, 0));
        StartCoroutine(LerpEffects(_sparkles, _sparklesFadeOut, 1, 0));
    }

/// <summary>
/// Fades an image from a start value to an end value over a given duration of time
/// </summary>
/// <param name="img"> The image to change the alpha of </param>
/// <param name="duration"> The duration of time the fade should take </param>
/// <param name="start"> What value the image alpha should begin at </param>
/// <param name="end"> What value the image alpha should end at </param>
/// <returns></returns>
    private IEnumerator LerpEffects(Image img, float duration, float start, float end)
    {
        // This float will be updated over time to set the interpolation percentage
        // according the the specified lerp duration
        float time = 0;

        // Get the color property of the image
        Color colA = img.color;

        // Fade the image over time
        while (time < duration)
        {

            // Set the image alpha over time
            colA.a = Mathf.Lerp(start, end, time / duration);
            img.color = colA;

            // Add the seconds passed to time
            time += Time.deltaTime;

            // Return a null value
            yield return null;
        }

        // Just in case, set the image alpha to the given end value at the end
        img.color = new Color(img.color.r, img.color.b, img.color.b, end);
    }
}
