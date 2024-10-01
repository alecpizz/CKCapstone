/******************************************************************
 *    Author: Alec Pizziferro
 *    Contributors: N/A
 *    Date Created: 9/29/2024
 *    Description: Placement tool for grid objects.
 *    To use, simply attach to a gameobject that you wish to position on the grid.
 *******************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// Placement tool for grid objects. To use, simply attach to a gameobject that you wish to position on the grid.
/// </summary>
[DefaultExecutionOrder(-10000)]
public class GridPlacer : MonoBehaviour
{
    [InfoBox("Use this component to add this gameObject to a grid.")]
    [SerializeField] 
    private int _dummyField;
    /// <summary>
    /// Places the object on the grid and updates its position.
    /// </summary>
    private void Start()
    {
        transform.position = GridBase.Instance.CellToWorld(GridBase.Instance.WorldToCell(transform.position));
        GridBase.Instance.AddEntry(this.gameObject);
    }

    /// <summary>
    /// Updates the position of the object on the grid.
    /// </summary>
    private void OnDisable()
    {
        UpdatePosition();
    }

    /// <summary>
    /// External accessor to update the position of the object on the grid.
    /// </summary>
    public void UpdatePosition()
    {
        transform.position = GridBase.Instance.CellToWorld(GridBase.Instance.WorldToCell(transform.position));
        GridBase.Instance.UpdateEntry(this.gameObject);
    }

    /// <summary>
    /// Removes the object on the grid.
    /// </summary>
    private void OnDestroy()
    {
        GridBase.Instance.RemoveEntry(gameObject);
    }
}
