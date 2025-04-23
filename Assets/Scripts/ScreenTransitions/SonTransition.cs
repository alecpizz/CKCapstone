/******************************************************************
*    Author: Zayden Joyner
*    Contributors: 
*    Date Created: 4/23/25
*    Description: Fades the son screen in, then fades to white.
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class SonTransition : MonoBehaviour
{
    [Header("Test Controls")]
    [SerializeField] private bool _testFadeIn = false;
    [SerializeField] private bool _testFadeOut = false;

    [SerializeField] private float _notesFade1 = .2f;
    [SerializeField] private float _notesFade2 = .2f;
    [SerializeField] private float _notesFade3 = .2f;
    [SerializeField] private float _whiteFade = 1.5f;

    [Header("Object References")]
    [SerializeField] private GameObject _noteImg1;
    [SerializeField] private GameObject _noteImg2;
    [SerializeField] private GameObject _noteImg3;
    [SerializeField] private GameObject _whiteObj;

    private Image _notes1;
    private Image _notes2;
    private Image _notes3;
    private Image _white;

    /// <summary>
    /// Assign references
    /// </summary>
    private void Start()
    {
        _notes1 = _noteImg1.GetComponent<Image>();
        _notes2 = _noteImg2.GetComponent<Image>();
        _notes3 = _noteImg3.GetComponent<Image>();
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
            _notes1.color = new Color(_notes1.color.r, _notes1.color.b, _notes1.color.b, 0f);
            _notes2.color = new Color(_notes2.color.r, _notes2.color.b, _notes2.color.b, 0f);
            _notes3.color = new Color(_notes3.color.r, _notes3.color.b, _notes3.color.b, 0f);
            _white.color = new Color(_white.color.r, _white.color.b, _white.color.b, 0f);

            ScreenFade();

            _testFadeIn = false;
        }

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
        // Start the LerpEffect coroutine with the relevant values
        StartCoroutine(LerpEffects(_white, _whiteFade));
        StartCoroutine(LerpEffects(_notes1, _notesFade1));
        StartCoroutine(LerpEffects(_notes2, _notesFade2));
        StartCoroutine(LerpEffects(_notes3, _notesFade3));
    }

    public void FadeOut()
    {
        StartCoroutine(FadeFromWhite());
    }

    /// <summary>
    /// Lerps all of the screen effects based on the given parameters.
    /// </summary>
    /// <returns> null </returns>
    private IEnumerator LerpEffects(Image img, float duration)
    {

        // This float will be updated over time to set the interpolation percentage
        // according the the specified lerp duration
        float time = 0;

        Color colA = img.color;

        // Fade all the screen effects over time
        while (time < duration)
        {

            // Set the aura intensity over time
            colA.a = Mathf.Lerp(0f, 1f, time / duration);
            img.color = colA;

            // Add the seconds passed to time
            time += Time.deltaTime;

            // Return a null value
            yield return null;
        }

        // Just in case, set all the effects to their end values at the end
        img.color = new Color(img.color.r, img.color.b, img.color.b, 1f);
    }

    private IEnumerator FadeFromWhite()
    {
        _notes1.color = new Color(_notes1.color.r, _notes1.color.b, _notes1.color.b, 0f);
        _notes2.color = new Color(_notes2.color.r, _notes2.color.b, _notes2.color.b, 0f);
        _notes3.color = new Color(_notes3.color.r, _notes3.color.b, _notes3.color.b, 0f);

        float time = 0;

        Color whiteA = _white.color;

        while (time < _whiteFade)
        {

            // Set the aura intensity over time
            whiteA.a = Mathf.Lerp(1f, 0f, time / _whiteFade);
            _white.color = whiteA;

            // Add the seconds passed to time
            time += Time.deltaTime;

            // Return a null value
            yield return null;
        }

        _white.color = new Color(_white.color.r, _white.color.b, _white.color.b, 0f);
    }
}
