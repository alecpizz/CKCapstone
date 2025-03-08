/******************************************************************
 *    Author: Alec Pizziferro
 *    Contributors:  nullptr
 *    Date Created: 3/6/2025
 *    Description: Animation setup for the main menu.
 *    Works based on forwarding unity events through some buttons.
 *******************************************************************/

using System.Collections.Generic;
using FMODUnity;
using PrimeTween;
using SaintsField;
using UnityEngine;
using UnityEngine.EventSystems;

public class CassetteMenuAnimator : MonoBehaviour
{

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
    [SerializeField] private float _lidCloseDuration = 0.2f;
    [SerializeField] private Ease _lidCloseEase = Ease.InCirc;

    [SerializeField] private TweenSettings<Vector3> _lidClose;

    [SepTitle("FMOD Event Refs", EColor.Purple)] [SerializeField]
    private FMODUnity.EventReference _onHoverEvent;

    [SerializeField] private FMODUnity.EventReference _onUnHoverEvent;
    [SerializeField] private FMODUnity.EventReference _onSelectEvent;
    [SerializeField] private FMODUnity.EventReference _onLidCloseEvent;
    
    [SepTitle("Cassette Parts", EColor.Blue)] [SerializeField]
    private Transform _lid;
    [SerializeField] private GameObject _menuCanvas;

    private bool _isPlayingLidAnimation = false;
    private readonly List<CassetteButton> _buttons = new();
    private float _initButtonYPos;
    private float _initLidRotation;

    /// <summary>
    /// Grabs the buttons from the children and caches some values.
    /// </summary>
    private void Awake()
    {
        _buttons.AddRange(GetComponentsInChildren<CassetteButton>());
        EventSystem.current.SetSelectedGameObject(_menuCanvas);
        _initButtonYPos = _buttons[0].transform.localPosition.y;
        _initLidRotation = _lid.transform.localEulerAngles.x;
    }
    
    /// <summary>
    /// Plays the hover button animation.
    /// </summary>
    /// <param name="idx">Index of the button to hover on.</param>
    public void OnHoverButton(int idx)
    {
        if (_isPlayingLidAnimation) return;
        Tween.LocalEulerAngles(_buttons[idx].transform, endValue: new Vector3(_hoverRotate, 0f), ease: _hoverEase,
            duration: _hoverDuration, startValue: new Vector3(0f, 0f));
        Tween.LocalPositionY(_buttons[idx].transform, endValue: _hoverDepth, ease:
            _hoverEase, duration: _hoverDuration).OnComplete(
            () => { _buttons[idx].OnHover?.Invoke(); });
        AudioManager.Instance.PlaySound(_onHoverEvent, new ParamRef()
        {
            Name = "Main Menu",
            Value = 1f
        });
    }

    /// <summary>
    /// Plays the un-hover button animation.
    /// </summary>
    /// <param name="idx">Index of the button to un-hover on.</param>
    public void OnUnHoverButton(int idx)
    {
        if (_isPlayingLidAnimation) return;
        Tween.LocalEulerAngles(_buttons[idx].transform, endValue: new Vector3(0f, 0f), ease: _unHoverEase,
            duration: _unHoverDuration, startValue: new Vector3(_hoverRotate, 0f));
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

    /// <summary>
    /// Plays the select button animation. Can also play
    /// the lid closing animation.
    /// </summary>
    /// <param name="idx">Index of the button to select.</param>
    public void OnSelectButton(int idx)
    {
        if (_isPlayingLidAnimation) return;
        if (!_buttons[idx].PlayClosingAnimation)
        {
            //move it down further
            Tween.LocalPositionY(_buttons[idx].transform, endValue: _selectDepth, ease: _selectEase,
                duration: _selectDuration).OnComplete(() =>
            {
                _buttons[idx].OnClick?.Invoke();
                //pop back up in time
                Tween.LocalPositionY(_buttons[idx].transform, endValue: _initButtonYPos, ease: _unHoverEase,
                    duration: _unHoverDuration);
                Tween.LocalEulerAngles(_buttons[idx].transform, endValue: new Vector3(0f, 0f), ease: _unHoverEase,
                    duration: _unHoverDuration, startValue: new Vector3(_hoverRotate, 0f));
            });
            AudioManager.Instance.PlaySound(_onSelectEvent, new ParamRef()
            {
                Name = "Main Menu",
                Value = 0f
            });
        }
        else
        {
            _isPlayingLidAnimation = true;
            //rotate the button down, then rotate the lid. this isn't realistic, but it makes more sense
            //for gameplay.
            Tween.LocalPositionY(_buttons[idx].transform, endValue: _selectDepth, ease: _selectEase,
                duration: _selectDuration).Chain(Tween.LocalRotation(_lid, startValue: _lid.localRotation,
                endValue: Quaternion.Euler(0f, 0f, 0f),
                ease: _lidCloseEase, duration: _lidCloseDuration)).OnComplete(() =>
            {
                AudioManager.Instance.PlaySound(_onLidCloseEvent);
                _buttons[idx].OnClick?.Invoke();
                _isPlayingLidAnimation = false;
            });
        }
    }
    
}