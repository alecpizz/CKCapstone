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

public class PortalTransition : MonoBehaviour
{
    [Header("Test Controls")]
    [SerializeField] private bool _testFadeIn = false;
    [SerializeField] private bool _testFadeOut = false;

    [SerializeField] private float _whiteFade = 1.5f;
    [SerializeField] private float _glowFade = 1.5f;
    [SerializeField] private float _sparklesFadeIn = 1.5f;
    [SerializeField] private float _sparklesFadeOut = 1.5f;

    [Header("Object References")]
    [SerializeField] private GameObject _whiteObj;
    [SerializeField] private GameObject _glowObj;
    [SerializeField] private GameObject _sparklesObj;

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
    /// Allows the user to test all fade transitions in editor
    /// </summary>
    private void Update()
    {
        // Test fading in to Stage 1 (less intense)
        if (_testFadeIn)
        {
            _white.color = new Color(_white.color.r, _white.color.b, _white.color.b, 0f);
            _glow.color = new Color(_glow.color.r, _glow.color.b, _glow.color.b, 0f);
            _sparkles.color = new Color(_sparkles.color.r, _sparkles.color.b, _sparkles.color.b, 0f);

            ScreenFade();

            _testFadeIn = false;
        }
        // Test fading in to Stage 1 (less intense)
        if (_testFadeOut)
        {
            FadeOut();
            _testFadeOut = false;
        }

    }

    /// <summary>
    /// Fade in from nothing
    /// </summary>
    public void ScreenFade()
    {
        _glowObj.SetActive(true);

        StartCoroutine(LerpEffects(_white, _whiteFade, 0, 1));
        StartCoroutine(LerpEffects(_glow, _glowFade, 0, 1));
        StartCoroutine(LerpEffects(_sparkles, _sparklesFadeIn, 0, 1));
    }
    public void FadeOut()
    {
        _glow.color = new Color(_glow.color.r, _glow.color.b, _glow.color.b, 0f);
        _glowObj.SetActive(false);
        StartCoroutine(LerpEffects(_white, _whiteFade, 1, 0));
        StartCoroutine(LerpEffects(_sparkles, _sparklesFadeOut, 1, 0));
    }

    /// <summary>
    /// Lerps all of the screen effects based on the given parameters.
    /// </summary>
    /// <returns> null </returns>
    private IEnumerator LerpEffects(Image img, float duration, float start, float end)
    {

        // This float will be updated over time to set the interpolation percentage
        // according the the specified lerp duration
        float time = 0;

        Color colA = img.color;

        // Fade all the screen effects over time
        while (time < duration)
        {

            // Set the aura intensity over time
            colA.a = Mathf.Lerp(start, end, time / duration);
            img.color = colA;

            // Add the seconds passed to time
            time += Time.deltaTime;

            // Return a null value
            yield return null;
        }

        // Just in case, set all the effects to their end values at the end
        img.color = new Color(img.color.r, img.color.b, img.color.b, end);
    }
}
