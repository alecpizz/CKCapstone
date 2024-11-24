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
    [SerializeField] private float _vignetteFadeInTime = 0.2f;
    [SerializeField] private Ease _vignetteStartEasing = Ease.InQuad;

    [SerializeField] private float _vignetteFadeOutTime = 0.2f;
    [SerializeField] private Ease _vignetteEndEasing = Ease.OutQuad;

    public TurnState TurnState => _internalState;
    private TurnState _internalState = TurnState.Player;

    private Vignette _vignette;

    [SerializeField] private float _vignetteIntensity = 0.4f;
    [SerializeField] private float _vignetteSmoothness = 0.1f;
    [SerializeField] private Color _vignetteColor = Color.red;

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
        _vignette.smoothness.value = _vignetteSmoothness;
        _vignette.color.value = _vignetteColor;
    }

    /// <summary>
    /// Register to round manager
    /// </summary>
    private void Start()
    {
        _vignette.active = RoundManager.Instance.EnemiesPresent;

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
                newValue => _vignette.intensity.value = newValue, _vignetteStartEasing, 1, CycleMode.Restart,
                0.0f, 0.2f)
                .OnComplete(() => ToggleTurnState());
        }
        else
        {
            Tween.Custom(_vignetteIntensity, 0f, _vignetteFadeOutTime, newValue => _vignette.intensity.value = newValue, _vignetteEndEasing, 1, CycleMode.Restart,
                0.2f, 0.0f)
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
        _internalState = _internalState == TurnState.Player ? TurnState.Enemy : TurnState.Player;
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
