using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Author: Trinity
/// Description: Manages all enemies in a scene
/// </summary>
public class EnemyManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField]
    PlayerManager player;

    [SerializeField]
    Transform playerTransform;

    [Header("Enemy Materials")]
    [SerializeField] List<EnemyMaterials> _enemyMaterials;

    [Header("Enemy Stats")]
    [SerializeField]
    float timeToMove = 0.2f;

    [SerializeField]
    EnemySortType enemySortType = EnemySortType.DISTANCE_DESCENDING;

    List<EnemyBehavior> enemies = new();

    private Vector3 lastPlayerPosition = Vector3.zero;

    public GridStats playerTile { get; private set; }

    private void Start()
    {
        GameplayManagers.Instance.Room.RoomVictoryEvent += OnRoomVictory;
    }

    /// <summary>
    /// Adds an enemy to the enemies list to be tracked and managed
    /// </summary>
    /// <param name="enemy">Enemey to add. Should only be added once</param>
    public void AddEnemy(EnemyBehavior enemy)
    {
        enemies.Add(enemy);
    }

    /// <summary>
    /// Calls Move() for all enemies
    /// </summary>
    public void MoveAllEnemies()
    {
        SortEnemies(enemySortType);

        //Debug.Log("-------------------------------------");
        foreach (EnemyBehavior enemy in enemies)
        {
            //Debug.Log("Path Length: " + enemy.GetPathLength());
            enemy.Move();
        }

        foreach (EnemyBehavior enemy in enemies)
            enemy.ClearPath();
    }

    /// <summary>
    /// Sorts enemies based on distance
    /// </summary>
    /// <param name="sortType">Order by which the enemies are sorted</param>
    private void SortEnemies(EnemySortType sortType)
    {
        switch (sortType)
        {
            case EnemySortType.NONE:
                return;

            case EnemySortType.PATH_LENGTH_ASCENDING:
                enemies = enemies.OrderBy(p => p.GetPathLength()).ToList();
                return;
            case EnemySortType.PATH_LENGTH_DESCENDING:
                enemies = enemies.OrderByDescending(p => p.GetPathLength()).ToList();
                return;

            case EnemySortType.DISTANCE_ASCENDING:
                enemies = enemies.OrderBy(p => Vector3.Distance(p.transform.position, playerTransform.position)).ToList();
                return;
            case EnemySortType.DISTANCE_DESCENDING:
                enemies = enemies.OrderByDescending(p => Vector3.Distance(p.transform.position, playerTransform.position)).ToList();
                return;
        }
    }

    private EnemyMaterials GetAssociatedEnemyMaterials(HarmonizationType hType)
    {
        foreach (EnemyMaterials em in _enemyMaterials)
        {
            if (em.GetHarmonizationType() == hType)
            {
                return em;
            }
        }
        return null;
    }

    private void OnRoomVictory()
    {
        foreach (EnemyBehavior enemy in enemies)
            if(!enemy.IsHarmonized())
                Destroy(enemy.gameObject);

        GameplayManagers.Instance.Room.RoomVictoryEvent -= OnRoomVictory;
    }

    // Setters and Getters
    public Material GetDissonantMaterial(HarmonizationType hType)
    {
        return GetAssociatedEnemyMaterials(hType).GetDisMaterial();
    }

    public Material GetHarmonizedMaterial(HarmonizationType hType)
    {
        return GetAssociatedEnemyMaterials(hType).GetHarmMaterial();
    }

    public Material GetStunnedMaterial(HarmonizationType hType)
    {
        return GetAssociatedEnemyMaterials(hType).GetStunnedMaterial();
    }

    /*public Material GetStunnedMaterial()
    {
        return stunnedMaterial;
    }*/

    public List<EnemyBehavior> GetEnemyList()
    {
        return enemies;
    }

    public int EnemyCount()
    {
        return enemies.Count;
    }

    public Transform GetPlayerTransform()
    {
        return playerTransform;
    }

    public float GetTimeToMove()
    {
        return timeToMove;
    }

    public float GetTotalTimeToMove()
    {
        return timeToMove * (1 + GameplayManagers.Instance.Mover.GetRotationTimeModifier());
    }

    public void SetLastPlayerPosition(Vector3 newPosition)
    {
        //Debug.Log(newPosition);
        lastPlayerPosition = newPosition;
    }

    public Vector3 GetLastPlayerPosition()
    {
        return lastPlayerPosition;
    }

    public void SetPlayerTile(GridStats tile)
    {
        playerTile = tile;
    }

    public PlayerManager GetPlayer()
    {
        return player;
    }

}

public enum EnemySortType
{
    PATH_LENGTH_ASCENDING,
    PATH_LENGTH_DESCENDING,
    DISTANCE_ASCENDING,
    DISTANCE_DESCENDING,
    NONE
}

[System.Serializable]
public class EnemyMaterials
{
    [SerializeField]
    HarmonizationType _harmonizationType;
    [SerializeField]
    Material dissonantMaterial;

    [SerializeField]
    Material harmonizedMaterial;

    [SerializeField]
    Material stunnedMaterial;

    public HarmonizationType GetHarmonizationType()
    {
        return _harmonizationType;
    }

    public Material GetDisMaterial()
    {
        return dissonantMaterial;
    }
    
    public Material GetHarmMaterial()
    {
        return harmonizedMaterial;
    }

    public Material GetStunnedMaterial()
    {
        return stunnedMaterial;
    }
}

