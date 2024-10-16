/******************************************************************
*    Author: Claire Noto
*    Contributors: Claire Noto, Trinity Hutson
*    Date Created: 10/10/24
*    Description: Script that handles the harmony beam
*******************************************************************/
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class HarmonyBeam : MonoBehaviour
{
    [SerializeField] private float _laserDistance = 50f;
    [SerializeField] private EventReference _harmonySound = default;

    private LineRenderer _lineRenderer;

    private EnemyBehavior eb;

    // Track enemy states: true = hit, false = not hit
    private Dictionary<GameObject, bool> _enemyHitStates = new();
    private EventInstance _key;

    void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        _key = AudioManager.Instance.PlaySound(_harmonySound);
    }

    void FixedUpdate()
    {
        ShootLaser(transform.position, transform.forward, _laserDistance);
    }

    /// <summary>
    /// Shoots the line renderer and raycast.
    /// </summary>
    /// <param name="startPosition">position the laser starts from</param>
    /// <param name="direction">direction the laser is going</param>
    /// <param name="distance">distance the laser will go</param>
    public void ShootLaser(Vector3 startPosition, Vector3 direction, float distance)
    {
        // Clear the previous line and start a new one
        _lineRenderer.positionCount = 1;
        _lineRenderer.SetPosition(0, startPosition);  // Starting point of the laser

        Vector3 currentStartPosition = startPosition;
        Vector3 currentDirection = direction;
        float remainingDistance = distance;

        int reflections = 0;  
        const int maxReflections = 10;  // Limit the number of reflections to prevent infinite loops

        HashSet<GameObject> currentlyHitEnemies = new();  // Used to track enemies hit in this frame

        while (remainingDistance > 0 && reflections < maxReflections)
        {
            RaycastHit[] hits = Physics.RaycastAll(currentStartPosition, currentDirection, remainingDistance);
            System.Array.Sort(hits, (hit1, hit2) => hit1.distance.CompareTo(hit2.distance));  // Sort hits by distance

            bool hitSomething = false;
            foreach (RaycastHit hit in hits)
            {
                _lineRenderer.positionCount += 1;
                _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, hit.transform.position);

                if (hit.collider.CompareTag("Enemy"))
                {
                    GameObject enemy = hit.collider.gameObject;
                    currentlyHitEnemies.Add(enemy);

                    // Check if the enemy was not hit before
                    if (!_enemyHitStates.ContainsKey(enemy) || !_enemyHitStates[enemy])
                    {
                        // Handle when an enemy first enters the beam
                        _enemyHitStates[enemy] = true;
                        Debug.Log("Froze enemy: " + enemy.name);

                        eb = hit.collider.GetComponent<EnemyBehavior>();
                        eb.enemyFrozen = true;
                    }

                    continue;
                }
                else if (hit.collider.CompareTag("Reflective"))
                {
                    var reflectiveObject = hit.collider.GetComponent<ReflectiveObject>();
                    if (reflectiveObject)
                    {
                        // Get the reflected direction from the ReflectiveObject
                        currentDirection = reflectiveObject.GetReflectionDirection(currentDirection);

                        // Update currentStartPosition to where the beam hit the reflective object
                        currentStartPosition = hit.transform.position;
                        Debug.Log("New Position: " + currentStartPosition);
                        remainingDistance -= hit.distance;
                        reflections++;
                        hitSomething = true;
                        break;
                    }
                }
                else
                {
                    // Stop the beam at a non-enemy, non-reflective object
                    remainingDistance = 0;
                    hitSomething = true;
                    break;
                }
            }

            if (!hitSomething)
            {
                _lineRenderer.positionCount += 1;
                _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, currentStartPosition + currentDirection * remainingDistance);
                //_lineRenderer.SetPosition(_lineRenderer.positionCount - 1, currentStartPosition);
                break;
            }
        }

        // Handle when enemies leave the beam
        List<GameObject> enemiesToUnfreeze = new();
        foreach (var enemy in _enemyHitStates.Keys)
        {
            if (!_enemyHitStates[enemy]) continue;  // If already unfrozen, skip

            if (!currentlyHitEnemies.Contains(enemy))
            {
                // The enemy was hit before but is no longer in the beam
                Debug.Log("Unfroze enemy: " + enemy.name);

                eb = enemy.GetComponent<EnemyBehavior>();
                eb.enemyFrozen = false;

                enemiesToUnfreeze.Add(enemy);
            }
        }

        foreach (GameObject enemy in enemiesToUnfreeze)
        {
            _enemyHitStates[enemy] = false;
        }
    }

    private void OnDestroy()
    {
        AudioManager.Instance.StopSound(_key, true);
    }
}
