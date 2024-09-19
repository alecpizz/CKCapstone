using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Author: Trinity
/// Description: Controls each enemy's movement
/// </summary>
public class EnemyBehavior : MonoBehaviour, IHarmonizable
{
    [Header("AI")]
    [SerializeField]
    float timeToMove = 1;

    [SerializeField]
    float timeToRotate = 0.1f;

    [Tooltip("The amount of added cost for a tile if it is already claimed")]
    [SerializeField]
    float claimedCost = 1;

    [SerializeField]
    float prioritizedBonus = 1;
    internal List<GridStats> path { get; private set; } = new();
    [Space]

    [Header("Variables")]
    [SerializeField] bool _permaHarmonizable;
    [SerializeField] HarmonizationType _harmonizationType;
    private bool isHarmonized = false;
    private bool isStunned = false;
    private bool hasFailed = false;

    [Header("Visuals")]
    [SerializeField]
    List<Renderer> renderers;

    [SerializeField] GameObject _reflectorVisual;
    [SerializeField] Animator animator;
    private const string walkAnim = "StartWalk";

    private void Awake()
    {
        hasFailed = false;

        UpdateMaterials();

        GameplayManagers.Instance.Enemy.AddEnemy(this);

        _reflectorVisual.SetActive(GetComponent<ReflectHarmony>().GetCanReflect());

    }

    // For testing purposes
    private void Update()
    {
        //Commenting this out to make sure it doesn't make it into playtest build - Ryan
        /*if (Input.GetKeyUp(KeyCode.R))
        {
            Move();
        }*/
    }

    // Player Detection
    private void OnTriggerStay(Collider other)
    {
        if (hasFailed)
            return;

        if (!other.CompareTag("Player") || isHarmonized || isStunned)
            return;

        GameplayManagers.Instance.Room.RoomFail();
        hasFailed = true;
    }

    /// <summary>
    /// Moves the enemy based on their pathfinding. If the enemy is harmonized, does nothing.
    /// </summary>
    public void Move()
    {
        if (isHarmonized || isStunned)
            return;

        Vector2 moveDirection = GetNextMove();
        if (moveDirection == Vector2.zero)
            return;

        StartCoroutine(MoveAndRotate(timeToMove, timeToRotate, moveDirection));
    }

    public IEnumerator MoveAndRotate(float moveTime, float rotateTime, Vector2 moveDirection)
    {
        StartWalkAnim();

        StartCoroutine(RotateOverTime(rotateTime, moveDirection));

        yield return new WaitForSeconds(rotateTime);

        GameplayManagers.Instance.Mover.MoveCharacter(gameObject, moveDirection, moveTime);
    }

    /// <summary>
    /// Rotates the Enemy towards the given direction
    /// </summary>
    /// <param name="direction">Direction to rotate towards</param>
    private void RotateToDirection(Vector2 direction)
    {
        Vector3 newDir = transform.position + new Vector3(direction.x, 0, direction.y);
        transform.LookAt(newDir, Vector3.up);
    }

