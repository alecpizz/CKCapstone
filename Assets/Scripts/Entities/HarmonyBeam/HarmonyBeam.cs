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

    // Track enemy states: true = hit, false = not hit
    private Dictionary<EnemyBehavior, bool> _enemyHitStates = new();
    private EventInstance _key;
    private bool _toggled = true;

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
        if (_toggled)
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
        _lineRenderer.SetPosition(0, startPosition);

        Vector3 currentStartPosition = startPosition;
        Vector3 currentDirection = direction;
        float remainingDistance = distance;

        int reflections = 0;
        const int maxReflections = 10;

        HashSet<EnemyBehavior> currentlyHitEnemies = new();  // Track enemies hit in this frame

        while (remainingDistance > 0 && reflections < maxReflections)
        {
            RaycastHit[] hits = Physics.RaycastAll(currentStartPosition, currentDirection, remainingDistance);
            System.Array.Sort(hits, (hit1, hit2) => hit1.distance.CompareTo(hit2.distance));

            bool hitSomething = false;
            foreach (RaycastHit hit in hits)
            {
                _lineRenderer.positionCount += 1;
                _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, hit.transform.position);

                if (hit.collider.CompareTag("Enemy"))
                {
                    EnemyBehavior enemyBehavior = hit.collider.GetComponent<EnemyBehavior>();
                    if (enemyBehavior != null)
                    {
                        currentlyHitEnemies.Add(enemyBehavior);  // Track the hit enemy behavior

                        // Immediately freeze the enemy upon being hit by this beam
                        if (!enemyBehavior.enemyFrozen)  // Freeze only if not already frozen
                        {
                            HarmonyBeamManager.Instance.BeamHitsEnemy(enemyBehavior);
                        }

                        // Mark this enemy as being hit in this frame
                        _enemyHitStates[enemyBehavior] = true;
                    }
                    continue;
                }
                else if (hit.collider.CompareTag("Reflective"))
                {
                    var reflectiveObject = hit.collider.GetComponent<ReflectiveObject>();
                    if (reflectiveObject)
                    {
                        currentDirection = reflectiveObject.GetReflectionDirection(currentDirection);
                        currentStartPosition = hit.transform.position;
                        remainingDistance -= hit.distance;
                        reflections++;
                        hitSomething = true;
                        break;
                    }
                }
                else
                {
                    remainingDistance = 0;
                    hitSomething = true;
                    break;
                }
            }

            if (!hitSomething)
            {
                _lineRenderer.positionCount += 1;
                _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, currentStartPosition + currentDirection * remainingDistance);
                break;
            }
        }

        // Handle enemies that are no longer being hit by this beam
        List<EnemyBehavior> enemiesToUnfreeze = new();
        foreach (var enemy in _enemyHitStates.Keys)
        {
            if (_enemyHitStates[enemy] && !currentlyHitEnemies.Contains(enemy))
            {
                // This enemy was hit before but is no longer hit in this frame
                enemiesToUnfreeze.Add(enemy);
            }
        }

        foreach (EnemyBehavior enemy in enemiesToUnfreeze)
        {
            _enemyHitStates[enemy] = false;  // Mark the enemy as no longer hit by this beam
            HarmonyBeamManager.Instance.BeamStopsHittingEnemy(enemy);  // Report to the manager
        }
    }

    /// <summary>
    /// Toggles the beam on and off when called, handles sound accordingly.
    /// </summary>
    public void ToggleBeam()
    {
        _toggled = !_toggled;
        AudioManager.Instance.ToggleSound(_key, _toggled);
    }

    private void OnDestroy()
    {
        AudioManager.Instance.StopSound(_key, true);
    }
}
