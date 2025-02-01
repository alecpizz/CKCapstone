/******************************************************************
*    Author: Claire Noto
*    Contributors: Claire Noto
*    Date Created: 10/12/24
*    Description: Script that handles harmony beam reflections
*******************************************************************/
using UnityEngine;

public class ReflectiveObject : MonoBehaviour, IHarmonyBeamEntity
{
    public bool AllowLaserPassThrough => true;
    public bool HitWrapAround => false;
    public Vector3 Position => transform.position;
    private HarmonyBeam _harmonyBeam;

    /// <summary>
    /// Sets up references to harmony beam attached to this object
    /// </summary>
    private void Start()
    {
        _harmonyBeam = GetComponent<HarmonyBeam>();
        _harmonyBeam.ToggleBeam(false);
    }

    /// <summary>
    /// Returns the new direction for the beam after reflecting
    /// </summary>
    /// <param name="incomingDirection">direction the original laser is coming from</param>
    public Vector3 GetReflectionDirection()
    {
        return transform.forward;
    }

    /// <summary>
    /// Enables the reflective HarmonyBeam instance.
    /// </summary>
    public void ToggleBeam(bool toggle)
    {
        _harmonyBeam.ToggleBeam(toggle);
    }

    /// <summary>
    /// When this object is hit by a laser, turn on the beam, and check for objects.
    /// </summary>
    public void OnLaserHit()
    {
        _harmonyBeam.ToggleBeam(true);
    }

    /// <summary>
    /// When this object is non longer being hit by a laser, turn off the beam
    /// and check for any exited objects.
    /// </summary>
    public void OnLaserExit()
    {
        _harmonyBeam.ToggleBeam(false);
    }
}