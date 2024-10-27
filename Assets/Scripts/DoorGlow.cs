/******************************************************************
*    Author: Taylor Sims
*    Contributors: 
*    Date Created: 10/22/24
*    Description: 
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorGlow : MonoBehaviour
{
    [SerializeField] private Material doorMaterial; // Assign your material in the Inspector
    [SerializeField] private Color glowColor = Color.yellow; // Set the glow color
    [SerializeField] private float glowIntensity = 15.0f; // Intensity of the glow
    
    // Combined method to trigger door unlocking and visuals
    public void GlowAndUnlockDoor()
    {
        // Enable the emission and set the color to make the door glow
        doorMaterial.EnableKeyword("_EMISSION");
        doorMaterial.SetColor("_EmissionColor", glowColor * glowIntensity);
    }
}
