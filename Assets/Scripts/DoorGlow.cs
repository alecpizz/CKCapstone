/******************************************************************
*    Author: Taylor Sims
*    Contributors: Trinity
*    Date Created: 10/22/24
*    Description: Creates a glow around the door.
*    Changes the door visuals once the door is unlocked and opened.
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class DoorGlow : MonoBehaviour
{
    [SerializeField] [FormerlySerializedAs("glowColor")] private Color _glowColor = Color.yellow; // Set the glow color
    [SerializeField] [FormerlySerializedAs("glowIntensity")] private float _glowIntensity = 15.0f; // Intensity of the glow

    private Renderer _renderer; // The renderer attached to this object

    /// <summary>
    /// Renders the glow on the door once the function wakes
    /// </summary>
    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    /// <summary>
    /// Combined method to trigger door unlocking and visuals
    /// </summary>
    public void GlowAndUnlockDoor()
    {
        if (_renderer == null)
        {
            return;
        }

        // Enable the emission and set the color to make the door glow
        _renderer.material.EnableKeyword("_EMISSION");
        _renderer.material.SetColor("_EmissionColor", _glowColor * _glowIntensity);
    }
}
