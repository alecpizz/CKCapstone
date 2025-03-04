/******************************************************************
*    Author: Josephine Qualls
*    Contributors: Josh Eddy, Nick Grinstead
*    Date Created: 10/24/2024
*    Description: Controls where the reflection cube reflects the harmony beam.
*******************************************************************/

using PrimeTween;
using SaintsField;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Inherits methods from IParentSwitch
/// </summary>
public class ReflectionSwitch : MonoBehaviour, IParentSwitch
{
    //Assign relevant reflection cube
    [SerializeField] ReflectiveObject _mirror;

    [SerializeField] private float _rotationDuration = 0.2f;
    [SerializeField] private float _beamToggleDelay = 0.6f;

    [ProgressBar(-270f, 270f, 90f)]
    [SerializeField] private float _rotationDegrees = 180f;

    private bool _shouldActivate = false;

    /// <summary>
    /// When switch is on, the reflection will face the opposite direction
    /// </summary>
    public void SwitchActivation()
    {
        MoveObject();
        _shouldActivate = !_shouldActivate;
    }

    /// <summary>
    /// If this object should take a turn, it will flip the direction
    /// of its attached mirror
    /// </summary>
    /// <param name="direction">The direction the player moved</param>
    public void MoveObject()
    {
        _mirror.ToggleBeam(false);

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
        _mirror.CheckForBeamPostRotation();
    }
}
