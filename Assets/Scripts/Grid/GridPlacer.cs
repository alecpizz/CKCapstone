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
using System.Runtime.InteropServices.WindowsRuntime;
using SaintsField;
using SaintsField.Playa;
using UnityEngine;

/// <summary>
/// Placement tool for grid objects. To use, simply attach to a gameobject that you wish to position on the grid.
/// </summary>
public class GridPlacer : MonoBehaviour, IGridEntry
{
    public bool IsTransparent { get => _isTransparent; set => _isTransparent = value; }
    public bool BlocksHarmonyBeam { get => _blocksHarmonyBeam; set => _blocksHarmonyBeam = value; }
    public Vector3 Position { get => transform.position; }
    public GameObject GetGameObject { get => gameObject; }

    [InfoBox("Use this component to add this gameObject to a grid.")]
    [LayoutStart("Settings", ELayout.Background | ELayout.TitleBox)]
    [SerializeField] 
    private bool _isTransparent = false;

    [SerializeField]
    private bool _blocksHarmonyBeam = true;

    [Space]
    [SerializeField] 
    private bool _snapToGrid = true;

    [SerializeField]
    private Vector3 offset = Vector3.zero;

    [Space]
    [SerializeField] 
    private bool _disableGridCell = false;

    [Space]
    [SerializeField] 
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

    /// <summary>
    /// Places this object in the center of its grid cell
    /// </summary>
    public void SnapToGridSpace()
    {
        Vector3Int cellPos = GridBase.Instance.WorldToCell(transform.position);
        transform.position = GridBase.Instance.CellToWorld(cellPos);
    }
}
