/******************************************************************
*    Author: David Galmines
*    Contributors: ...
*    Date Created: 11/17/24
*    Description: This makes splash text pop up to indicate a 
*    challenge level
*******************************************************************/
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BonusChallengeDisplay : MonoBehaviour
{
    //this is the UI game object that you want to appear
    [SerializeField] private TMP_Text _text;

    //this is the main camera, the UI will always point towards it
    //relative to its world position
    [SerializeField] private GameObject _camera;

    //this is how fast you want the text to fade in and out
    //ideally a value between 0.5 and 4
    [SerializeField] private float _textFadeSpeed;

    //background panel
    [SerializeField] private GameObject _panel;

    //how quickly do you want the splash text to fade in and out?
    [SerializeField] private float _fadeTime;

    //color variable for making the text fade in and out
    private Color _textColor;

    //color variable for the background panel
    private Color _panelColor;

    //image component of the UI panel
    private Image _panelBackground;

    //are we ready to delete this object?
    private bool _readyToDestroy;

    /// <summary>
    /// Assigns the color value based on the text UI, and makes it 
    /// transparent.
    /// </summary>
    private void Start()
    {
        _textColor = _text.color;
        _textColor.a = 0;
        _panelBackground = _panel.GetComponent<Image>();
        _panelColor = _panelBackground.color;
        _panelColor.a = 0;
        StartCoroutine(FadingIn());
        StartCoroutine(FadingOut());
    }

    /// <summary>
    /// Fades the text in.
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadingIn()
    {
        _panelBackground.canvasRenderer.SetAlpha(0);
        _panelBackground.CrossFadeAlpha(1, _fadeTime, false);

        _text.canvasRenderer.SetAlpha(0);
        _text.CrossFadeAlpha(1, _fadeTime, false);
        yield return null;
    }

    /// <summary>
    /// Fades the text out.
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadingOut()
    {
        yield return new WaitForSeconds(2);
        _readyToDestroy = true;

        _text.canvasRenderer.SetAlpha(1);
        _text.CrossFadeAlpha(0, _fadeTime, false);

        _panelBackground.canvasRenderer.SetAlpha(1);
        _panelBackground.CrossFadeAlpha(0, _fadeTime, false);
        yield return null;
    }
}
