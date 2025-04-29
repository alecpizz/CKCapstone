/******************************************************************
*    Author: Zayden Joyner
*    Contributors: 
*    Date Created: 4/22/25
*    Description: Fades the illness symptom screen in, then fades to white.
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class IllnessTransition : MonoBehaviour
{
    [Header("Test Controls")]
    [Tooltip("Use this to test fading the screen in")]
    [SerializeField] private bool _testFadeIn = false;
    [Tooltip("Use this to test fading the screen out")]
    [SerializeField] private bool _testFadeOut = false;

    [Header("Intensity Values")]
    [Tooltip("How opaque the aura effect should initially become")]
    [SerializeField] private float _auraAlpha = .98f;
    [Tooltip("How apparent the chromatic aberration effect should initially become")]
    [SerializeField] private float _chromaticAberration = .23f;
    [Tooltip("How opaque the aura effect should fade to second")]
    [SerializeField] private float _auraAlpha2 = .5f;
    [Tooltip("How apparent the chromatic aberration effect should fade to second")]
    [SerializeField] private float _chromaticAberration2 = .15f;

    [Header("Duration Values")]
    [Tooltip("How long the aura and related effects should take to fade in")]
    [SerializeField] private float _auraFadeInDuration = .4f;
    [Tooltip("How long the aura and related effects should take to fade out")]
    [SerializeField] private float _auraFadeOutDuration = .4f;
    [Tooltip("How long the white screen should take to fade in")]
    [SerializeField] private float _whiteFadeIn = .4f;
    [Tooltip("How long the white screen should take to fade out")]
    [SerializeField] private float _whiteFadeOut = .4f;
    [Tooltip("How long the effects should pause at their climax before fading out")]
    [SerializeField] private float _pauseDuration = .2f;

    [Header("Object References")]
    [Tooltip("The aura effect UI image")]
    [SerializeField] private GameObject _auraObj;
    [Tooltip("The illness post processing volume")]
    [SerializeField] private GameObject _postProcessObj;
    [Tooltip("The white screen UI image")]
    [SerializeField] private GameObject _whiteObj;

    private Volume _postProcess;
    private Image _aura;
    private Image _white;

    /// <summary>
    /// Assign references
    /// </summary>
    private void Start()
    {
        _postProcess = _postProcessObj.GetComponent<Volume>();
        _aura = _auraObj.GetComponent<Image>();
        _white = _whiteObj.GetComponent<Image>();
    }

    /// <summary>
    /// Allows the user to test all fade transitions in editor
    /// </summary>
    private void Update()
    {
        // Test fading in
        if (_testFadeIn)
        {
            ScreenFade();
            _testFadeIn = false;
        }

        // Test fading out
        if (_testFadeOut)
        {
            FadeOut();
            _testFadeOut = false;
        }
    }

    /// <summary>
    /// Fade in from nothing.  Call this when the screen should fade in.
    /// </summary>
    public void ScreenFade()
    {
        // Reset all values
        _white.color = new Color(_white.color.r, _white.color.b, _white.color.b, 0f);
        _aura.color = new Color(_aura.color.r, _aura.color.b, _aura.color.b, 0f);
        if (_postProcess.profile.TryGet<ChromaticAberration>(out ChromaticAberration chroma))
        {
            chroma.intensity.value = 0f;
        }

        // Start the LerpEffect coroutine with the relevant values
        StartCoroutine(LerpEffects());
    }

    /// <summary>
    /// Fades out from white.  Call this when the screen should fade out.
    /// </summary>
    public void FadeOut()
    {
        // Reset all values
        _white.color = new Color(_white.color.r, _white.color.b, _white.color.b, 1f);
        _aura.color = new Color(_aura.color.r, _aura.color.b, _aura.color.b, 0f);
        if (_postProcess.profile.TryGet<ChromaticAberration>(out ChromaticAberration chroma))
        {
            chroma.intensity.value = 0f;
        }

        // Start the fade out
        StartCoroutine(FadeWhite(1f, 0f, _whiteFadeOut));
    }

    /// <summary>
    /// Lerps the aura and related effects between their various stages.
    /// </summary>
    /// <returns> null </returns>
    private IEnumerator LerpEffects()
    {
        // FADE IN AURA //
        
        // This float will be updated over time to set the interpolation percentage
        // according the the specified lerp duration
        float time = 0;

        // Get the specific components that need adjusting for easier reference
        ChromaticAberration chroma;
        Color auraA = _aura.color;

        // Fade all the screen effects over time
        while (time < _auraFadeInDuration)
        {

            // Set the aura intensity over time
            auraA.a = Mathf.Lerp(0f, _auraAlpha, time / _auraFadeInDuration);
            _aura.color = auraA;

            // Set the chromatic aberration over time
            if (_postProcess.profile.TryGet<ChromaticAberration>(out chroma))
            {
                chroma.intensity.value = Mathf.Lerp(0f, _chromaticAberration, time / _auraFadeInDuration);
            }

            // Add the seconds passed to time
            time += Time.deltaTime;

            // Return a null value
            yield return null;
        }

        // Just in case, set all the effects to their end values at the end

        // Aura alpha
        _aura.color = new Color(_aura.color.r, _aura.color.b, _aura.color.b, _auraAlpha);
        // Chromatic aberration
        if (_postProcess.profile.TryGet<ChromaticAberration>(out chroma))
        {
            chroma.intensity.value = _chromaticAberration;
        }

        // Pause for an amount of time
        yield return new WaitForSeconds(_pauseDuration);

        // FADE OUT TO NEXT VALUES //

        // Reset time
        time = 0f;

        // Fade out over a specified duration of time
        while (time < _auraFadeOutDuration)
        {

            // Set the aura intensity over time
            auraA.a = Mathf.Lerp(_auraAlpha, _auraAlpha2, time / _auraFadeOutDuration);
            _aura.color = auraA;

            // Set the chromatic aberration over time
            if (_postProcess.profile.TryGet<ChromaticAberration>(out chroma))
            {
                chroma.intensity.value = Mathf.Lerp(_chromaticAberration, _chromaticAberration2, time / _auraFadeOutDuration);
            }

            // Add the seconds passed to time
            time += Time.deltaTime;

            // Return a null value
            yield return null;
        }

        // Just in case, set all the effects to their end values at the end

        // Aura alpha
        _aura.color = new Color(_aura.color.r, _aura.color.b, _aura.color.b, _auraAlpha2);
        // Chromatic aberration
        if (_postProcess.profile.TryGet<ChromaticAberration>(out chroma))
        {
            chroma.intensity.value = _chromaticAberration2;
        }

        // Start fading the white screen in
        StartCoroutine(FadeWhite(0f, 1f, _whiteFadeIn));

        // FADE OUT TO ZERO //

        // Reset time
        time = 0f;

        // Fade out effects completely as the white fades in
        while (time < _whiteFadeIn)
        {

            // Set the aura intensity over time
            auraA.a = Mathf.Lerp(_auraAlpha2, 0f, time / _whiteFadeIn);
            _aura.color = auraA;

            // Set the chromatic aberration over time
            if (_postProcess.profile.TryGet<ChromaticAberration>(out chroma))
            {
                chroma.intensity.value = Mathf.Lerp(_chromaticAberration2, 0f, time / _whiteFadeIn);
            }

            // Add the seconds passed to time
            time += Time.deltaTime;

            // Return a null value
            yield return null;
        }

        // Just in case, set all the effects to their end values at the end

        // Aura alpha
        _aura.color = new Color(_aura.color.r, _aura.color.b, _aura.color.b, 0f);
        // Chromatic aberration
        if (_postProcess.profile.TryGet<ChromaticAberration>(out chroma))
        {
            chroma.intensity.value = 0f;
        }
    }

    /// <summary>
    /// Fades the white screen from a set start to a set end value
    /// </summary>
    /// <param name="start"> How opaque the screen should be to begin with </param>
    /// <param name="end"> How opaque the screen should become </param>
    /// <returns> null </returns>
    private IEnumerator FadeWhite(float start, float end, float duration)
    {
        // This float will be updated over time to set the interpolation percentage
        // according the the specified lerp duration
        float time = 0f;

        // Get the color property of the white screen
        Color whiteA = _white.color;

        // Fade over time
        while (time < duration)
        {
            // Set the vignette intensity over time
            whiteA.a = Mathf.Lerp(start, end, time / duration);
            _white.color = whiteA;

            // Add the seconds passed to time
            time += Time.deltaTime;

            // Return a null value
            yield return null;
        }

        // Set the screen to full opacity at the end
        _white.color = new Color(_white.color.r, _white.color.b, _white.color.b, end);
    }
}
