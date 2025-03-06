using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using PrimeTween;
using SaintsField;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class CassetteMenuAnimator : MonoBehaviour
{
    [SepTitle("Tweens", EColor.Blue)] [SerializeField]
    private TweenSettings<float> _onHover;

    [SerializeField] private TweenSettings<float> _offHover;
    [SerializeField] private TweenSettings<float> _onSelect;
    [SerializeField] private TweenSettings<Vector3> _lidClose;

    [SepTitle("FMOD Event Refs", EColor.Purple)] [SerializeField]
    private FMODUnity.EventReference _onHoverEvent;

    [SerializeField] private FMODUnity.EventReference _onUnHoverEvent;
    [SerializeField] private FMODUnity.EventReference _onSelectEvent;

    [SepTitle("Cassette Parts", EColor.Blue)] [SerializeField]
    private Transform _lid;

    private bool _inputBlocked = false;
    private Camera _cam;
    private CassetteButton _currentSelectedButton;
    private Collider _currentHoverTarget = null;
    private readonly List<CassetteButton> _buttons = new();
    private int _selectedButtonIndex = 0;
    private PlayerControls _playerControls;
    private bool _expectingMouseInput = true;

    private void Awake()
    {
        _cam = Camera.main;
        _playerControls = new PlayerControls();
        if (_buttons.Count == 0)
        {
            _buttons.AddRange(GetComponentsInChildren<CassetteButton>());
        }

        _currentSelectedButton = _buttons[_selectedButtonIndex];
        Tween.LocalPositionY(_currentSelectedButton.transform, _onHover);
    }

    private void OnEnable()
    {
        _playerControls.Enable();
        _playerControls.InGame.SelectMenuItem1.performed += SelectMenuItem1Onperformed;
        _playerControls.InGame.Movement.performed += MovementOnperformed;
    }


    private void OnDisable()
    {
        _playerControls.InGame.SelectMenuItem1.performed -= SelectMenuItem1Onperformed;
        _playerControls.InGame.Movement.performed -= MovementOnperformed;
        _playerControls.Disable();
    }

    private void MovementOnperformed(InputAction.CallbackContext ctx)
    {
        if (_inputBlocked) return;
        _expectingMouseInput = false;
        var input = ctx.ReadValue<Vector2>();
        if (Mathf.Abs(input.x) <= 0.1f) //only allow l/r inputs
        {
            return;
        }

        int sign = (int)Mathf.Sign(input.x);
        _selectedButtonIndex += sign;
        if (_selectedButtonIndex >= _buttons.Count)
        {
            _selectedButtonIndex = 0;
        }
        else if (_selectedButtonIndex < 0)
        {
            _selectedButtonIndex = _buttons.Count - 1;
        }

        if (_currentSelectedButton != null)
        {
            OnUnHoverButton();
        }

        _currentSelectedButton = _buttons[_selectedButtonIndex];
        OnHoverButton();
    }

    private void SelectMenuItem1Onperformed(InputAction.CallbackContext ctx)
    {
        if (_inputBlocked) return;
        if (_currentSelectedButton == null) return;
        if (_expectingMouseInput && _currentHoverTarget == null) return;
        OnSelectButton();
        if (_currentSelectedButton.PlayClosingAnimation) return; //will set at end of animation
        _currentHoverTarget = null;
        _currentSelectedButton = null;
    }

    private void OnSelectButton()
    {
        Tween.LocalPositionY(_currentSelectedButton.transform, _onSelect);
        AudioManager.Instance.PlaySound(_onSelectEvent, new ParamRef()
        {
            Name = "Main Menu",
            Value = 0f
        });

        if (_currentSelectedButton.PlayClosingAnimation)
        {
            _inputBlocked = true;
            Tween.LocalEulerAngles(_lid, _lidClose).OnComplete(() =>
            {
                _currentSelectedButton.OnClick?.Invoke();
                _inputBlocked = false;
                _currentSelectedButton = null;
                _currentHoverTarget = null;
            });
        }
        else
        {
            _currentSelectedButton.OnClick?.Invoke();
            _inputBlocked = false;
        }
    }

    private void OnHoverButton()
    {
        Tween.LocalPositionY(_currentSelectedButton.transform, _onHover);
        AudioManager.Instance.PlaySound(_onHoverEvent);
        _currentSelectedButton.OnHover?.Invoke();
    }

    private void OnUnHoverButton()
    {
        Tween.LocalPositionY(_currentSelectedButton.transform, _offHover);
        AudioManager.Instance.PlaySound(_onUnHoverEvent, new ParamRef()
        {
            Name = "Main Menu",
            Value = 1f
        });
        _currentSelectedButton.OnUnHover?.Invoke();
    }

    private void FixedUpdate()
    {
        if (_inputBlocked) return;
        var cursorPos = _cam.ScreenPointToRay(Mouse.current.position.value);
        if (Physics.Raycast(cursorPos, out RaycastHit hit, 100f))
        {
            _expectingMouseInput = true;
            if (!hit.collider.TryGetComponent(out CassetteButton button))
            {
                if (_currentSelectedButton != null)
                {
                    OnUnHoverButton();
                }

                _currentSelectedButton = null;
                _currentHoverTarget = null;
                return;
            }

            _currentHoverTarget = hit.collider;
            if (_currentSelectedButton != null && _currentSelectedButton != button)
            {
                OnUnHoverButton();
                _currentSelectedButton = button;
                _selectedButtonIndex = _buttons.IndexOf(button);
                OnHoverButton();
            }
            else if (_currentSelectedButton == null)
            {
                _currentSelectedButton = button;
                _selectedButtonIndex = _buttons.IndexOf(button);
                OnHoverButton();
            }
        }
        else
        {
            if (_currentSelectedButton == null || !_expectingMouseInput) return;
            OnUnHoverButton();
            _currentSelectedButton = null;
            _currentSelectedButton = null;
        }
    }
}