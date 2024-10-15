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
public class GridPlacer : MonoBehaviour, IGridEntry
{
    [InfoBox("Use this component to add this gameObject to a grid.")]
    [BoxGroup("Settings")]
    [SerializeField] 
    private bool isTransparent = false;

    [SerializeField] [BoxGroup("Settings")]
    private bool snapToGrid = true;

    [SerializeField] [BoxGroup("Settings")]
    private bool disableGridCell = false;
    /// <summary>
    /// Places the object on the grid and updates its position.
    /// </summary>
    private void Start()
    {
        if (snapToGrid)
        {
            transform.position = GridBase.Instance.CellToWorld(GridBase.Instance.WorldToCell(transform.position));
        }
        GridBase.Instance.AddEntry(this);
        if (disableGridCell)
        {
            GridBase.Instance.DisableCellVisual(GridBase.Instance.WorldToCell(transform.position));
        }
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
        if (GridBase.Instance == null) return;
        if (snapToGrid)
        {
            transform.position = GridBase.Instance.CellToWorld(GridBase.Instance.WorldToCell(transform.position));
        }
        transform.position = GridBase.Instance.CellToWorld(GridBase.Instance.WorldToCell(transform.position));
        GridBase.Instance.UpdateEntry(this);
    }

    /// <summary>
    /// Removes the object on the grid.
    /// </summary>
    private void OnDestroy()
    {
        if (GridBase.Instance == null) return; 
        GridBase.Instance.RemoveEntry(this);
    }

    public bool IsTransparent { get => isTransparent; }
    public Vector3 Position { get => transform.position; }
    public GameObject GetGameObject { get => gameObject; }
}
