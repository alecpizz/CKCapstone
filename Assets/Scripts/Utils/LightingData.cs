/******************************************************************
 *    Author: Alec Pizziferro
 *    Contributors:  nullptr
 *    Date Created: 3/5/2025
 *    Description: A class that holds a few specific lighting
 *    parameters.
 *******************************************************************/

using System;
using SaintsField;
using UnityEngine;


[Serializable]
public class LightingData
{
    public bool ApplyLightingData = true;

    [ShowIf(nameof(ApplyLightingData), true)]
    public float AmbientIntensity = 0.5f;

    [ShowIf(nameof(ApplyLightingData), true)]
    public float GodRayAlpha = 1.0f;

    [ShowIf(nameof(ApplyLightingData), true)] [Range(1, 5)] [ShowIf(nameof(ApplyLightingData), true)]
    public int MinimumGridZ = 3;

    [ShowIf(nameof(ApplyLightingData), true)]
    public int CloudSpacing = 2;

    [ShowIf(nameof(ApplyLightingData), true)]
    public float HueMultiplier = 1.0f;

    [ShowIf(nameof(ApplyLightingData), true)]
    public float RainParticleMinSize = 0.01f;

    [ShowIf(nameof(ApplyLightingData), true)]
    public float RainParticleMaxSize = 0.02f;

    [ShowIf(nameof(ApplyLightingData), true)] [Range(0f, 1f)] [ShowIf(nameof(ApplyLightingData), true)]
    public float RainSimulationSpeed = 0.55f;

    [ShowIf(nameof(ApplyLightingData), true)]
    public float RainEmissionRate = 500f;

    [ShowIf(nameof(ApplyLightingData), true)] [Range(0f, 1f)]
    public float RainAlpha = 0.682353f;
}