/******************************************************************
*    Author: David Galmines
*    Contributors: Rider Hagen
*    Date Created: 10/23/24
*    Description: This is a trigger for supplemental UI to show up 
*    whenever the player comes into contact.
*******************************************************************/
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SupplementalUI : MonoBehaviour
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

    //arrow attatched to the panel
    [SerializeField] private GameObject _arrow;

    //color variable for making the text fade in and out
    private Color _textColor;

    //color variable for the background panel
    private Color _panelColor;

    //image component of the UI panel
    private Image _panelBackground;

    //color variable for the arrow
    private Color _arrowColor;

    //image for arrow attatched to the panel
    private Image _arrowImage;

    //does the text blurb need to appear on a delay?
    [SerializeField] private bool _hasDelay;

    //countdown for delayed text blurbs
    private float _delayTimer;

    /// <summary>
    /// Assigns the color value based on the text UI, and makes it 
    /// transparent.
    /// </summary>
    private void Start()
    {
        _textColor = _text.color;
        _textColor.a = 0;
        _panelBackground = _panel.GetComponent<Image>();
        _arrowImage = _arrow.GetComponent<Image>();
        _panelColor = _panelBackground.color;
        _panelColor.a = 0;
        _arrowColor = _arrowImage.color;
        _arrowColor.a = 0;

        if (_hasDelay)
        {
            _delayTimer = 3.5f;
        }
    }

    /// <summary>
    /// Tells the text to stop fading out when the player initially 
    /// touches the trigger.
    /// </summary>
    /// <param name="other">the player</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && _delayTimer == 0)
        {
            StopAllCoroutines();
            StartCoroutine("FadingIn");
        }
    }

     //Commenting out OnTriggerStay in case a bug shows up involving 
       //player collision

    /// <summary>
    /// Tells the text to start fading in if the player is still 
    /// touching the trigger.
    /// </summary>
    /// <param name="other">the player</param>
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && _hasDelay && _delayTimer == 0)
        {
            StopAllCoroutines();
            StartCoroutine("FadingIn");
        }
    }

    //Commenting out OnTriggerStay in case a bug shows up involving
    //player collision 

    /// <summary>
    /// Tells the text to start fading out when the player leaves.
    /// </summary>
    /// <param name="other">the player</param>
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            StopAllCoroutines();
            StartCoroutine("FadingOut");
        }
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

        while (_arrowColor.a < 0.9f)
        {
            _arrowColor.a += (Time.deltaTime * _textFadeSpeed);
            yield return null;
        }

        while (_textColor.a < 1)
        {
            _textColor.a += (Time.deltaTime * _textFadeSpeed);
            yield return null;
        }
    }

    /// <summary>
    /// Fades the text out.
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadingOut()
    {
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

        while (_arrowColor.a > 0)
        {
            _arrowColor.a -= (Time.deltaTime * _textFadeSpeed);
            yield return null;
        }
    }

    /// <summary>
    /// Updates transparency of text and orientation of UI.
    /// </summary>
    private void Update()
    {
        if (_camera == null || _text == null)
            return;

        //makes the UI always face the camera if it isn't already
        if (_text.transform.localEulerAngles != 
            _camera.transform.eulerAngles)
        {
            _text.transform.localEulerAngles = 
                _camera.transform.eulerAngles;
        }

        //makes the background always face the camera if it isn't already
        if (_panel.transform.localEulerAngles !=
            _camera.transform.eulerAngles)
        {
            _panel.transform.localEulerAngles =
                _camera.transform.eulerAngles;
        }

        if (_arrow.transform.localEulerAngles !=
    _camera.transform.eulerAngles)
        {
            _arrow.transform.localEulerAngles =
                _camera.transform.eulerAngles;
        }

            //updates the UI text color
            _text.color = _textColor;

        //updates background panel color
        _panelBackground.color = _panelColor;

        //updates arrow color
        _arrowImage.color = _arrowColor;

        //the function that delays the text blurb from showing up instantly
        if (_hasDelay && _delayTimer != 0)
        {
            _delayTimer -= (1 * Time.deltaTime);

            if (_delayTimer <= 0)
            {
                _delayTimer = 0;
            }
        }
    }
}
