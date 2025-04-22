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
    [Tooltip("Test fading in from nothing to Stage 1 (less intense) from editor")]
    [SerializeField] private bool _testFadeIn = false;

    [Header("Intensity Values - Stage 1 (Lower Intensity)")]
    [Tooltip("How transparent the aura effect should be at Stage 2")]
    [SerializeField] private float _auraAlpha = .98f;
    [Tooltip("How intense the chromatic aberration effect should be at Stage 2")]
    [SerializeField] private float _chromaticAberration = .23f;
    //[Tooltip("How bright the post exposure effect should be at Stage 2")]
    //[SerializeField] private float _stage2Exposure = .7f;
    [Tooltip("How long Stage 2 should take to fade (both in and out)")]
    [SerializeField] private float _auraFadeInDuration = .4f;
    [SerializeField] private float _auraFadeOutDuration = .4f;
    [SerializeField] private float _whiteFadeDuration = .4f;
    [SerializeField] private float _pauseDuration = .2f;

    [Header("Object References")]
    [Tooltip("The aura effect UI image")]
    [SerializeField] private GameObject _auraObj;
    [Tooltip("The illness post processing volume")]
    [SerializeField] private GameObject _postProcessObj;
    [Tooltip("The illness post processing volume")]
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
        // Test fading in to Stage 1 (less intense)
        if (_testFadeIn)
        {
            _white.color = new Color(_white.color.r, _white.color.b, _white.color.b, 0f);
            _aura.color = new Color(_aura.color.r, _aura.color.b, _aura.color.b, 0f);
            if (_postProcess.profile.TryGet<ChromaticAberration>(out ChromaticAberration chroma))
            {
                chroma.intensity.value = 0f;
            }
            ScreenFade();
            _testFadeIn = false;
        }

    }

    /// <summary>
    /// Fade in from nothing
    /// </summary>
    public void ScreenFade()
    {
        // Start the LerpEffect coroutine with the relevant values
        StartCoroutine(LerpEffects());
    }

    /// <summary>
    /// Lerps all of the screen effects based on the given parameters.
    /// </summary>
    /// <returns> null </returns>
    private IEnumerator LerpEffects()
    {
        // FADE IN AURA //
        
        // This float will be updated over time to set the interpolation percentage
        // according the the specified lerp duration
        float time = 0;

        // Create vars to hold the post processing components
        ChromaticAberration chroma;
        Color auraA = _aura.color;
        Color whiteA = _white.color;

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

        // Vignette alpha
        // Aura alpha
        _aura.color = new Color(_aura.color.r, _aura.color.b, _aura.color.b, _auraAlpha);
        // Chromatic aberration
        if (_postProcess.profile.TryGet<ChromaticAberration>(out chroma))
        {
            chroma.intensity.value = _chromaticAberration;
        }

        yield return new WaitForSeconds(_pauseDuration);

        // FADE TO WHITE //

        time = 0;

        // Fade over time
        while (time < _whiteFadeDuration)
        {
            // Set the vignette intensity over time
            whiteA.a = Mathf.Lerp(0f, 1f, time / _whiteFadeDuration);
            _white.color = whiteA;

            // Add the seconds passed to time
            time += Time.deltaTime;

            // Return a null value
            yield return null;
        }

        _white.color = new Color(_white.color.r, _white.color.b, _white.color.b, 1f);
    }
}