    private IEnumerator RotateOverTime(float rotationTime, Vector2 direction)
    {
        Vector3 targetRotationEuler = new(0, Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y)).eulerAngles.y, 0);
        Quaternion targetRotation = Quaternion.Euler(targetRotationEuler);

        float time = 0;
        while (time < 1)
        {
            time += Time.deltaTime / rotationTime;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, time);
            yield return new WaitForEndOfFrame();
        }
    }

    /// <summary>
    /// A* pathfinding for enemies
    /// </summary>
    /// <returns>The path generated</returns>
    private List<GridStats> GeneratePath(GridStats startTile, GridStats targetTile)
    {
        if (startTile == null || targetTile == null)
            return null;

        GridStats playerTile = GameplayManagers.Instance.Enemy.playerTile;

        var toSearch = new List<GridStats>() { startTile };
        var processed = new List<GridStats>();

        // Runs while there are tiles to search
        while (toSearch.Any())
        {
            var current = toSearch[0];

            // Checks for the most efficient next tile
            foreach (var t in toSearch)
                if (t.F < current.F || t.F == current.F && t.H < current.H) current = t;

            // Moves that tile to processed and removes it from toSearch
            processed.Add(current);
            toSearch.Remove(current);

            // Checks if the tile is the target, if so creates a finalized path to it
            if (current == targetTile)
            {
                var currentPathTile = targetTile;
                var path = new List<GridStats>();
                var count = 100;
                while (currentPathTile != startTile)
                {
                    path.Add(currentPathTile);
                    currentPathTile = currentPathTile.Connection;
                    count--;
                    if (count < 0) Debug.LogError("Unreachable Count");
                }

                return path;
            }
            //Debug.Log(current.Neighbors.Count);

            // If the tile is not the target, searches through the tile's available and unprocessed neighbors
            foreach (var neighbor in current.Neighbors.Where(t => t.GetGridAvailability() != GridAvailability.Occupied && !processed.Contains(t)))
            {

                // Checks if the neighbor is going to be searched
                var inSearch = toSearch.Contains(neighbor);

                float costToNeighbor;
                if (neighbor == playerTile)
                    costToNeighbor = 0;
                else
                    costToNeighbor = current.G + current.GetDistance(neighbor) + (neighbor.GetGridAvailability() == GridAvailability.Claimed ? claimedCost : 0);

                // If the neighbor is an efficient move, makes the connection for the path later
                if (!inSearch || costToNeighbor < neighbor.G)
                {
                    neighbor.SetG(costToNeighbor);
                    neighbor.SetConnection(current);

                    // If not in search, neighbor is added to be searched later
                    if (!inSearch)
                    {
                        neighbor.SetH(neighbor.GetDistance(targetTile));
                        toSearch.Add(neighbor);
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the direction of the enemie's next move
    /// </summary>
    /// <returns>Direction of next move</returns>
    public Vector2 GetNextMove()
    {
        List<List<GridStats>> paths = new();
        List<GridStats> targets = new();
        //int currentTarget = -1;

        EnemyManager enemyManager = GameplayManagers.Instance.Enemy;
        GridBehavior grid = GameplayManagers.Instance.Grid;

        GridStats startTile = grid.DetermineGridSpaceFromPosition(transform.position);

        Vector3 nextPlayerPosition = PredictNextPlayerPosition();
        Vector3 currentPlayerPosition = enemyManager.GetPlayerTransform().position;
        Vector3 lastPlayerPosition = enemyManager.GetLastPlayerPosition();

        targets.Add(grid.DetermineGridSpaceFromPosition(nextPlayerPosition));
        targets.Add(grid.DetermineGridSpaceFromPosition(currentPlayerPosition));
        targets.Add(grid.DetermineGridSpaceFromPosition(lastPlayerPosition));

        foreach (GridStats target in targets)
            paths.Add(GeneratePath(startTile, target));

        int shortest = 9999;
        foreach(List<GridStats> path in paths)
        {
            if (path == null)
                continue;

            if(path.Count < shortest - prioritizedBonus)
            {
                this.path = path;
                shortest = path.Count;
            }
        }

        if(path == null || path.Count == 0)
        {
            Debug.LogWarning("Pathfinding Error: No Path Found");
            return Vector2.zero;
        }

        foreach(GridStats tile in path)
        {
            tile.SetGridAvailability(GridAvailability.Claimed);
        }

        startTile.SetGridAvailability(GridAvailability.Empty);
        path[^1].SetGridAvailability(GridAvailability.Occupied);
        return startTile.GetDirectionTo(path[^1]);
    }

    IEnumerator DelayedClearTile(GridStats tile)
    {
        yield return new WaitForFixedUpdate();
        tile.SetGridAvailability(GridAvailability.Empty);
    }

    public void ClearPath()
    {
        foreach (GridStats tile in path)
            tile.FindObjectAbove(true);
    }

    private Vector3 PredictNextPlayerPosition()
    {
        EnemyManager enemyManager = GameplayManagers.Instance.Enemy;
        Vector3 predictedPos = (2 * enemyManager.GetPlayerTransform().position) - enemyManager.GetLastPlayerPosition();

        return predictedPos;
    }

    /// <summary>
    /// Updates the enemy's materials to match their harmonization state
    /// </summary>
    private void UpdateMaterials()
    {
        Material newMaterial;
        if (isHarmonized)
            newMaterial = GameplayManagers.Instance.Enemy.GetHarmonizedMaterial(_harmonizationType);
        else if (isStunned)
            newMaterial = GameplayManagers.Instance.Enemy.GetStunnedMaterial(_harmonizationType);
        else
            newMaterial = GameplayManagers.Instance.Enemy.GetDissonantMaterial(_harmonizationType);

        foreach (Renderer r in renderers)
            r.material = newMaterial;
    }

    /// <summary>
    /// Starts the walk animation
    /// </summary>
    private void StartWalkAnim()
    {
        animator.SetTrigger(walkAnim);
        Debug.Log("--------------------------------------------------------------");
    }

    // Setters and Getters
    public int GetPathLength()
    {
        return path.Count;
    }

    public void SetHarmonized(bool harmonized, HarmonizationType hType)
    {
        isStunned = harmonized;
        if (hType == _harmonizationType)
            isHarmonized = harmonized;

        UpdateMaterials();
        MusicController.Instance.UpdateEnemyMusic(!harmonized);
    }

    public bool IsHarmonized()
    {
        return isHarmonized;
    }

/*    public bool IsStunned()
    {
        return IsStunned
    }*/

    public HarmonizationObjectCatagory GetHarmonizationCatagory()
    {
        return HarmonizationObjectCatagory.Enemy;
    }

    public HarmonizationType GetHarmonizationType()
    {
        return _harmonizationType;
    }

    public bool IsPermaHarmonizable()
    {
        return _permaHarmonizable;
    }
}
