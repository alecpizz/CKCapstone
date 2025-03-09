/******************************************************************
 *    Author: Alec Pizziferro
 *    Contributors: Nick Grinstead
 *    Date Created: 9/20/2024
 *    Description: Class for interfacing with a grid, stores information about
 *   what entities are in a cell, as well as provides methods for converting world
 *   space information to grid space and vice versa.
 *******************************************************************/

using System;
using System.Collections.Generic;
using SaintsField;
using SaintsField.Playa;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Class for interfacing with a grid. Stores information about
/// what entities are in a cell. Also contains space conversions within it.
/// </summary>
[DefaultExecutionOrder(-10000)]
[RequireComponent(typeof(Grid))]
public class GridBase : MonoBehaviour
{
    public static GridBase Instance { get; private set; }

    [InfoBox("Assign this to draw the grid gizmos.")] [SerializeField]
    private Grid _grid;

    [LayoutStart("Grid Parameters", ELayout.Background | ELayout.TitleBox)] [SerializeField] [MinValue(2)]
    private int _gridSize = 8;

    [SerializeField]
    private Material _primaryGridMat;

    [SerializeField]
    private Material _secondaryGridMat;

    [LayoutStart("Grid Viuals", ELayout.Background | ELayout.TitleBox)] [SerializeField]
    private GameObject _gridPrefab;

   [SerializeField] [OnValueChanged(nameof(OnDrawMeshChanged))]
    private bool _drawGridMesh = true;

    private Dictionary<Vector3Int, HashSet<IGridEntry>> _gridEntries = new();
    private Dictionary<IGridEntry, Vector3Int> _gameObjectToGridMap = new();
    [SerializeField] [HideInInspector] private GameObject _gridMeshHolder;
    private Dictionary<Vector3Int, GameObject> _gridObjects = new();

    private const string GridMeshName = "Grid Mesh Holder";

    public int Size => _gridSize;

