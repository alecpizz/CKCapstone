/******************************************************************
*    Author: Taylor Sims
*    Contributors: Trinity
*    Date Created: 10/22/24
*    Description: 
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorGlow : MonoBehaviour
{
    [SerializeField] private Color glowColor = Color.yellow; // Set the glow color
    [SerializeField] private float glowIntensity = 15.0f; // Intensity of the glow

    private Renderer _renderer; // The renderer attached to this object

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    // Combined method to trigger door unlocking and visuals
    public void GlowAndUnlockDoor()
    {
        if (_renderer == null)
        {
            return;
        }

        // Enable the emission and set the color to make the door glow
        _renderer.material.EnableKeyword("_EMISSION");
        _renderer.material.SetColor("_EmissionColor", glowColor * glowIntensity);
    }
}
