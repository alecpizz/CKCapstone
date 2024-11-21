/******************************************************************
 *    Author: Nick Grinstead
 *    Contributors: 
 *    Date Created: 11/19/24
 *    Description: Script for playing the vignette fade in and out effect.
 *      Assumes it's on the same object as a volume object.
 *******************************************************************/
using PrimeTween;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VignetteController : MonoBehaviour, ITurnListener
{
    [SerializeField] float _vignetteFadeInTime = 0.2f;
    [SerializeField] Ease _vignetteStartEasing = Ease.InQuad;

    [SerializeField] float _vignetteFadeOutTime = 0.2f;
    [SerializeField] Ease _vignetteEndEasing = Ease.OutQuad;

    public TurnState TurnState => _internalState;
    private TurnState _internalState = TurnState.Player;

    public Vignette Vignette  => _vignette;
    private Vignette _vignette;

    public float Intensity => _vignetteIntensity;
    [SerializeField] private float _vignetteIntensity = 0.4f;

    /// <summary>
    /// Initialize Vignette
    /// </summary>
    private void Awake()
    {
        _vignette = GetComponent<Volume>().profile.Add<Vignette>();
        _vignette.active = true;
        _vignette.color.overrideState = _vignette.center.overrideState =
            _vignette.intensity.overrideState = _vignette.smoothness.overrideState =
            _vignette.rounded.overrideState = true;
    }

    /// <summary>
    /// Register to round manager
    /// </summary>
    private void Start()
    {
        RoundManager.Instance.RegisterListener(this);
    }

    /// <summary>
    /// Unregister from round manager
    /// </summary>
    private void OnDisable()
    {
        RoundManager.Instance.UnRegisterListener(this);
    }

    /// <summary>
    /// Tweens vignette intensity in or out based on turn state
    /// </summary>
    /// <param name="direction"></param>
    public void BeginTurn(Vector3 direction)
    {
        if (_internalState == TurnState.Player)
        {
            Tween.Custom(0f, _vignetteIntensity, _vignetteFadeInTime,
                newValue => _vignette.intensity.value = newValue, _vignetteStartEasing)
                .OnComplete(() => ToggleTurnState());
        }
        else
        {
            Tween.Custom(_vignetteIntensity, 0f, _vignetteFadeOutTime, newValue => _vignette.intensity.value = newValue)
            .OnComplete(() => ToggleTurnState());
        }
    }

    /// <summary>
    /// Ends the current turn before re-registering to RoundManager as a new turn state.
    /// </summary>
    private void ToggleTurnState()
    {
        RoundManager.Instance.CompleteTurn(this);
        RoundManager.Instance.UnRegisterListener(this);
        _internalState = _internalState == TurnState.Player ? TurnState.PostProcessing : TurnState.Player;
        RoundManager.Instance.RegisterListener(this);
    }

    /// <summary>
    /// Completes turn early
    /// </summary>
    public void ForceTurnEnd()
    {
        RoundManager.Instance.CompleteTurn(this);
    }
}
