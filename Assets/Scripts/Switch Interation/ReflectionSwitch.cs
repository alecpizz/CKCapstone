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
public class ReflectionSwitch : MonoBehaviour, IParentSwitch, ITurnListener
{
    //Assign relevant reflection cube
    [SerializeField] ReflectiveObject _mirror;

    [SerializeField] private float _rotationDuration = 0.2f;
    [SerializeField] private float _beamToggleDelay = 0.6f;

    [ProgressBar(-270f, 270f, 90f)]
    [SerializeField] private float _rotationDegrees = 180f;

    public TurnState TurnState => TurnState.World;

    private bool _shouldMoveOnTurn = false;
    private bool _shouldActivate = false;

    /// <summary>
    /// Registers instance to the RoundManager
    /// </summary>
    private void OnEnable()
    {
        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.RegisterListener(this);
        }
    }

    /// <summary>
    /// Unregistering from RoundManager
    /// </summary>
    private void OnDisable()
    {
        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.UnRegisterListener(this);
        }
    }

    /// <summary>
    /// When switch is on, the reflection will face the opposite direction
    /// </summary>
    public void SwitchActivation()
    {
        _shouldMoveOnTurn = true;
        _shouldActivate = true;
    }

    /// <summary>
    /// When switch is off, the reflection will face the original direction
    /// </summary>
    public void SwitchDeactivation()
    {
        _shouldMoveOnTurn = true;
        _shouldActivate = false;
    }

    /// <summary>
    /// If this object should take a turn, it will flip the direction
    /// of its attached mirror
    /// </summary>
    /// <param name="direction">The direction the player moved</param>
    public void BeginTurn(Vector3 direction)
    {
        if (!_shouldMoveOnTurn)
        {
            RoundManager.Instance.CompleteTurn(this);
            return;
        }

        _shouldMoveOnTurn = false;
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
        _mirror.ToggleBeam(true);
        RoundManager.Instance.CompleteTurn(this);
    }

    /// <summary>
    /// Forcibly ends this object's turn
    /// </summary>
    public void ForceTurnEnd()
    {
        _shouldMoveOnTurn = false;
        RoundManager.Instance.CompleteTurn(this);
    }
}
