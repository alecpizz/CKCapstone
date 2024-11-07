/******************************************************************
 *    Author: Alec Pizziferro
 *    Contributors: Trinity Hutson
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
    public bool IsTransparent { get => _isTransparent; }
    public bool BlocksHarmonyBeam { get => _blocksHarmonyBeam; }
    public Vector3 Position { get => transform.position; }
    public GameObject GetGameObject { get => gameObject; }

    [InfoBox("Use this component to add this gameObject to a grid.")]
    [BoxGroup("Settings")]
    [SerializeField] 
    private bool _isTransparent = false;

    [SerializeField]
    private bool _blocksHarmonyBeam = false;

    [Space]
    [SerializeField] [BoxGroup("Settings")]
    private bool _snapToGrid = true;

    [SerializeField]
    [BoxGroup("Settings")]
    private Vector3 offset = Vector3.zero;

    [Space]
    [SerializeField] [BoxGroup("Settings")]
    private bool _disableGridCell = false;

    [Space]
    [SerializeField] [BoxGroup("Settings")]
    private bool _isVisable = true;

    /// <summary>
    /// Places the object on the grid and updates its position. Also disables the mesh rendere on transparent grid objects
    /// </summary>
    private void Start()
    {
        if (_snapToGrid)
        {
            transform.position = GridBase.Instance.CellToWorld(GridBase.Instance.WorldToCell(transform.position)) + offset;
        }
        GridBase.Instance.AddEntry(this);
        if (_disableGridCell)
        {
            GridBase.Instance.DisableCellVisual(GridBase.Instance.WorldToCell(transform.position));
        }
        if (!_isVisable && gameObject.TryGetComponent(out Renderer renderer))
        {
            renderer.enabled = false;
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
        if (_snapToGrid)
        {
            transform.position = GridBase.Instance.CellToWorld(GridBase.Instance.WorldToCell(transform.position)) + offset;
        }
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
}
