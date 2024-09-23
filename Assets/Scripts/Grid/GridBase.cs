using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Grid))]
public class GridBase : MonoBehaviour
{
    [SerializeField] [MinValue(2)] private int gridSize = 8;
    [SerializeField] private Transform cursor;
    [SerializeField] private Grid grid;
    //TODO: type based dictionaries rather than GameObjects, keeping this since I dunno what the types will be.
    private Dictionary<Vector3Int, HashSet<GameObject>> _gridEntries = new();
    private Dictionary<GameObject, Vector3Int> _gameObjectToGridMap = new();
    private void OnDrawGizmos()
    {
        if (grid == null) return;
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                Gizmos.DrawWireCube(grid.GetCellCenterWorld(new Vector3Int(i, 0, j)), grid.cellSize);
            }
        }

        Gizmos.color = Color.red;
        if (cursor == null) return;
        Gizmos.DrawWireCube(grid.GetCellCenterWorld(WorldToCell(cursor.position)), grid.cellSize);
    }

    public void AddEntry(GameObject obj)
    {
        if (_gameObjectToGridMap.ContainsKey(obj)) return;
        _gameObjectToGridMap.Add(obj, WorldToCell(obj.transform.position));
        var set = GetCellEntries(obj.transform.position);
        set.Add(obj);
    }

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

    public HashSet<GameObject> GetCellEntries(Vector3Int coordinate)
    {
        if (_gridEntries.TryGetValue(coordinate, out var entries))
        {
            return entries;
        }
        _gridEntries.Add(coordinate, new HashSet<GameObject>());
        return _gridEntries[coordinate];
    }

    public HashSet<GameObject> GetCellEntries(Vector3 worldSpacePos)
    {
        return GetCellEntries(WorldToCell(worldSpacePos));
    }

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

    public Vector3 CellToWorld(Vector3Int cellPos)
    {
        return grid.GetCellCenterWorld(cellPos);
    }
}