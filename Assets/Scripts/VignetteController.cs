/******************************************************************
 *    Author: Nick Grinstead
 *    Contributors: Trinity Hutson
 *    Date Created: 11/19/24
 *    Description: Script for playing the vignette fade in and out effect.
 *      Assumes it's on the same object as a volume object.
 *******************************************************************/
using PrimeTween;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using SaintsField;
using SaintsField.Playa;
using System;

public class VignetteController : MonoBehaviour, ITurnListener
{
    public static Action<bool> InteractionTriggered;

    public TurnState TurnState => TurnState.Player;
    public TurnState SecondaryTurnState => TurnState.Enemy;

    private Vignette _vignette;

    [SerializeField] private Color _vignetteColor = Color.red;

    [Space]

    [InfoBox("Movement Vignette Settings")]
    [SerializeField] private float _vignetteFadeInTime = 0.2f;
    [SerializeField] private Ease _vignetteStartEasing = Ease.InQuad;

    [SerializeField] private float _vignetteFadeOutTime = 0.2f;
    [SerializeField] private Ease _vignetteEndEasing = Ease.OutQuad;

    [SerializeField] private float _vignetteIntensity = 0.4f;
    [SerializeField] private float _vignetteSmoothness = 0.1f;

    [SerializeField] private float _vignetteEndDelay = 0.2f;

    [Space]

    [InfoBox("Interaction Vignette Settings")]
    [SerializeField] private float _interactableFadeInTime = 0.4f;
    [SerializeField] private Ease _interactableStartEasing = Ease.InQuad;

    [SerializeField] private float _interactableFadeOutTime = 0.4f;
    [SerializeField] private Ease _interactableEndEasing = Ease.OutQuad;

    [SerializeField] private float _interactableVignetteIntensity = 0.7f;
    [SerializeField] private float _interactableVignetteSmoothness = 0.9f;

    /// <summary>
    /// Initialize Vignette
    /// </summary>
    private void Awake()
    {
        InteractionTriggered += ToggleInteractableVignette;
        
        _vignette = GetComponent<Volume>().profile.Add<Vignette>();
        _vignette.active = true;
        _vignette.color.overrideState = _vignette.center.overrideState =
            _vignette.intensity.overrideState = _vignette.smoothness.overrideState =
            _vignette.rounded.overrideState = true;
        _vignette.smoothness.value = _vignetteSmoothness;
        _vignette.color.value = _vignetteColor;
    }

    /// <summary>
    /// Register to round manager
    /// </summary>
    private void Start()
    {
        if (RoundManager.Instance == null)
            return;

        bool enemies = RoundManager.Instance.EnemiesPresent;
        _vignette.active = enemies;
        if (!enemies) return;
        RoundManager.Instance.RegisterListener(this);
    }

    /// <summary>
    /// Unregister from round manager
    /// </summary>
    private void OnDisable()
    {
        InteractionTriggered -= ToggleInteractableVignette;

        if (RoundManager.Instance == null)
            return;

        RoundManager.Instance.UnRegisterListener(this);
    }

    /// <summary>
    /// Tweens vignette intensity in or out based on turn state
    /// </summary>
    /// <param name="direction"></param>
    public void BeginTurn(Vector3 direction)
    {
        if (RoundManager.Instance.IsPlayerTurn)
        {
            Tween.Custom(_vignette.intensity.value, _vignetteIntensity, _vignetteFadeInTime,
                newValue => _vignette.intensity.value = newValue,
                _vignetteStartEasing, 1, CycleMode.Restart,
                0.0f, _vignetteEndDelay)
                .OnComplete(() => ToggleTurnState());
        }
        else
        {
            ToggleTurnState();
            Tween.Custom(_vignette.intensity.value, 0f, _vignetteFadeOutTime, 
                newValue => _vignette.intensity.value = newValue,
                _vignetteEndEasing, 1, CycleMode.Restart,
                _vignetteEndDelay, 0.0f);
        }
    }

    /// <summary>
    /// Ends the current turn before re-registering to RoundManager as a new turn state.
    /// </summary>
    private void ToggleTurnState()
    {
        if (RoundManager.Instance == null)
            return;

        RoundManager.Instance.CompleteTurn(this);
    }

    /// <summary>
    /// Completes turn early
    /// </summary>
    public void ForceTurnEnd()
    {
        if (RoundManager.Instance == null)
            return;

        RoundManager.Instance.CompleteTurn(this);
    }

    /// <summary>
    /// Invoked to toggle the vignette for interacting with story objects
    /// on or off
    /// </summary>
    /// <param name="isActive">True if the vignette should toggle on</param>
    private void ToggleInteractableVignette(bool isActive)
    {
        if (isActive)
        {
            Sequence.Create(1).Group(
                Tween.Custom(_vignette.intensity.value, _interactableVignetteIntensity,
                _interactableFadeInTime, newIntensity => _vignette.intensity.value = newIntensity,
                _interactableStartEasing, 1, CycleMode.Restart,
                0.0f, 0.2f)).Group(
                Tween.Custom(_vignette.smoothness.value, _interactableVignetteSmoothness,
                _interactableFadeInTime, newSmoothness => _vignette.smoothness.value = newSmoothness,
                _interactableStartEasing, 1, CycleMode.Restart,
                0.0f, 0.2f));
        }
        else
        {
            Sequence.Create(1).Group(
                Tween.Custom(_vignette.intensity.value, 0f, _interactableFadeOutTime,
                newIntensity => _vignette.intensity.value = newIntensity,
                _interactableEndEasing, 1, CycleMode.Restart,
                0.0f, 0.2f)).Group(
                Tween.Custom(_vignette.smoothness.value, _vignetteSmoothness, _interactableFadeOutTime,
                newSmoothness => _vignette.smoothness.value = newSmoothness,
                _interactableEndEasing, 1, CycleMode.Restart,
                0.0f, 0.2f));
        }
    }
}
