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

    private bool _isHovered = false;

    private DebugInputActions _inputActions;

    private void OnEnable()
    {
        _inputActions = new DebugInputActions();
        _inputActions.UI.Enable();
        _inputActions.UI.Scroll.performed += OnScrollInput;
    }

    private void OnDisable()
    {
        _inputActions.UI.Disable();
        _inputActions.UI.Scroll.performed -= OnScrollInput;
    }

    private void OnMouseEnter()
    {
        _isHovered = true;
    }

    private void OnMouseExit()
    {
        _isHovered = false;
    }

    private void OnScrollInput(InputAction.CallbackContext context)
    {
        Vector2 newPosition = _scrollRect.normalizedPosition;
        float changeInScroll = context.ReadValue<float>();
        newPosition.y = (int)changeInScroll;
        _scrollRect.normalizedPosition += newPosition.normalized * _scrollSensitivity;

    }
}
