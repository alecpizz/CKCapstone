/******************************************************************
*    Author: David Galmines
*    Contributors: David Galmines
*    Date Created: 10/10/24
*    Description: Pulsates a set of arrows around the player.
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerArrows : MonoBehaviour
{
    //The beats per minute of the song in the level
    [SerializeField] private float _bpm;

    //The speed at which you want the arrows to shrink
    [SerializeField] private float _pulseSpeed;

    //The minimum size an arrow can pulsate to
    [SerializeField] private float _minYScale;

    //The maximum size an arrow can pulsate to
    [SerializeField] private float _maxYScale;

    //The 4 arrow objects
    [SerializeField] private GameObject _arrow1;
    [SerializeField] private GameObject _arrow2;
    [SerializeField] private GameObject _arrow3;
    [SerializeField] private GameObject _arrow4;

    //The variable calculated from BPM, beats per second
    private float timingBetweenEachBeat;

    //Is the pulsating coroutine allowed to restart?
    private bool canRestartCoroutine;

    /// <summary>
    /// Calculates the beats per second from BPM, & starts the 
    /// pulse coroutine.
    /// </summary>
    void Start()
    {
        timingBetweenEachBeat = _bpm / 60;
        StartCoroutine("Pulse");
    }

    /// <summary>
    /// Shrinks the y scale of the arrows after they grow depending
    /// on the pulse speed.
    /// </summary>
    void Update()
    {
        //shorthands for arrow scales
        
        //if coroutine is active...
        if (!canRestartCoroutine)
        {
            Vector3 scale1 = _arrow1.transform.localScale;
            Vector3 scaleMod = new Vector3(0, _pulseSpeed * Time.deltaTime,  0f);
           
            // //if arrow 1 is not at its minimum scale...
            if (scale1.y >= _minYScale)
            {
                //...shrink its scale
                _arrow1.transform.localScale -= scaleMod;
                _arrow2.transform.localScale -= scaleMod;
                _arrow3.transform.localScale -= scaleMod;
                _arrow4.transform.localScale -= scaleMod;
            }
        }

        //if coroutine is not active...
        if (canRestartCoroutine)
        {
            //...activate it
            StartCoroutine("Pulse");
        }
    }

    /// <summary>
    /// Makes the arrows pulsate to the beat of the song.
    /// </summary>
    /// <returns>null</returns>
    private IEnumerator Pulse()
    {
        //coroutine is active
        canRestartCoroutine = false;
        //grow the arrows to max size
        _arrow1.transform.localScale = new Vector3(0.23f, _maxYScale, 
            0.63f);
        _arrow2.transform.localScale = new Vector3(0.23f, _maxYScale, 
            0.63f);
        _arrow3.transform.localScale = new Vector3(0.23f, _maxYScale, 
            0.63f);
        _arrow4.transform.localScale = new Vector3(0.23f, _maxYScale, 
            0.63f);
        //wait depending on the BPM to end the coroutine
        //beats per second is converted to seconds per beat
        yield return new WaitForSeconds(1 / timingBetweenEachBeat);
        //coroutine is no longer active
        canRestartCoroutine = true;
        yield return null;
    }
}
