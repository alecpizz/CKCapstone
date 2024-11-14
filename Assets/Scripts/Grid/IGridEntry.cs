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
    /// The position of the object, in world space.
    /// </summary>
    Vector3 Position { get; }

    GameObject GetGameObject { get; }
}
