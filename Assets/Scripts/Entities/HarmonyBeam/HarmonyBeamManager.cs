/******************************************************************
*    Author: Claire Noto
*    Contributors: Claire Noto
*    Date Created: 10/21/24
*    Description: Script that tracks all harmony beams in the scene.
*                 Used for enemy frozen states.
*******************************************************************/
using System.Collections.Generic;
using UnityEngine;

public class HarmonyBeamManager : MonoBehaviour
{
    private static HarmonyBeamManager _instance;

    // Track how many beams are affecting each enemy
    private Dictionary<EnemyBehavior, int> _enemyHitCounts = new();
    public static HarmonyBeamManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<HarmonyBeamManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("HarmonyBeamManager");
                    _instance = obj.AddComponent<HarmonyBeamManager>();
                }
            }
            return _instance;
        }
    }

    // Called when a beam starts hitting an enemy
    public void BeamHitsEnemy(EnemyBehavior enemy)
    {
        if (!_enemyHitCounts.ContainsKey(enemy))
        {
            _enemyHitCounts[enemy] = 0;
        }

        _enemyHitCounts[enemy]++;

        // If this is the first beam hitting the enemy, freeze it
        if (_enemyHitCounts[enemy] == 1)
        {
            enemy.enemyFrozen = true;
            Debug.Log("Froze enemy: " + enemy.name);
        }
    }

    public void BeamStopsHittingEnemy(EnemyBehavior enemy)
    {
        if (_enemyHitCounts.ContainsKey(enemy))
        {
            _enemyHitCounts[enemy]--;

            // If no more beams are hitting the enemy, unfreeze it
            if (_enemyHitCounts[enemy] <= 0)
            {
                enemy.enemyFrozen = false;
                Debug.Log("Unfroze enemy: " + enemy.name);
                _enemyHitCounts.Remove(enemy);  // Remove enemy from tracking
            }
        }
    }
}