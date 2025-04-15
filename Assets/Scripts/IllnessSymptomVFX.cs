/******************************************************************
*    Author: Zayden Joyner
*    Contributors: 
*    Date Created: 4/3/25
*    Description: Fades the illness symptom screen effects in and out.
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class IllnessSymptomVFX : MonoBehaviour
{
    [Header("Test Controls")]
    [Tooltip("Test fading in from nothing to Stage 1 (less intense) from editor")]
    [SerializeField] private bool _testStage1FadeIn = false;
    [Tooltip("Test fading in from Stage 1 (less intense) to Stage 2 (more intense) from editor")]
    [SerializeField] private bool _testStage2FadeIn = false;
    [Tooltip("Test fading out from Stage 2 (more intense) to Stage 1 (less intense) from editor")]
    [SerializeField] private bool _testStage2FadeOut = false;
    [Tooltip("Test fading out from Stage 1 (less intense) to nothing from editor")]
    [SerializeField] private bool _testStage1FadeOut = false;

    [Header("Intensity Values - Stage 1 (Lower Intensity)")]
    [Tooltip("How transparent the vignette should be at Stage 1")]
    [SerializeField] private float _stage1VignetteAlpha = .25f;
    [Tooltip("How transparent the aura effect should be at Stage 1")]
    [SerializeField] private float _stage1AuraAlpha = .47f;
    [Tooltip("How intense the chromatic aberration effect should be at Stage 1")]
    [SerializeField] private float _stage1ChromaticAberration = .23f;
    [Tooltip("How bright the post exposure effect should be at Stage 1")]
    [SerializeField] private float _stage1Exposure = .7f;
    [Tooltip("How long Stage 1 should take to fade (both in and out)")]
    [SerializeField] private float _stage1FadeDuration = .4f;

    [Header("Intensity Values - Stage 1 (Lower Intensity)")]
    [Tooltip("How transparent the vignette should be at Stage 2")]
    [SerializeField] private float _stage2VignetteAlpha = .71f;
    [Tooltip("How transparent the aura effect should be at Stage 2")]
    [SerializeField] private float _stage2AuraAlpha = .98f;
    [Tooltip("How intense the chromatic aberration effect should be at Stage 2")]
    [SerializeField] private float _stage2ChromaticAberration = .23f;
    [Tooltip("How bright the post exposure effect should be at Stage 2")]
    [SerializeField] private float _stage2Exposure = .7f;
    [Tooltip("How long Stage 2 should take to fade (both in and out)")]
    [SerializeField] private float _stage2FadeDuration = .4f;

    [Header("Object References")]
    [Tooltip("The vignette UI image")]
    [SerializeField] private GameObject _vignetteObj;
    [Tooltip("The aura effect UI image")]
    [SerializeField] private GameObject _auraObj;
    [Tooltip("The illness post processing volume")]
    [SerializeField] private GameObject _postProcessObj;

    private Volume _postProcess;
    private Image _vignette;
    private Image _aura;

    /// <summary>
    /// Assign references
    /// </summary>
    private void Start()
    {
        _postProcess = _postProcessObj.GetComponent<Volume>();
        _vignette = _vignetteObj.GetComponent<Image>();
        _aura = _auraObj.GetComponent<Image>();
    }

    /// <summary>
    /// Allows the user to test all fade transitions in editor
    /// </summary>
    private void Update()
    {
        // Test fading in to Stage 1 (less intense)
        if (_testStage1FadeIn)
        {
            Stage1FadeIn();
            _testStage1FadeIn = false;
        }

        // Test fading in to Stage 2 (more intense)
        if (_testStage2FadeIn)
        {
            Stage2FadeIn();
            _testStage2FadeIn = false;
        }

        // Test fading out of Stage 2 (more intense) to Stage 1 (less intense)
        if (_testStage2FadeOut)
        {
            Stage2FadeOut();
            _testStage2FadeOut = false;
        }

        // Test fading out of Stage 1 (less intense)
        if (_testStage1FadeOut)
        {
            Stage1FadeOut();
            _testStage1FadeOut = false;
        }
    }

    /// <summary>
    /// Fade in from nothing
    /// </summary>
    public void Stage1FadeIn()
    {
        // Start the LerpEffect coroutine with the relevant values
        StartCoroutine(LerpEffects(0f, _stage1VignetteAlpha, 0f, _stage1AuraAlpha, 0f, _stage1ChromaticAberration, 
            0f, _stage1Exposure, _stage1FadeDuration));
    }

    /// <summary>
    /// Fade in from Stage 1 (less intense) to Stage 2 (more intense)
    /// </summary>
    public void Stage2FadeIn()
    {
        // Start the LerpEffect coroutine with the relevant values
        StartCoroutine(LerpEffects(_stage1VignetteAlpha, _stage2VignetteAlpha, _stage1AuraAlpha, _stage2AuraAlpha, 
            _stage1ChromaticAberration, _stage2ChromaticAberration, _stage1Exposure, _stage2Exposure, _stage2FadeDuration));
    }

    /// <summary>
    /// Fade out to nothing
    /// </summary>
    public void Stage1FadeOut()
    {
        // Start the LerpEffect coroutine with the relevant values
        StartCoroutine(LerpEffects(_stage1VignetteAlpha, 0f, _stage1AuraAlpha, 0f, _stage1ChromaticAberration, 
            0f, _stage1Exposure, 0f, _stage1FadeDuration));
    }

    /// <summary>
    /// Fade out from Stage 2 (more intense) to Stage 1 (less intense)
    /// </summary>
    public void Stage2FadeOut()
    {
        // Start the LerpEffect coroutine with the relevant values
        StartCoroutine(LerpEffects(_stage2VignetteAlpha, _stage1VignetteAlpha, _stage2AuraAlpha, _stage1AuraAlpha, 
            _stage2ChromaticAberration, _stage1ChromaticAberration, _stage2Exposure, _stage1Exposure, _stage2FadeDuration));
    }


    /// <summary>
    /// Lerps all of the screen effects based on the given parameters.
    /// This function is reused for all of the different stages of these effects.
    /// </summary>
    /// <param name="startVignetteAlpha"> The starting value for the alpha value of the vignette </param>
    /// <param name="endVignetteAlpha"> The ending value for the alpha value of the vignette </param>
    /// <param name="startAuraAlpha"> The starting value for the alpha value of the aura effect </param>
    /// <param name="endAuraAlpha"> The ending value for the alpha value of the aura effect </param>
    /// <param name="startChromAb"> The starting value for the chromatic aberration effect </param>
    /// <param name="endChromAb"> The ending value for the chromatic aberration effect </param>
    /// <param name="startExposure"> The starting value for the post exposure effect </param>
    /// <param name="endExposure"> The ending value for the post exposure effect </param>
    /// <param name="duration"> How long the fade should take </param>
    /// <returns> null </returns>
    private IEnumerator LerpEffects(float startVignetteAlpha, float endVignetteAlpha, float startAuraAlpha, float endAuraAlpha, 
        float startChromAb, float endChromAb, float startExposure, float endExposure, float duration)
    {
        // This float will be updated over time to set the interpolation percentage
        // according the the specified lerp duration
        float time = 0;

        // Create vars to hold the post processing components
        ChromaticAberration chroma;
        ColorAdjustments colAdj;
        Color vigA = _vignette.color;
        Color auraA = _aura.color;

        // Fade all the screen effects over time
        while (time < duration)
        {
            // Set the vignette intensity over time
            vigA.a = Mathf.Lerp(startVignetteAlpha, endVignetteAlpha, time / duration);
            _vignette.color = vigA;

            // Set the aura intensity over time
            auraA.a = Mathf.Lerp(startAuraAlpha, endAuraAlpha, time / duration);
            _aura.color = auraA;

            // Set the chromatic aberration over time
            if (_postProcess.profile.TryGet<ChromaticAberration>(out chroma))
            {
                chroma.intensity.value = Mathf.Lerp(startChromAb, endChromAb, time / duration);
            }

            // Set the post exposure over time
            if (_postProcess.profile.TryGet<ColorAdjustments>(out colAdj))
            {
                colAdj.postExposure.value = Mathf.Lerp(startExposure, endExposure, time / duration);
            }

            // Add the seconds passed to time
            time += Time.deltaTime;

            // Return a null value
            yield return null;
        }


        // Just in case, set all the effects to their end values at the end

        // Vignette alpha
        _vignette.color = new Color(_vignette.color.r, _vignette.color.b, _vignette.color.b, endVignetteAlpha);
        // Aura alpha
        _aura.color = new Color(_aura.color.r, _aura.color.b, _aura.color.b, endAuraAlpha);
        // Chromatic aberration
        if (_postProcess.profile.TryGet<ChromaticAberration>(out chroma))
        {
            chroma.intensity.value = endChromAb;
        }
        // Exposure
        if (_postProcess.profile.TryGet<ColorAdjustments>(out colAdj))
        {
            colAdj.postExposure.value = endExposure;
        }
    }
}
