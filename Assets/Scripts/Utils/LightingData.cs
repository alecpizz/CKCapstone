/******************************************************************
 *    Author: Alec Pizziferro
 *    Contributors:  nullptr
 *    Date Created: 3/5/2025
 *    Description: A class that holds a few specific lighting
 *    parameters.
 *******************************************************************/
using System;
using UnityEngine;



[Serializable]
public class LightingData
{
    public float AmbientIntensity = 0.5f;
    public float GodRayAlpha = 1.0f;
    [Range(1, 5)]
    public int MinimumGridZ = 3;
    public int CloudSpacing = 2;
    public float HueMultiplier = 1.0f;
    public float RainParticleMinSize = 0.01f;
    public float RainParticleMaxSize = 0.02f;
    [Range(0f, 1f)]
    public float RainSimulationSpeed = 0.55f;
    public float RainEmissionRate = 500f;
    [Range(0f, 1f)]
    public float RainAlpha = 0.682353f;
}
