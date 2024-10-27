/******************************************************************
*    Author: Josephine Qualls
*    Contributors: N/A
*    Date Created: 10/27/2024
*    Description: Particle effect that indicates Ghost Wall location.
*******************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plays ghost wall particle effect
/// </summary>
public class GhostVFX : MonoBehaviour
{
    //Assign the effect to Ghost Wall
    [SerializeField] private ParticleSystem _ghostLights;

    /// <summary>
    /// Play the assigned particle effect
    /// </summary>
    public void Start()
    {
        _ghostLights.Play();
    }
}
