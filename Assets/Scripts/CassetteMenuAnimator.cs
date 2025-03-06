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

    private readonly List<CassetteButton> _buttons = new();

    private void Awake()
    {
       _buttons.AddRange(GetComponentsInChildren<CassetteButton>());
       EventSystem.current.SetSelectedGameObject(_menuCanvas);
    }
    public void OnHoverButton(int idx)
    {
        Tween.LocalPositionY(_buttons[idx].transform, _onHover);
        AudioManager.Instance.PlaySound(_onHoverEvent);
        // _buttons[idx].OnHover?.Invoke();
    }

    public void OnUnHoverButton(int idx)
    {
        Tween.LocalPositionY(_buttons[idx].transform, _offHover);
        AudioManager.Instance.PlaySound(_onUnHoverEvent);
        // _buttons[idx].OnUnHover?.Invoke();
    }

    public void OnSelectButton(int idx)
    {
        Tween.LocalPositionY(_buttons[idx].transform, _onSelect);
        AudioManager.Instance.PlaySound(_onSelectEvent, new ParamRef()
        {
            Name = "Main Menu",
            Value = 0f
        });
    }
}