/******************************************************************
*    Author: Alec Pizziferro
*    Contributors: Nullptr
*    Date Created: 11/3/2024
*    Description: Helper utility to serialize interfaces.
*******************************************************************/
using System;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Tool for wrapping up an interface into a serialized field.
/// Use the dot operator to access the interface.
/// </summary>
/// <typeparam name="TInterface">The interfacec to serialize.</typeparam>
[Serializable]
public class InterfaceReference<TInterface> where TInterface : class
{
    [SerializeField]
    private Object _target = null;

    public InterfaceReference()
    {
        _target = null;
    }

    public InterfaceReference(TInterface target)
    {
        Interface = target;
    }

    public Object Target => _target;

    public TInterface Interface
    {
        get => _target as TInterface;
        set => _target = value as Object;
    }
}