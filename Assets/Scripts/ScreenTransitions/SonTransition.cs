/******************************************************************
*    Author: Zayden Joyner
*    Contributors: 
*    Date Created: 4/23/25
*    Description: Fades in and out a screen effect when the player is hugged by the son.
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class SonTransition : ScreenTransitionBase
{
    [Header("Adjustable Values")]
    [Tooltip("How long the white screen should take to fade in")]
    [SerializeField] private float _whiteFadeIn = 1.5f;
    [Tooltip("How long the white screen should take to fade out")]
    [SerializeField] private float _whiteFadeOut = 1.5f;

    [Header("Object References")]
    [Tooltip("The white screen UI image")]
    [SerializeField] private GameObject _whiteObj;
    [Tooltip("The music note particles")]
    [SerializeField] private GameObject _noteParticles;

    // Internal references
    private Image _white;
    private ParticleSystem _notes;

    /// <summary>
    /// Assign references
    /// </summary>
    private void Start()
    {
        _white = _whiteObj.GetComponent<Image>();
        _notes = _noteParticles.GetComponent<ParticleSystem>();
    }

    /// <summary>
    /// Fade in from nothing. Call this when the screen should fade in.
    /// </summary>
    public override void FadeIn()
    {
        // Reset the screen color
        _white.color = new Color(_white.color.r, _white.color.b, _white.color.b, 0f);

        // Play the note particles
        _noteParticles.SetActive(true);
        _notes.Play();

        // Start fading in the white screen
        StartCoroutine(LerpEffects(_white, _whiteFadeIn));
    }

    /// <summary>
    /// Fade out from white.  Call this when the screen should fade out.
    /// </summary>
    public override void FadeOut()
    {
        StartCoroutine(FadeFromWhite());
    }

/// <summary>
/// Fades any effects (white screen) in from zero opacity
/// </summary>
/// <param name="img"> The screen overlay image to fade </param>
/// <param name="duration"> How long the fade should take </param>
/// <returns> null </returns>
    private IEnumerator LerpEffects(Image img, float duration)
    {

        // This float will be updated over time to set the interpolation percentage
        // according the the specified lerp duration
        float time = 0;

        // Get the color property of the image
        Color colA = img.color;

        // Fade any screen effects over time
        while (time < duration)
        {

            // Set the image color over time
            colA.a = Mathf.Lerp(0f, 1f, time / duration);
            img.color = colA;

            // Add the seconds passed to time
            time += Time.deltaTime;

            // Return a null value
            yield return null;
        }

        // Just in case, set any effects to their end values at the end
        img.color = new Color(img.color.r, img.color.b, img.color.b, 1f);

        // Turn off net particles
        _noteParticles.SetActive(false);
    }

    /// <summary>
    /// Fades the screen out from white to nothing
    /// </summary>
    /// <returns> null </returns>
    private IEnumerator FadeFromWhite()
    {
        // This float will be updated over time to set the interpolation percentage
        // according the the specified lerp duration
        float time = 0;

        // Get the color property of the image
        Color whiteA = _white.color;

        // Fade the white screen out over time
        while (time < _whiteFadeOut)
        {

            // Set the image color over time
            whiteA.a = Mathf.Lerp(1f, 0f, time / _whiteFadeOut);
            _white.color = whiteA;

            // Add the seconds passed to time
            time += Time.deltaTime;

            // Return a null value
            yield return null;
        }

        // Just in case, set the white screen to zero
        _white.color = new Color(_white.color.r, _white.color.b, _white.color.b, 0f);
    }
}
