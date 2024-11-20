using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VignetteStart : MonoBehaviour, ITurnListener
{
    [SerializeField] float _vignetteFadeInTime = 0.2f;

    public TurnState TurnState => TurnState.Player;

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

    private void Start()
    {
        RoundManager.Instance.RegisterListener(this);
    }

    private void OnDisable()
    {
        RoundManager.Instance.UnRegisterListener(this);
    }

    public void BeginTurn(Vector3 direction)
    {
        Tween.Custom(0f, _vignetteIntensity, _vignetteFadeInTime, newValue => _vignette.intensity.value = newValue)
            .OnComplete(() => RoundManager.Instance.CompleteTurn(this));
    }

    public void ForceTurnEnd()
    {
    }
}
