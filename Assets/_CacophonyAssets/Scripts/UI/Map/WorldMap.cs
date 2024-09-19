using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class KeyElement
{
    public string KeyElementName;
    public Color KeyElementColor;
}

/// <summary>
/// Author: Trinity
/// Coconspirator: Ryan
/// Description: Controls the UI map dynamically
/// </summary>
public class WorldMap : MonoBehaviour
{
    [Header("Variables")]
    public int _columns;
    public int _rows;
    public float _scale = 1;
    [SerializeField] int minimapYOffset;
    //public float distanceBetweenRooms = 125;
    public Vector2Int gridCenter;
    [Space]

    [Header("References")]
    [SerializeField] GameObject tilePrefab;
    [SerializeField] RectTransform anchorPanel;
    [SerializeField] MapScreenUI _ui;
    [Space]

    [SerializeField] private KeyElement _newRoom;
    [SerializeField] private KeyElement _completeRoom;
    [SerializeField] private KeyElement _playerRoom;
    [SerializeField] private KeyElement _door;

    private MapTile[,] _gridArray;

    private Vector2 anchorOffset;

    private void Start()
    {
        PrepareKey();

        anchorOffset = anchorPanel.anchoredPosition;

        if (SaveSceneData.Instance != null)
            AssignRooms(SaveSceneData.Instance.GetScenes());
        else
            CreateGrid();
    }

    private void PrepareKey()
    {
        _ui.SpawnKeyElement(_newRoom);
        _ui.SpawnKeyElement(_completeRoom);
        _ui.SpawnKeyElement(_playerRoom);
        _ui.SpawnKeyElement(_door);
    }

    /// <summary>
    /// Creates an empty grid
    /// </summary>
    void CreateGrid()
    {
        _gridArray = new MapTile[_columns, _rows];
        //Debug.Log(smartOffset);
        for (int xGrid = 0; xGrid < _columns; xGrid++)
        {
            for (int yGrid = 0; yGrid < _rows; yGrid++)
            {
                InstantiateGridTile(xGrid, yGrid);
            }
        }
    }

    /// <summary>
    /// Creates the map based on scenes from SaveSceneData
    /// </summary>
    /// <param name="scenes"></param>
    public void AssignRooms(List<GameplayScenes> scenes)
    {
        _gridArray = new MapTile[_columns, _rows];

        SaveSceneData.Instance.GetSceneDataOfCurrentScene().hasVisited = true;

        foreach (GameplayScenes scene in scenes)
        {
            InstantiateGridTile(scene);
        }
    }

    public MapTile InstantiateGridTile(int x, int y)
    {
        GameObject obj = Instantiate(tilePrefab, transform);
        MapTile mapTile = obj.GetComponent<MapTile>();
        mapTile.SetPositionFromCoordinates(x, y, _scale, anchorOffset);

        _gridArray[x, y] = mapTile;

        return mapTile;
    }

    public MapTile InstantiateGridTile(GameplayScenes scene)
    {
        if (!scene.hasVisited)
            return null;

        GameObject obj = Instantiate(tilePrefab, transform);
        MapTile mapTile = obj.GetComponent<MapTile>();
        mapTile.SetScene(scene);

        if (SaveSceneData.Instance.GetSceneDataOfCurrentScene().sceneID == scene.sceneID)
            mapTile.SetColor(_playerRoom.KeyElementColor);
        else if (scene.completed)
            mapTile.SetColor(_completeRoom.KeyElementColor);
        else
            mapTile.SetColor(_newRoom.KeyElementColor);

        int x = scene.positionOnSceneGrid.x + gridCenter.x;
        int y = scene.positionOnSceneGrid.y + gridCenter.y;

        //mapTile.SetPositionFromCoordinates(x, y, _scale, anchorOffset);
        //mapTile.SetPositionFromCoordinates(scene.positionOnSceneGrid.x, y, _scale, anchorOffset);
        mapTile.SetPositionFromCoordinates(scene.positionOnSceneGrid.x, scene.positionOnSceneGrid.y + minimapYOffset, _scale, anchorOffset);

        if (OutOfBoundsCheck(x, y))
        {
            //Debug.Log(gameObject.name);
            //Debug.LogError("MapTile coordinates out of bounds: x = " + x + " | y = " + y);
            return null;
        }
            
        _gridArray[x, y] = mapTile;

        return mapTile;
    }

    private bool OutOfBoundsCheck(int x, int y)
    {
        return x >= _columns || y >= _rows || x < 0 || y < 0;
    }
}
