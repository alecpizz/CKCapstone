using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: Ryan
/// Description: Reflects the harmony wave
/// </summary>
public class ReflectHarmony : MonoBehaviour
{
    [SerializeField] bool _canReflect;
    [SerializeField] List<Vector3> _reflectDirection;

    public List<Vector3> GetReflectDirection()
    {
        return _reflectDirection;
    }

    public bool GetCanReflect()
    {
        return _canReflect;
    }
}
