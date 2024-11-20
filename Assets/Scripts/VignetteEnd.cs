using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VignetteEnd : MonoBehaviour, ITurnListener
{
    [SerializeField] float _vignetteFadeOutTime = 0.2f;

    public TurnState TurnState => TurnState.PostProcessing;

    private Vignette _vignette;
    private float _vignetteIntensity;

    /// <summary>
    /// Get vignette that VignetteStart set-up
    /// </summary>
    private void Start()
    {
        VignetteStart startScript = GetComponent<VignetteStart>();
        _vignette = startScript.Vignette;
        _vignetteIntensity = startScript.Intensity;

        RoundManager.Instance.RegisterListener(this);
    }

    private void OnDisable()
    {
        RoundManager.Instance.UnRegisterListener(this);
    }

    public void BeginTurn(Vector3 direction)
    {
        Tween.Custom(_vignetteIntensity, 0f, _vignetteFadeOutTime, newValue => _vignette.intensity.value = newValue)
            .OnComplete(() => RoundManager.Instance.CompleteTurn(this));
    }

    public void ForceTurnEnd()
    {
    }
}
