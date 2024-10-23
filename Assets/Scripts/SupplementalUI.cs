/******************************************************************
*    Author: David Galmines
*    Contributors: ...
*    Date Created: 10/23/24
*    Description: This is a trigger for supplemental UI to show up 
*    whenever the player comes into contact.
*******************************************************************/
using TMPro;
using UnityEngine;

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

    //an orange sphere that indicates where the trigger is on the
    //canvas; this will help designers when placing it on the grid
    [SerializeField] private GameObject _triggerIndicator;

    //color variable for making the text fade in and out
    private Color color;

    //is the text currently fading out?
    private bool fadingOut;

    /// <summary>
    /// Assigns the color value based on the text UI, and makes it 
    /// transparent.
    /// </summary>
    private void Start()
    {
        color = _text.color;
        color.a = 0;

        //hides the trigger indicator when playing
        _triggerIndicator.SetActive(false);
    }

    /// <summary>
    /// Tells the text to stop fading out when the player initially 
    /// touches the trigger.
    /// </summary>
    /// <param name="other">the player</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && fadingOut)
        {
            fadingOut = false;
        }
    }

    /// <summary>
    /// Tells the text to start fading in if the player is still 
    /// touching the trigger.
    /// </summary>
    /// <param name="other">the player</param>
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (color.a < 1)
            {
                color.a += (Time.deltaTime * _textFadeSpeed);
            }

            //this check is here in case OnTriggerEnter doesn't
            //get detecetd
            if (fadingOut)
            {
                fadingOut = false;
            }
        }
    }

    /// <summary>
    /// Tells the text to start fading out when the player leaves.
    /// </summary>
    /// <param name="other">the player</param>
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            fadingOut = true;
        }
    }

    /// <summary>
    /// Updates transparency of text and orientation of UI.
    /// </summary>
    private void Update()
    {   
        //makes the UI always face the camera if it isn't already
        if (_text.transform.localEulerAngles != 
            _camera.transform.eulerAngles)
        {
            _text.transform.localEulerAngles = 
                _camera.transform.eulerAngles;
        }

        //lowers the opacity of the UI when prompted
        if (fadingOut && color.a > 0)
        {
            color.a -= (Time.deltaTime * _textFadeSpeed);
        }

        //updates the UI text color
        _text.color = color;
    }
}
