using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: Liz
/// Description: Holds every manager for easy editor use
/// </summary>
public class ManagerParent : MonoBehaviour
{
    [SerializeField] private List<GameObject> _managerPrefabs;

    private void Awake()
    {
        SpawnManagers();
    }

    /// <summary>
    /// Spawns every manager
    /// </summary>
    private void SpawnManagers()
    {
        foreach (GameObject manager in _managerPrefabs)
        {
            Instantiate(manager, transform.position, Quaternion.identity);
        }
    }
}