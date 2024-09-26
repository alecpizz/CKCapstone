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
    public static GridBase Instance { get; private set; }
    
    [InfoBox("Assign this to draw the grid gizmos.")] [SerializeField]
    private Grid grid;
    [Header("Grid Parameters")]
    [SerializeField] [MinValue(2)] private int gridSize = 8;
    [SerializeField] private Transform debugCursor;
    [Header("Grid Visuals")]
    [SerializeField] private Material primaryGridMat;
    [SerializeField] private Material secondaryGridMat;
    private Dictionary<Vector3Int, HashSet<GameObject>> _gridEntries = new();
    private Dictionary<GameObject, Vector3Int> _gameObjectToGridMap = new();
    [SerializeField] [HideInInspector] private GameObject gridMeshHolder;

    /// <summary>
    /// Grabs the grid component.
    /// </summary>
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance);
            Instance = this;
        }
        grid = GetComponent<Grid>();
        GenerateMesh();
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

        if (debugCursor == null) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(GetCellPositionInDirection(debugCursor.position, Vector3.forward), grid.cellSize);
        Gizmos.DrawWireCube(GetCellPositionInDirection(debugCursor.position, Vector3.back), grid.cellSize);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(GetCellPositionInDirection(debugCursor.position, Vector3.right), grid.cellSize);
        Gizmos.DrawWireCube(GetCellPositionInDirection(debugCursor.position, Vector3.left), grid.cellSize);
        Gizmos.color = Color.red;
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
    /// Checks if the cell contains any objects.
    /// </summary>
    /// <param name="cellID">The ID of the cell.</param>
    /// <returns>True if the cell is empty.</returns>
    public bool CellIsEmpty(Vector3Int cellID)
    {
        var set = GetCellEntries(cellID);
        if (set == null)
        {
            return false;
        }
        return set.Count == 0;
    }

    /// <summary>
    /// Checks if the cell contains any objects.
    /// </summary>
    /// <param name="position">The world position to find a cell.</param>
    /// <returns>True if the cell is empty.</returns>
    public bool CellIsEmpty(Vector3 position)
    {
        var set = GetCellEntries(position);
        if (set == null)
        {
            return false;
        }
        return set.Count == 0;
    }

    /// <summary>
    /// Converts a world space position to a cell index.
    /// If outside the bounds of the grid, it will be clamped.
    /// </summary>
    /// <param name="worldSpacePos">The position to convert.</param>
    /// <returns>A cell index for the grid.</returns>
    public Vector3Int WorldToCell(Vector3 worldSpacePos)
    {
        worldSpacePos.y = transform.position.y;
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
        Vector3Int pos = cellPos;
        pos.x = Mathf.Clamp(cellPos.x, 0, gridSize - 1);
        pos.y = Mathf.Clamp(cellPos.y, 0, gridSize - 1);
        pos.z = Mathf.Clamp(cellPos.z, 0, gridSize - 1);
        return grid.GetCellCenterWorld(pos);
    }

    /// <summary>
    /// Gets the world space cell position for a cell in the desired direction,
    /// relative to the original position.
    /// </summary>
    /// <param name="position">The position of the object.</param>
    /// <param name="dir">The direction the object will move in. This will scale based on the grid.
    /// This must be in a cardinal direction.</param>
    /// <returns></returns>
    public Vector3 GetCellPositionInDirection(Vector3 position, Vector3 dir)
    {
        return CellToWorld(WorldToCell(position) + Vector3ToInt(dir.normalized));
    }

    /// <summary>
    /// Flattens a Vec3 into a Vec3int.
    /// </summary>
    /// <param name="v">A vector3.</param>
    /// <returns>A flattened Vector3Int.</returns>
    private static Vector3Int Vector3ToInt(Vector3 v)
    {
        return new Vector3Int(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y), Mathf.FloorToInt(v.z));
    }

    /// <summary>
    /// Generates a grid by spawning a bunch of planes respective to the cells.
    /// </summary>
    [Button]
    private void GenerateMesh()
    {
        if (grid == null)
        {
            return;
        }

        if (gridMeshHolder != null)
        {
            if (Application.isPlaying)
            {
               Destroy(gridMeshHolder);
            }
            else
            {
                DestroyImmediate(gridMeshHolder);
            }
        }
        
        gridMeshHolder = new GameObject("Grid Mesh Holder")
        {
            transform =
            {
                parent = transform
            }
        };
        gridMeshHolder.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                var pos = CellToWorld(new Vector3Int(i, 0, j));
                var plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
                plane.transform.position = pos;
                plane.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                var scale = plane.transform.localScale;
                scale.x = (grid.cellSize.x);
                scale.y = (grid.cellSize.z);
                plane.transform.localScale = scale;
                plane.transform.SetParent(gridMeshHolder.transform, true);
                plane.GetComponent<MeshRenderer>().material = i % 2 == 1 ^ j % 2 == 0 ? primaryGridMat : secondaryGridMat;
            }
        }
    }
}