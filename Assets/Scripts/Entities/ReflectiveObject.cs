/******************************************************************
*    Author: Claire Noto
*    Contributors: Claire Noto
*    Date Created: 10/12/24
*    Description: Script that handles harmony beam reflections
*******************************************************************/


using UnityEngine;

public class ReflectiveObject : MonoBehaviour, IHarmonyBeamEntity
{
    private HarmonyBeam _harmonyBeam;
    private Vector3 _fwdDir;

    private void Start()
    {
        _fwdDir = transform.forward;
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
    /// Toggles the direction of the reflection
    /// </summary>
    /// <param name="left">true for left, false for right</param>
    public void FlipDirection(bool flip = true)
    {
        if (flip)
        {
            transform.forward = -_fwdDir;
        }
        else
        {
            transform.forward = _fwdDir;
        }
    }

    public bool AllowLaserPassThrough { get => true; }
    public void OnLaserHit(RaycastHit hit)
    {
        _harmonyBeam.ToggleBeam(true);
        _harmonyBeam.DetectObjects();
    }

    public void OnLaserExit()
    {
        _harmonyBeam.ToggleBeam(false);
        _harmonyBeam.DetectObjects();
    }

    public bool HitWrapAround { get => false; }
    public Vector3 Position { get => transform.position; }
}