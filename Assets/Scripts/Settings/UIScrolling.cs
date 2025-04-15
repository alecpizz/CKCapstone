/******************************************************************
*    Author: Nick Grinstead
*    Contributors: 
*    Date Created: April 14th, 2025
*    Description: This script adds functionality to dropdowns to
*       allow for scrolling with the mouse wheel.
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIScrolling : MonoBehaviour
{
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private float _scrollSensitivity = 0.1f;

    private DebugInputActions _inputActions;

    /// <summary>
    /// Initializing input action
    /// </summary>
    private void OnEnable()
    {
        _inputActions = new DebugInputActions();
        _inputActions.UI.Enable();
        _inputActions.UI.Scroll.performed += OnScrollInput;
    }

    /// <summary>
    /// Disabling input action
    /// </summary>
    private void OnDisable()
    {
        _inputActions.UI.Disable();
        _inputActions.UI.Scroll.performed -= OnScrollInput;
    }

    /// <summary>
    /// Moves the position of the scroll rect
    /// </summary>
    private void OnScrollInput(InputAction.CallbackContext context)
    {
        Vector2 newPosition = _scrollRect.normalizedPosition;
        float changeInScroll = context.ReadValue<float>();
        newPosition.y = (int)changeInScroll;
        _scrollRect.normalizedPosition += newPosition.normalized * _scrollSensitivity;
    }
}
