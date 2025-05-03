/******************************************************************
*    Author: Zayden Joyner
*    Contributors: 
*    Date Created: 4/29/25
*    Description: Fades the to/from a black screen.
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class FadeToBlack : SceneTransitionBase
{
    [Header("Adjustable Values")]
    [Tooltip("How long the screen should take to fade in")]
    [SerializeField] private float _fadeIn = 1.5f;
    [Tooltip("How long the screen should take to fade out")]
    [SerializeField] private float _fadeOut = 1.5f;

    [Header("Object References")]
    [Tooltip("The black screen UI image")]
    [SerializeField] private GameObject _blackScreenObj;

    // Internal references
    private Image _blackScreen;
    private bool _inProgress = false;

    public override void Init()
    {
        _blackScreen = _blackScreenObj.GetComponent<Image>();
    }

    public override bool InProgress()
    {
        return _inProgress;
    }

    /// <summary>
    /// Fade in screen effeects from nothing.  Call this when the screen should fade in.
    /// </summary>
    public override void FadeIn()
    {
        // Reset all color properties
        _blackScreen.color = new Color(_blackScreen.color.r, _blackScreen.color.b, _blackScreen.color.b, 0f);

        // Fade the screen in over different durations of time
        StartCoroutine(LerpEffects(_blackScreen, _fadeIn, 0, 1));
    }

    /// <summary>
    /// Fade out screen effects.  Call this when the screen should fade out.
    /// </summary>
    public override void FadeOut()
    {
        StartCoroutine(LerpEffects(_blackScreen, _fadeOut, 1, 0));
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
        _inProgress = true;
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
        _inProgress = false;
    }
}
