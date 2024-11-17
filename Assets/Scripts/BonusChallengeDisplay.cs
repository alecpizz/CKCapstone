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

        Invoke("InitiateFadeOut", 2);
    }

    /// <summary>
    /// Fades the text in.
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadingIn()
    {
        while (_panelColor.a < 0.9f)
        {
            _panelColor.a += (Time.deltaTime * _textFadeSpeed);
            yield return null;
        }

        while (_textColor.a < 1)
        {
            _textColor.a += (Time.deltaTime * _textFadeSpeed);
            yield return null;
        }
    }

    /// <summary>
    /// Function gets invoked to start coroutine because invoking a coroutine 
    /// didn't work for me.
    /// </summary>
    private void InitiateFadeOut()
    {
        StartCoroutine(FadingOut());
    }

    /// <summary>
    /// Tells the UI element to self-destruct after fading onto screen.
    /// </summary>
    private void Update()
    {
        if (_textColor.a == 0 && _readyToDestroy)
        {
            Destroy(gameObject);
        }

        //updates the UI text color
        _text.color = _textColor;

        //updates background panel color
        _panelBackground.color = _panelColor;
    }

    /// <summary>
    /// Fades the text out.
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadingOut()
    {
        _readyToDestroy = true;

        while (_textColor.a > 0)
        {
            _textColor.a -= (Time.deltaTime * _textFadeSpeed);
            yield return null;
        }

        while (_panelColor.a > 0)
        {
            _panelColor.a -= (Time.deltaTime * _textFadeSpeed);
            yield return null;
        }
    }
}
