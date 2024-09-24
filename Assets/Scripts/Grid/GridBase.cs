/******************************************************************
*    Author: Alec Pizziferro
*    Contributors: N/A
*    Date Created: 9/20/2024
*    Description: Class for interfacing with a grid, stores information about
 *   what entities are in a cell, as well as provides methods for converting world
 *   space information to grid space and vice versa.
*******************************************************************/

using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Class for interfacing with a grid. Stores information about
/// what entities are in a cell. Also contains space conversions within it.
/// </summary>
[RequireComponent(typeof(Grid))]
public class GridBase : MonoBehaviour
{
    [SerializeField] [MinValue(2)] private int gridSize = 8;
    [SerializeField] private Transform debugCursor;
    [InfoBox("Assign this to draw the grid gizmos.")]
    [SerializeField] private Grid grid;
    //TODO: type based dictionaries rather than GameObjects, keeping this since I dunno what the types will be.
    private Dictionary<Vector3Int, HashSet<GameObject>> _gridEntries = new();
    private Dictionary<GameObject, Vector3Int> _gameObjectToGridMap = new();

    /// <summary>
    /// Grabs the grid component.
    /// </summary>
    private void Awake()
    {
        grid = GetComponent<Grid>();
    }

    /// <summary>
    /// Debug drawer for the grid.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (grid == null) return;
        //draw cells at points 0-grid size.
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                Gizmos.DrawWireCube(grid.GetCellCenterWorld(new Vector3Int(i, 0, j)), grid.cellSize);
            }
        }

        //try and draw a red box based on the cursor position.
        Gizmos.color = Color.red;
        if (debugCursor == null) return;
        Gizmos.DrawWireCube(grid.GetCellCenterWorld(WorldToCell(debugCursor.position)), grid.cellSize);
    }

    /// <summary>
    /// Adds a gameObject to the grid cell. Will not add if the grid already contains it.
    /// </summary>
    /// <param name="obj">The gameObject to add.</param>
    public void AddEntry(GameObject obj)
    {
        if (_gameObjectToGridMap.ContainsKey(obj)) return;
        Vector3Int cell = WorldToCell(obj.transform.position);
        _gameObjectToGridMap.Add(obj, cell);
        var set = GetCellEntries(cell);
        set.Add(obj);
    }

    /// <summary>
    /// Updates an entries status in the grid. If the entry isn't in the grid, it will be added.
    /// If there is no position change it will exit early.
    /// </summary>
    /// <param name="obj"></param>
    public void UpdateEntry(GameObject obj)
    {
        if (!_gameObjectToGridMap.TryGetValue(obj, out var prevPos))
        {
            AddEntry(obj);
            return;
        }

        Vector3Int newPos = WorldToCell(obj.transform.position);
        if (prevPos == newPos) return;
        _gameObjectToGridMap[obj] = newPos;
        _gridEntries[prevPos].Remove(obj);
        if (!_gridEntries.ContainsKey(newPos))
        {
            _gridEntries.Add(newPos, new HashSet<GameObject>());
        }
        _gridEntries[newPos].Add(obj);
    }

    /// <summary>
    /// Gets the entries in a cell at a specific coordinate.
    /// </summary>
    /// <param name="coordinate">The cell index.</param>
    /// <returns>A hashset of entries within the cell.</returns>
    public HashSet<GameObject> GetCellEntries(Vector3Int coordinate)
    {
        if (_gridEntries.TryGetValue(coordinate, out var entries))
        {
            return entries;
        }
        _gridEntries.Add(coordinate, new HashSet<GameObject>());
        return _gridEntries[coordinate];
    }

    /// <summary>
    /// Gets the entries in the nearest cell relative to the world space coordinate.
    /// </summary>
    /// <param name="worldSpacePos">The position, in world space, to compare against.</param>
    /// <returns>A hashset of entries within the cell.</returns>
    public HashSet<GameObject> GetCellEntries(Vector3 worldSpacePos)
    {
        return GetCellEntries(WorldToCell(worldSpacePos));
    }

    /// <summary>
    /// Converts a world space position to a cell index.
    /// If outside the bounds of the grid, it will be clamped.
    /// </summary>
    /// <param name="worldSpacePos">The position to convert.</param>
    /// <returns>A cell index for the grid.</returns>
    public Vector3Int WorldToCell(Vector3 worldSpacePos)
    {
        Vector3 local = transform.InverseTransformPoint(worldSpacePos);

        Vector3 cellSize = grid.cellSize;
        Vector3 cellGap = grid.cellGap;
 
        int x = Mathf.Clamp(Mathf.FloorToInt(local.x / (cellSize.x + cellGap.x)), 0, gridSize - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(local.y / (cellSize.y + cellGap.y)), 0, gridSize - 1);
        int z = Mathf.Clamp(Mathf.FloorToInt(local.z / (cellSize.z + cellGap.z)), 0, gridSize - 1);
        
        return new Vector3Int(x, y, z);
    }

    /// <summary>
    /// Converts a cell position to a world space position.
    /// </summary>
    /// <param name="cellPos">The cell index to view.</param>
    /// <returns>A world space, centered position of a cell.</returns>
    public Vector3 CellToWorld(Vector3Int cellPos)
    {
        return grid.GetCellCenterWorld(cellPos);
    }
}