using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Author: Ryan
/// Editor(s): Trinity
/// Description: Data of each gridspace
/// </summary>
public class GridStats : MonoBehaviour
{
    [SerializeField] int _xPos = 0;
    [SerializeField] int _yPos = 0;
    [SerializeField]
    private GridAvailability _gridAvailability = GridAvailability.Empty;
    [SerializeField]
    bool doTrackAvailability = false;

    public GameObject _objectOnTile;

    private Renderer tileRenderer;

    private static readonly List<Vector2> Directions = new()
    {
        new Vector2(0, 1),
        new Vector2(-1, 0),
        new Vector2(0, -1),
        new Vector2(1, 0)
    };

    public List<GridStats> Neighbors { get; protected set; }
    public GridStats Connection { get; private set; }
    public float G { get; private set; }
    public float H { get; private set; }
    public float F => G + H;

    private Material _previousMat;
    private UnityEvent selectedTileChangedEvent;

    private void Start()
    {
        tileRenderer = GetComponentInChildren<Renderer>();
        tileRenderer.material = GameplayManagers.Instance.Grid.GetBaseTileMaterial();

        StoreNeighbors();

        FindObjectAbove();

        /*RaycastHit playerHit = RaycastUp(GameplayManagers.Instance.Grid.GetPlayerLayerMask());
        if (playerHit.collider != null && playerHit.collider.gameObject.CompareTag("Player"))
        {
            //Debug.Log("HitPlayerWithRaycast");
            StartingPositionHighlight();
        }*/
            
    }

    /// <summary>
    /// Checks if there is an object above this
    /// </summary>
    public void FindObjectAbove(bool setAvailability = true)
    {
        RaycastHit rayHit;
        /*if (Physics.BoxCast(transform.position, new Vector3(5, 5, 5), Vector3.up, out rayHit,
            Quaternion.identity, 5, GameplayManagers.Instance.Grid.GetObjectsOnTilesLayerMask()))
        {
            Debug.Log("FoundObjectAbove");
            SetObjectOnTile(rayHit.collider.gameObject);
        }*/
        if (Physics.Raycast(transform.position, Vector3.up, out rayHit, 5, GameplayManagers.Instance.Grid.GetObjectsOnTilesLayerMask()))
        {
            //Debug.Log("FoundObjectAbove");
            SetObjectOnTile(rayHit.collider.gameObject);
            if (setAvailability)
            {
                if (!rayHit.collider.CompareTag("Enemy"))
                    SetGridAvailability(GridAvailability.Occupied);
            }
        }
        else
            if(setAvailability)
                SetGridAvailability(GridAvailability.Empty);
    }

    private RaycastHit RaycastUp(LayerMask layerCheck)
    {
        RaycastHit tempRayHit;
        Physics.Raycast(transform.position, Vector3.up, out tempRayHit, 5,layerCheck);
        return tempRayHit;
    }

    /// <summary>
    /// Stores the neighboring tiles for reference later
    /// </summary>
    public void StoreNeighbors()
    {
        Neighbors = new List<GridStats>();

        foreach (var tile in Directions.Select(dir => GameplayManagers.Instance.Grid.FindGridInDirection(dir, this)).Where(tile => tile != null))
        {
            Neighbors.Add(tile);
        }
    }

    /// <summary>
    /// Calculates the indirect distance between self and another tile. Not a straight line to the target, instead is the assumed path length to reach it
    /// </summary>
    /// <param name="other">Other tile</param>
    /// <returns>Indirect distance</returns>
    public float GetDistance(GridStats other)
    {
        var dist = GetDirectionTo(other);

        var lowest = Mathf.Min(dist.x, dist.y);
        var highest = Mathf.Max(dist.x, dist.y);

        var horizontalMovesRequired = highest - lowest;

        return lowest * 14 + horizontalMovesRequired * 10;
    }

    /// <summary>
    /// Calculates the direct distance between self and another tile
    /// </summary>
    /// <param name="other">Other tile</param>
    /// <returns>Direct distance</returns>
    public Vector2 GetDirectionTo(GridStats other)
    {
        return new Vector2Int(-(GetXPos() - other.GetXPos()), -(GetYPos() - other.GetYPos()));
    }
    
    // For testing purposes
    public void Highlight()
    {
        tileRenderer.material = GameplayManagers.Instance.Grid.GetHighlightedTileMaterial();
    }

    /// <summary>
    /// Highlights the tile, and subsequently listens for when the player rotates again to un-highlight
    /// </summary>
    /// <param name="selectedTileChangedEvent">Event from the player that is triggered when they rotate / move</param>
    public void Highlight(UnityEvent selectedTileChangedEvent)
    {
        //if (GameplayManagers.Instance == null) Debug.Log("Failed Highlight");
        _previousMat = tileRenderer.material;
        tileRenderer.material = GameplayManagers.Instance.Grid.GetHighlightedTileMaterial();
        /*if (GameplayManagers.Instance.Grid.GetHighlightedTileMaterial() == null || tileRenderer.material == null)
        {
            Debug.Log("1" + GameplayManagers.Instance.Grid.GetHighlightedTileMaterial());
            Debug.Log("2" + tileRenderer.material);
        }*/
        
        selectedTileChangedEvent.AddListener(ResetMaterial);
        this.selectedTileChangedEvent = selectedTileChangedEvent;
    }

    public void StartingPositionHighlight()
    {
        //Debug.Log("StartingTileChanged");
        tileRenderer.material = GameplayManagers.Instance.Grid.GetStartingTileMaterial();
    }

    /// <summary>
    /// Turns off highlight, reverting back to base material. Then stops listening to the event
    /// </summary>
    private void ResetMaterial()
    {
        //tileRenderer.material = GameplayManagers.Instance.Grid.GetBaseTileMaterial();
        tileRenderer.material = _previousMat;
        selectedTileChangedEvent.RemoveListener(ResetMaterial);
        selectedTileChangedEvent = null;
    }

    // Setters and Getters
    public void SetConnection(GridStats tile)
    {
        Connection = tile;
    }

    public void SetG(float g)
    {
        G = g;
    }

    public void SetH(float h)
    {
        H = h;
    }

    public void SetXPos(int newX)
    {
        _xPos = newX;
    }
    public void SetYPos(int newY)
    {
        _yPos = newY;
    }
    public void SetGridAvailability(GridAvailability newGridA)
    {
        if (doTrackAvailability)
            Debug.Log("changed!");

        _gridAvailability = newGridA;
    }
    public void SetObjectOnTile(GameObject obj)
    {
        if (doTrackAvailability)
            Debug.Log("Object change!");

        _objectOnTile = obj;
    }

    public int GetXPos()
    {
        return (_xPos);
    }
    public int GetYPos()
    {
        return (_yPos);
    }
    public GridAvailability GetGridAvailability()
    {
        return _gridAvailability;
    }
    public GameObject GetObjectOnTile()
    {
        FindObjectAbove(false);
        return _objectOnTile;
    }

    private void OnDestroy()
    {
        selectedTileChangedEvent = null;
    }
}

public enum GridAvailability
{
    Occupied,
    Claimed,
    Empty
};