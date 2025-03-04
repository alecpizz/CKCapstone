/******************************************************************
*    Author: Josephine Qualls
*    Contributors: Josh Eddy, Nick Grinstead
*    Date Created: 10/22/2024
*    Description: Controls where the harmony is facing.
*******************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PrimeTween;
using SaintsField;

/// <summary>
/// Inherits methods from IParentSwitch
/// </summary>
public class HarmonySwitch : MonoBehaviour, IParentSwitch
{
    [SerializeField] private float _rotationDuration = 0.2f;
    [SerializeField] private float _beamToggleDelay = 0.6f;

    [ProgressBar(-270f, 270f, 90f)]
    [SerializeField] private float _rotationDegrees = 180f;

    private bool _shouldActivate = false;

    private HarmonyBeam _beamScript;

    /// <summary>
    /// Registers instance to the RoundManager
    /// </summary>
    private void OnEnable()
    {
        _beamScript = GetComponent<HarmonyBeam>();
    }

    /// <summary>
    /// If this object should take a turn, it will rotate to its active or
    /// inactive position
    /// </summary>
    /// <param name="direction">The direction the player moved</param>
    public void MoveObject()
    {
        _beamScript.ToggleBeam(false);

        Vector3 targetRotation = transform.eulerAngles;
        targetRotation.y += _shouldActivate ? _rotationDegrees : -_rotationDegrees;

        Sequence.Create(1).Chain(
            Tween.Delay(_beamToggleDelay)).Chain(
            Tween.EulerAngles(transform, transform.eulerAngles, targetRotation, _rotationDuration)
            .OnComplete(() => ResetOnTweenEnd()));
    }

    /// <summary>
    /// Helper method to clean up after the rotation is finished
    /// </summary>
    private void ResetOnTweenEnd()
    {
        _beamScript.ToggleBeam(true);
    }

    /// <summary>
    /// Signals the beam that it needs to rotate on its turn
    /// </summary>
    public void SwitchActivation()
    {
        MoveObject();
        _shouldActivate = !_shouldActivate;
    }
}