    /// <summary>
    /// Grabs the grid component.
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance);
            Instance = this;
        }
        else
        {
            Instance = this;
        }

        _grid = GetComponent<Grid>();
        GenerateMesh();
    }

    
    private void OnValidate()
    {
        Instance = this;
    }

    /// <summary>
    /// Debug drawer for the grid.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (_grid == null) return;
        //draw cells at points 0-grid size.
        for (int i = 0; i < _gridSize; i++)
        {
            for (int j = 0; j < _gridSize; j++)
            {
                Gizmos.DrawWireCube(_grid.GetCellCenterWorld(new Vector3Int(i, 0, j)), _grid.cellSize);
            }
        }
    }

    /// <summary>
    /// Adds a gameObject to the grid cell. Will not add if the grid already contains it.
    /// </summary>
    /// <param name="obj">The gameObject to add.</param>
    public void AddEntry(IGridEntry obj)
    {
        if (_gameObjectToGridMap.ContainsKey(obj)) return;
        Vector3Int cell = WorldToCell(obj.Position);
        _gameObjectToGridMap.Add(obj, cell);
        var set = GetCellEntries(cell);
        set.Add(obj);
    }

    /// <summary>
    /// Removes a gameObject from the grid cell.
    /// </summary>
    /// <param name="obj">The object to remove.</param>
    public void RemoveEntry(IGridEntry obj)
    {
        if (!_gameObjectToGridMap.ContainsKey(obj)) return;
        Vector3Int cell = WorldToCell(obj.Position);
        _gameObjectToGridMap.Remove(obj);
        _gridEntries[cell].Remove(obj);
    }

    /// <summary>
    /// Updates an entries status in the grid. If the entry isn't in the grid, it will be added.
    /// If there is no position change it will exit early.
    /// </summary>
    /// <param name="obj"></param>
    public void UpdateEntry(IGridEntry obj)
    {
        UpdateEntryAtPosition(obj, obj.Position);
    }

    /// <summary>
    ///  Updates an entries status in the grid to be at a specified position. 
    ///  If the entry isn't in the grid, it will be added.
    /// If there is no position change it will exit early.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="position"></param>
    public void UpdateEntryAtPosition(IGridEntry obj, Vector3 objPosition)
    {
        if (!_gameObjectToGridMap.TryGetValue(obj, out var prevPos))
        {
            AddEntry(obj);
            return;
        }

        Vector3Int newPos = WorldToCell(objPosition);
        if (prevPos == newPos) return;
        _gameObjectToGridMap[obj] = newPos;
        _gridEntries[prevPos].Remove(obj);
        if (!_gridEntries.ContainsKey(newPos))
        {
            _gridEntries.Add(newPos, new HashSet<IGridEntry>());
        }

        _gridEntries[newPos].Add(obj);
    }

    /// <summary>
    /// Gets the entries in a cell at a specific coordinate.
    /// </summary>
    /// <param name="coordinate">The cell index.</param>
    /// <returns>A hashset of entries within the cell.</returns>
    public HashSet<IGridEntry> GetCellEntries(Vector3Int coordinate)
    {
        if (_gridEntries.TryGetValue(coordinate, out var entries))
        {
            return entries;
        }

        _gridEntries.Add(coordinate, new HashSet<IGridEntry>());
        return _gridEntries[coordinate];
    }

    /// <summary>
    /// Gets the entries in the nearest cell relative to the world space coordinate.
    /// </summary>
    /// <param name="worldSpacePos">The position, in world space, to compare against.</param>
    /// <returns>A hashset of entries within the cell.</returns>
    public HashSet<IGridEntry> GetCellEntries(Vector3 worldSpacePos)
    {
        return GetCellEntries(WorldToCell(worldSpacePos));
    }


    /// <summary>
    /// Checks if the cell contains all transparent entries.
    /// </summary>
    /// <param name="cellId">The id of the cell being searched.</param>
    /// <returns>True if all the elements in the cell are transparent.</returns>
    public bool CellIsTransparent(Vector3Int cellId)
    {
        var set = GetCellEntries(cellId);
        if (set == null)
        {
            return false;
        }

        foreach (var entry in set)
        {
            if (!entry.IsTransparent)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks if the cell contains all transparent entries.
    /// </summary>
    /// <param name="position">The world position to find a cell.</param>
    /// <returns>True if all the elements in the cell are transparent.</returns>
    public bool CellIsTransparent(Vector3 position)
    {
        return CellIsTransparent(WorldToCell(position));
    }

    /// <summary>
    /// Checks if the cell contains any objects.
    /// </summary>
    /// <param name="cellId">The ID of the cell.</param>
    /// <returns>True if the cell is empty.</returns>
    public bool CellIsEmpty(Vector3Int cellId)
    {
        var set = GetCellEntries(cellId);
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
        return CellIsEmpty(WorldToCell(position));
    }

    /// <summary>
    /// Converts a world space position to a cell index.
    /// If outside the bounds of the grid, it will be clamped.
    /// </summary>
    /// <param name="worldSpacePos">The position to convert.</param>
    /// <returns>A cell index for the grid.</returns>
    public Vector3Int WorldToCell(Vector3 worldSpacePos)
    {
        //convert the world space position to the space of this grid
        worldSpacePos.y = transform.position.y;
        Vector3 local = transform.InverseTransformPoint(worldSpacePos);

        Vector3 cellSize = _grid.cellSize;
        Vector3 cellGap = _grid.cellGap;

        //floor of a coord will give us a whole number coordinate.
        int x = Mathf.Clamp(Mathf.FloorToInt(local.x / (cellSize.x + cellGap.x)), 0, _gridSize - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(local.y / (cellSize.y + cellGap.y)), 0, _gridSize - 1);
        int z = Mathf.Clamp(Mathf.FloorToInt(local.z / (cellSize.z + cellGap.z)), 0, _gridSize - 1);

        return new Vector3Int(x, y, z);
    }

    /// <summary>
    /// Converts a cell position to a world space position.
    /// </summary>
    /// <param name="cellPos">The cell index to view.</param>
    /// <returns>A world space, centered position of a cell.</returns>
    public Vector3 CellToWorld(Vector3Int cellPos)
    {
        //just clamps the position within the grid's boundaries
        Vector3Int pos = cellPos;
        pos.x = Mathf.Clamp(cellPos.x, 0, _gridSize - 1);
        pos.y = Mathf.Clamp(cellPos.y, 0, _gridSize - 1);
        pos.z = Mathf.Clamp(cellPos.z, 0, _gridSize - 1);
        return _grid.GetCellCenterWorld(pos);
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
        return CellToWorld(WorldToCell(position + dir.normalized * _grid.cellSize.x));
    }
    
    /// <summary>
    /// Gets the cell space cell position for a cell in the desired direction, 
    /// relative to the original position.
    /// </summary>
    /// <param name="cell">Cell position</param>
    /// <param name="dir">The direction to check in. This must be a cardinal direction</param>
    /// <returns></returns>
    public Vector3Int GetCellInDirection(Vector3Int cell, Vector3 dir)
    {
        return WorldToCell(CellToWorld(cell + Vector3ToInt(dir.normalized)));
    }

    /// <summary>
    /// Toggles the grid visuals on and off in runtime.
    /// </summary>
    /// <param name="active">Whether or not the grid visual should be active.</param>
    public void ToggleGridVisuals(bool active)
    {
        _drawGridMesh = active;
        OnDrawMeshChanged();
    }
    
    
    /// <summary>
    /// Disables the grid visual for the cell.
    /// </summary>
    /// <param name="key"></param>
    public void DisableCellVisual(Vector3Int key)
    {
        _gridObjects[key].gameObject.SetActive(false);
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
        //if there's no grid object, or we cannot use the grid, exit early.
        if (_grid == null)
        {
            return;
        }

        if (!_drawGridMesh)
        {
            return;
        }

        //clean-up any old grids
        DestroyMesh();

        //make a new holder object to hold all of the cell tiles
        _gridMeshHolder = new GameObject(GridMeshName)
        {
            transform =
            {
                parent = transform
            }
        };
        _gridMeshHolder.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        _gridObjects.Clear();
        
        //for each square in the grid, instantiate the prefab for the tile
        for (int i = 0; i < _gridSize; i++)
        {
            for (int j = 0; j < _gridSize; j++)
            {
                //get its cell position and spawn it there.
                Vector3Int index = new Vector3Int(i, 0, j);
                var pos = CellToWorld(new Vector3Int(i, 0, j));
                var plane = Instantiate(_gridPrefab, _gridMeshHolder.transform, true);
                plane.transform.position = pos;
                var scale = plane.transform.localScale;
                plane.transform.localScale = scale;
                _gridObjects.Add(index, plane);
            }
        }
    }

    /// <summary>
    /// Destroys the grid mesh.
    /// </summary>
    private void DestroyMesh()
    {
        //destroy the grid if we have it, in editor don't wait.
        var gameObj = _gridMeshHolder == null ?
            transform.Find(GridMeshName).gameObject : _gridMeshHolder;
        if (Application.isPlaying)
        {
            Destroy(gameObj);
        }
        else
        {
            DestroyImmediate(gameObj);
        }
    }

    /// <summary>
    /// Editor callback for when the draw grid visual bool changed.
    /// </summary>
    private void OnDrawMeshChanged()
    {
        if (_drawGridMesh)
        {
            GenerateMesh();
        }
        else
        {
            DestroyMesh();
        }
    }
}