/******************************************************************
*    Author: Alec Pizziferro
*    Contributors: nullptr
*    Date Created: 10/31/2024
*    Description: Interface for entities that have special
 *  behavior when interacting with the harmony beam.
*******************************************************************/
using UnityEngine;

/// <summary>
/// Interface for entities that have special behavior when
/// interacting with the harmony beam.
/// </summary>
public interface IHarmonyBeamEntity
{
    /// <summary>
    /// Whether lasers can pass through this object.
    /// </summary>
    public bool AllowLaserPassThrough { get; }

    /// <summary>
    /// When a laser hit this object.
    /// </summary> 
    public void OnLaserHit();

    /// <summary>
    /// When a laser left this object after
    /// previously hitting it.
    /// </summary>
    public void OnLaserExit();

    /// <summary>
    /// Whether the laser should spawn vfx to wrap around this object.
    /// </summary>
    public bool HitWrapAround { get; }

    /// <summary>
    /// The position of the object.
    /// </summary>
    Vector3 Position { get; }
}