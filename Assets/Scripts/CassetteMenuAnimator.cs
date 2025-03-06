using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using PrimeTween;
using SaintsField;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CassetteMenuAnimator : MonoBehaviour
{
    [SerializeField] private GameObject _menuCanvas;

    [SepTitle("Tweens", EColor.Blue)] [SerializeField]
    private float _hoverDepth = 0.115f;

    [SerializeField] private float _hoverRotate;
    [SerializeField] private float _selectDepth = 0.0873f;
    [SerializeField] private Ease _hoverEase = Ease.InCirc;
    [SerializeField] private Ease _unHoverEase = Ease.OutCirc;
    [SerializeField] private Ease _selectEase = Ease.InCirc;
    [SerializeField] private float _hoverDuration = 0.2f;
    [SerializeField] private float _unHoverDuration = 0.2f;
    [SerializeField] private float _selectDuration = 0.2f;

    [SerializeField] private TweenSettings<Vector3> _lidClose;

    [SepTitle("FMOD Event Refs", EColor.Purple)] [SerializeField]
    private FMODUnity.EventReference _onHoverEvent;

    [SerializeField] private FMODUnity.EventReference _onUnHoverEvent;
    [SerializeField] private FMODUnity.EventReference _onSelectEvent;

    [SepTitle("Cassette Parts", EColor.Blue)] [SerializeField]
    private Transform _lid;

    private readonly List<CassetteButton> _buttons = new();
    private float _initButtonYPos;

    private void Awake()
    {
        _buttons.AddRange(GetComponentsInChildren<CassetteButton>());
        EventSystem.current.SetSelectedGameObject(_menuCanvas);
        _initButtonYPos = _buttons[0].transform.localPosition.y;
    }

    public void OnHoverButton(int idx)
    {
        Tween.LocalPositionY(_buttons[idx].transform, endValue: _hoverDepth, ease:
            _hoverEase, duration: _hoverDuration).OnComplete(
            () => { _buttons[idx].OnHover?.Invoke(); });
        AudioManager.Instance.PlaySound(_onHoverEvent, new ParamRef()
        {
            Name = "Main Menu",
            Value = 1f
        });
    }

    public void OnUnHoverButton(int idx)
    {
        Tween.LocalPositionY(_buttons[idx].transform, endValue: _initButtonYPos,
            ease: _unHoverEase, duration: _unHoverDuration).OnComplete(() =>
        {
            _buttons[idx].OnUnHover?.Invoke();
        });
        AudioManager.Instance.PlaySound(_onUnHoverEvent, new ParamRef()
        {
            Name = "Main Menu",
            Value = 1f
        });
    }

    public void OnSelectButton(int idx)
    {
        Tween.LocalPositionY(_buttons[idx].transform, endValue: _selectDepth, ease: _selectEase,
            duration: _selectDuration).OnComplete(() =>
        {
            _buttons[idx].OnClick?.Invoke();
            Tween.LocalPositionY(_buttons[idx].transform, endValue: _initButtonYPos, ease: _unHoverEase,
                duration: _unHoverDuration);
        });
        AudioManager.Instance.PlaySound(_onSelectEvent, new ParamRef()
        {
            Name = "Main Menu",
            Value = 0f
        });
    }
}