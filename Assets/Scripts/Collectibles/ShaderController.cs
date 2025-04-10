/******************************************************************
*    Author: Trinity Hutson
*    Contributors: None
*    Date Created: 10-22-24
*    Description: Controls the variables of shaders attached to the object. 
*    Specifically created for the ShineEffect Shader subgraph, but can be expanded to other shaders.
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderController : MonoBehaviour
{
    private static bool _HasInstantiatedIDs = false;

    private static int _ShineToggleID = -1;
    private static int _ShineTimeOffsetID = -1;

    [Header("Shine Effect")]
    [SerializeField]
    [Tooltip("Delays the shine effect by this amount in seconds.")]
    private float _initialShineDelay = 10f;

    private bool _shineToggle = true;

    private Renderer[] _renderers;

    /// <summary>
    /// Fetches all renderers on the object and instantiates the shader property IDs, if they haven't been instantiated yet.
    /// If this is on a collectible, it will fetch its order and apply it for the shine delay.
    /// </summary>
    private void Awake()
    {
        if(TryGetComponent(out Collectibles c))
        {
            _initialShineDelay = c.CollectibleNumber + 1;
        }

        _renderers = GetComponentsInChildren<Renderer>();

        InstantiatePropertyIDs();
    }

    /// <summary>
    /// Delays the shine effect if the initial delay was set, otherwise updates the renderers immediately to initiate the shine effect.
    /// </summary>
    private void Start()
    {
        if(_initialShineDelay > 0)
        {
            StartCoroutine(DelayedShineStart());
        }
        else
        {
            UpdateRenderers();
        }
    }

    /// <summary>
    /// Updates the material on each renderer found in the children of this object. 
    /// Sets specific properties of the material's shader to match their respective variables' values.
    /// </summary>
    private void UpdateRenderers()
    {
        foreach (Renderer r in _renderers)
        {
            r.material.SetInt(_ShineToggleID, _shineToggle ? 1 : 0);
            r.material.SetFloat(_ShineTimeOffsetID, _initialShineDelay);
        }
    }

    /// <summary>
    /// Disables the Shine Effect for an amount of seconds equal to _initialShineDelay.
    /// </summary>
    /// <returns></returns>
    private IEnumerator DelayedShineStart()
    {
        _shineToggle = false;
        UpdateRenderers();

        yield return new WaitForSeconds(_initialShineDelay);

        _shineToggle = true;
        UpdateRenderers();
    }

    /// <summary>
    /// Instantiates the IDs of shader properties. Only runs once.
    /// </summary>
    private static void InstantiatePropertyIDs()
    {
        if (_HasInstantiatedIDs)
        {
            return;
        }

        _ShineToggleID = Shader.PropertyToID("_Toggle");
        _ShineTimeOffsetID = Shader.PropertyToID("_TimeOffset");

        _HasInstantiatedIDs = true;
    }

}
