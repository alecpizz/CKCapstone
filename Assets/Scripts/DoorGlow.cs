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
    [SerializeField] private float glowIntensity = 5.0f; // Intensity of the glow
    private EndLevelDoor endLevelDoor; // Reference to EndLevelDoor script

    private void Start()
    {
        // Get the EndLevelDoor component from the same GameObject or assign it manually
        endLevelDoor = GetComponent<EndLevelDoor>();

        // Register to the same event EndLevelDoor uses if needed
        WinChecker.GotCorrectSequence += GlowAndUnlockDoor;
    }

    private void OnDisable()
    {
        // Unregister the event to prevent errors
        WinChecker.GotCorrectSequence -= GlowAndUnlockDoor;
    }

    // Combined method to trigger door unlocking and visuals
    private void GlowAndUnlockDoor()
    {
        if (endLevelDoor != null)
        {
            // Call the UnlockDoor method from EndLevelDoor
            endLevelDoor.UnlockDoor();
        }

        // Enable the emission and set the color to make the door glow
        doorMaterial.EnableKeyword("_EMISSION");
        doorMaterial.SetColor("_EmissionColor", glowColor * glowIntensity);
    }
}
