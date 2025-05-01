using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PrimeTween;

public class Floater : MonoBehaviour
{
    [SerializeField]
    private float _floatSpeed = 5;
    [SerializeField]
    private Vector3 _floatDirection = Vector3.up;
    [SerializeField]
    private Ease _easeType = Ease.InSine;

    private Vector3 _startPosition;

    void Start()
    {
        _startPosition = transform.position;
        Tween.PositionAtSpeed(transform, _startPosition + _floatDirection, _floatSpeed, _easeType, cycles: -1, CycleMode.Yoyo);
    }

}
