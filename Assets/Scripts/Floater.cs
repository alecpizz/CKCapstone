/******************************************************************
*    Author: Trinity Hutson
*    Contributors: 
*    Date Created: 5/1/2025
*    Description: Applies a floating effect to the object
*******************************************************************/
using UnityEngine;
using PrimeTween;

public class Floater : MonoBehaviour
{
    [SerializeField]
    private float _floatSpeed = 0.5f;
    [SerializeField]
    private Vector3 _floatDirection = Vector3.up;
    [Space]
    [SerializeField]
    private float _rotationSpeed = 5f;
    [SerializeField]
    private Vector3 _rotationDirection = Vector3.zero;
    [Space]
    [SerializeField]
    private Ease _easeType = Ease.Linear;
    [SerializeField]
    private CycleMode _rotationCycleMode = CycleMode.Yoyo;

    private Vector3 _startPosition;
    private Vector3 _startRotation;

    void Start()
    {
        _startPosition = transform.position;
        _startRotation = transform.rotation.eulerAngles;

        Tween.PositionAtSpeed(transform, _startPosition + _floatDirection, _floatSpeed, _easeType, cycles: -1, CycleMode.Yoyo);
        Tween.RotationAtSpeed(transform, Quaternion.Euler(_startRotation + _rotationDirection), _rotationSpeed, _easeType, cycles: -1, _rotationCycleMode);
    }

}
