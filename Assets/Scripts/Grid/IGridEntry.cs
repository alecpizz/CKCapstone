/******************************************************************
 *    Author: Alec Pizziferro
 *    Contributors: N/A
 *    Date Created: 10/3/2024
 *    Description: Interface for objects that exist on a grid.
 *******************************************************************/
using UnityEngine;

/// <summary>
/// Interface for objects that exist on a grid.
/// </summary>
public interface IGridEntry
{
    /// <summary>
    /// Whether or not the object can be passed through on the grid.
    /// </summary>
    bool IsTransparent { get; }

    /// <summary>
    /// Whether or not the object stops a harmony beam
    /// </summary>
    bool BlocksHarmonyBeam { get; }

    /// <summary>
    /// Whether or not the object stops a moving wall
    /// </summary>
    bool BlocksMovingWall { get; }

    /// <summary>
    /// The position of the object, in world space.
    /// </summary>
    Vector3 Position { get; }

    /// <summary>
    /// The gameObject this entry is attached to.
    /// </summary>
    GameObject EntryObject { get; }

    /// <summary>
    /// Used to position this object in the center of its grid cell
    /// </summary>
    void SnapToGridSpace();
}
