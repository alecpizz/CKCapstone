/******************************************************************
*    Author: Trinity Hutson
*    Contributors: 
*    Date Created: 5/4/2025
*    Description: Scrolls a canvas component for rolling credits
*******************************************************************/
using PrimeTween;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Utilities;

public class CreditsScroll : MonoBehaviour
{
    [SerializeField]
    RectTransform _creditsTransform;
    [Space]
    [SerializeField, Range(1, 100)]
    float _scrollSpeed = 50;
    [SerializeField]
    float _skipSpeedMult = 5;
    [Space]
    [SerializeField]
    float _creditsLength = 1000;

    float _scrollDuration;

    Tween _scrollTween;


    /// <summary>
    /// Initializes how long the credits scroll for
    /// </summary>
    private void Awake()
    {
        _scrollDuration = 1000 / _scrollSpeed;
    }

    private void Start()
    {
        ScrollCredits();
    }

    /// <summary>
    /// Detects if the player is pressing any input, and speeds up the credits if so
    /// </summary>
    private void Update()
    {
        if (!_scrollTween.isAlive)
            return;

        foreach (var device in InputSystem.devices)
        {
            foreach (var control in device.allControls)
            {
                if (control is ButtonControl button && button.isPressed)
                {
                    _scrollTween.timeScale = _skipSpeedMult;
                    return;
                }
            }
        }

        _scrollTween.timeScale = 1;
    }

    /// <summary>
    /// Creates the tween that controls the credit scrolling
    /// </summary>
    private void ScrollCredits()
    {
        _scrollTween = Tween.UIAnchoredPosition3DY(_creditsTransform,
            _creditsLength, _scrollDuration, ease: Ease.Linear)
                .OnComplete(() =>
                {
                    SceneController.Instance.LoadNewScene(0, SceneTransitionType.Black);
                });
    }
}
