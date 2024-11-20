/******************************************************************
 *    Author: Nick Grinstead
 *    Contributors: 
 *    Date Created: 11/19/24
 *    Description: Script for playing the vignette fade out effect.
 *      Assumes it's on the same object as a VignetteStart script.
 *******************************************************************/
using PrimeTween;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class VignetteEnd : MonoBehaviour, ITurnListener
{
    [SerializeField] float _vignetteFadeOutTime = 0.2f;

    public TurnState TurnState => TurnState.PostProcessing;

    private Vignette _vignette;
    private float _vignetteIntensity;

    /// <summary>
    /// Grab vignette that VignetteStart set-up
    /// </summary>
    private void Start()
    {
        VignetteStart startScript = GetComponent<VignetteStart>();
        _vignette = startScript.Vignette;
        _vignetteIntensity = startScript.Intensity;

        RoundManager.Instance.RegisterListener(this);
    }

    /// <summary>
    /// Unregister from listener
    /// </summary>
    private void OnDisable()
    {
        RoundManager.Instance.UnRegisterListener(this);
    }

    /// <summary>
    /// Tweens vignette intensity from _vignette intensity to 0.
    /// </summary>
    /// <param name="direction"></param>
    public void BeginTurn(Vector3 direction)
    {
        Tween.Custom(_vignetteIntensity, 0f, _vignetteFadeOutTime, newValue => _vignette.intensity.value = newValue)
            .OnComplete(() => RoundManager.Instance.CompleteTurn(this));
    }

    /// <summary>
    /// Completes turn early
    /// </summary>
    public void ForceTurnEnd()
    {
        RoundManager.Instance.CompleteTurn(this);
    }
}
