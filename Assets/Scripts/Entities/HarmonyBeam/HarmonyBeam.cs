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
    [SerializeField] private EventReference _harmonySound = default;
    [SerializeField] private EventReference _enemyHarmonization = default;

    [Space]
    [SerializeField]
    private bool _toggled = true;

    // Array for managing the multiple child particle systems
    [Header("Particles")]
    [SerializeField] private ParticleSystem[] _beamParticleSystems;
    [SerializeField] private GameObject _enemyHitEffectPrefab;
    [SerializeField] private GameObject _wallCollisionEffectPrefab;

    private GameObject _activeWallEffect; // Instance of the active wall collision effect

    private Dictionary<EnemyBehavior, GameObject> _activeEnemyEffects = new(); // Tracks active enemy effects
    private Dictionary<EnemyBehavior, bool> _enemyHitStates = new(); // Stores enemy hit states

    // There is a better way to do this, but this works for now
    private HashSet<EnemyBehavior> _currentlyHitEnemies = new(); // Checks enemy hit states every fixed frame
    private HashSet<ReflectiveObject> _currentlyHitReflectors = new(); // Same but for reflective objects
    private HashSet<ReflectiveObject> _previouslyHitReflectors = new();

    private const float _laserDistance = 50f;

    // Used for cutting the beam after it is blocked
    private int _alphaFadeID;
    private int _alphaRiseID;
    private Material _beamMat;

    private EventInstance _beamKey;
    private EventInstance _enemyKey;

    void Awake()
    {
        // Initialize the property IDs
        _alphaFadeID = Shader.PropertyToID("_AlphaFade");
        _alphaRiseID = Shader.PropertyToID("_AlphaRise");

        var renderer = _beamParticleSystems[0].GetComponent<Renderer>();
        if (renderer != null)
        {
            // Check materials in the renderer
            for (int i = 0; i < renderer.materials.Length; i++)
            {
                if (renderer.materials[i].name.Contains("MAT_HarmonyBeam 1"))
                {
                    _beamMat = renderer.materials[i];
                    break;
                }
            }
        }
    }

    private void Start()
    {
        _beamKey = AudioManager.Instance.PlaySound(_harmonySound);
        
        // Will be done later
        //_enemyKey = AudioManager.Instance.PlaySound(_enemyHarmonization);
        //AudioManager.Instance.PauseSound(_enemyKey, true);
    }

    void FixedUpdate()
    {
        if (_toggled)
        {
            ShootLaser(transform.position, transform.forward, _laserDistance);
        }
    }

    /// <summary>
    /// Shoots the line renderer and raycast.
    /// </summary>
    /// <param name="startPosition">position the laser starts from</param>
    /// <param name="direction">direction the laser is going</param>
    /// <param name="distance">distance the laser will go</param>
    public void ShootLaser(Vector3 startPosition, Vector3 direction, float distance)
    {
        Vector3 currentStartPosition = startPosition;
        Vector3 currentDirection = direction;
        float remainingDistance = distance;

        int reflections = 0;
        const int maxReflections = 10;

        _currentlyHitEnemies = new();  // Track enemies hit in this frame
        _currentlyHitReflectors = new(); 

        float blockedDistance = distance;
        bool wallHit = false;

        while (remainingDistance > 0 && reflections < maxReflections)
        {
            RaycastHit[] hits = Physics.RaycastAll(currentStartPosition, currentDirection, remainingDistance);
            System.Array.Sort(hits, (hit1, hit2) => hit1.distance.CompareTo(hit2.distance));

            bool hitSomething = false;
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.TryGetComponent(out EnemyBehavior enemyBehavior))
                {

                    _currentlyHitEnemies.Add(enemyBehavior);  // Track the hit enemy behavior

                    if (!enemyBehavior.enemyFrozen)
                    {
                        HarmonyBeamManager.Instance.BeamHitsEnemy(enemyBehavior);
                    }

                    // Will do later
                    //AudioManager.Instance.PauseSound(_enemyKey, false);

                    _enemyHitStates[enemyBehavior] = true;

                    // Handle enemy hit effect
                    HandleEnemyHitEffect(enemyBehavior);             
                }
                else if (hit.collider.TryGetComponent(out ReflectiveObject reflectiveObject))
                {
                    hitSomething = true;

                    if (_currentlyHitReflectors.Contains(reflectiveObject))
                        continue;

                    _currentlyHitReflectors.Add(reflectiveObject);

                    // Notify the ReflectiveObject that it has been hit
                    reflectiveObject.ToggleBeam(true);

                    currentDirection = reflectiveObject.GetReflectionDirection(currentDirection);
                    currentStartPosition = hit.point;
                    remainingDistance -= hit.distance;
                    reflections++;
                    break;                   
                }
                else if (hit.collider.CompareTag("Wall"))
                {
                    // Update the blocked distance and handle wall collision effect
                    blockedDistance = Vector3.Distance(startPosition, hit.point);

                    // Pass the hit point and normal to the effect handler
                    HandleWallCollisionEffect(hit.point, hit.normal);

                    wallHit = true;
                    remainingDistance = 0;
                    hitSomething = true;
                    break;
                }
                else
                {
                    // Non-wall hit, ignore for wall effect
                    blockedDistance = Vector3.Distance(startPosition, hit.point);
                    remainingDistance = 0;
                    hitSomething = true;
                    break;
                }
            }

            if (!hitSomething)
            {
                break;
            }
        }

        // If no wall is hit, remove the wall collision effect
        if (!wallHit)
        {
            DisableWallCollisionEffect();
        }

        // Adjust Alpha values based on blocked distance
        _beamMat.SetFloat(_alphaRiseID, ConvertWorldUnitsToAlphaRise(blockedDistance));

        // Remove effects from enemies that are no longer being hit
        RemoveObsoleteEnemyEffects();

        // Handle enemies that are no longer being hit
        List<EnemyBehavior> enemiesToUnfreeze = new();
        foreach (var enemy in _enemyHitStates.Keys)
        {
            if (_enemyHitStates[enemy] && !_currentlyHitEnemies.Contains(enemy))
            {
                enemiesToUnfreeze.Add(enemy);
            }
        }

        foreach (EnemyBehavior enemy in enemiesToUnfreeze)
        {
            _enemyHitStates[enemy] = false;
            HarmonyBeamManager.Instance.BeamStopsHittingEnemy(enemy);
        }

        // Untoggles unused relfective objects
        foreach (ReflectiveObject r in _previouslyHitReflectors)
        {
            if (!_currentlyHitReflectors.Contains(r))
            {
                r.ToggleBeam(false);
            }
        }

        // Clears previous reflectors and replaces it with the ones captured this tick
        _previouslyHitReflectors = _currentlyHitReflectors;
    }

    /// <summary>
    /// Handles the visual effect when the beam hits an enemy.
    /// </summary>
    /// <param name="enemy">The enemy being hit</param>
    private void HandleEnemyHitEffect(EnemyBehavior enemy)
    {
        if (_enemyHitEffectPrefab != null)
        {
            if (!_activeEnemyEffects.ContainsKey(enemy))
            {
                // Instantiate the effect and parent it to the enemy
                GameObject enemyEffect = Instantiate(_enemyHitEffectPrefab);
                enemyEffect.transform.SetParent(enemy.transform, true);
                enemyEffect.transform.localPosition = Vector3.zero;
                _activeEnemyEffects[enemy] = enemyEffect;
            }
        }
    }

    /// <summary>
    /// Removes the visual effect from enemies that are no longer being hit.
    /// </summary>
    /// <param name="currentlyHitEnemies">Set of enemies currently being hit</param>
    private void RemoveObsoleteEnemyEffects()
    {
        List<EnemyBehavior> enemiesToRemove = new();

        // Find enemies that are no longer hit
        foreach (var kvp in _activeEnemyEffects)
        {
            if (!_currentlyHitEnemies.Contains(kvp.Key))
            {
                kvp.Value.SetActive(false);
                enemiesToRemove.Add(kvp.Key);
            }
        }

        // Remove the effects from the dictionary
        foreach (var enemy in enemiesToRemove)
        {
            _activeEnemyEffects.Remove(enemy);
        }
    }
    
    /// <summary>
    /// Handles the visual effect when the beam hits a wall.
    /// </summary>
    /// <param name="hitPosition">The position where the wall was hit</param>
    /// <param name="hitNormal">The normal of the wall at the hit point</param>
    private void HandleWallCollisionEffect(Vector3 hitPosition, Vector3 hitNormal)
    {
        if (_wallCollisionEffectPrefab != null)
        {
            // Offset the hit position slightly in the direction of the normal to ensure the effect is on the surface
            Vector3 surfacePosition = hitPosition + hitNormal;

            // If no active effect exists, instantiate it
            if (_activeWallEffect == null)
            {
                _activeWallEffect = Instantiate(_wallCollisionEffectPrefab, surfacePosition, Quaternion.identity);
            }
            else
            {
                // Update the position if it's already active
                _activeWallEffect.transform.position = surfacePosition;
                _activeWallEffect.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Disables the wall collision effect when it's no longer needed.
    /// </summary>
    private void DisableWallCollisionEffect()
    {
        if (_activeWallEffect != null)
        {
            _activeWallEffect.SetActive(false);
        }
    }

    /// <summary>
    /// Converts a blocked distance in world units to the appropriate alphaRise value based on the current AlphaFade.
    /// </summary>
    /// <param name="blockedDistance">The distance at which the beam is blocked (in world units)</param>
    /// <returns>The corresponding alphaRise value</returns>
    private float ConvertWorldUnitsToAlphaRise(float blockedDistance)
    {
        // Set the current AlphaFade value from the material to 16
        // (16 is the value I made a dataset for)
        _beamMat.SetFloat(_alphaFadeID, 16f);

        // Use AlphaFade = 16 dataset
        return InterpolateAlphaRise(blockedDistance, 
            new float[] { 1, 2, 3, 4, 5, 6 }, 
            new float[] { 8, 10, 11.5f, 12.5f, 13, 13.5f });
    }

    /// <summary>
    /// Interpolates alphaRise based on blocked distance using a piecewise linear approximation.
    /// </summary>
    /// <param name="blockedDistance">The distance at which the beam is blocked (in world units)</param>
    /// <param name="distances">Array of known distances in world units</param>
    /// <param name="alphaRiseValues">Array of corresponding alphaRise values</param>
    /// <returns>Interpolated alphaRise value</returns>
    private float InterpolateAlphaRise(float blockedDistance, float[] distances, float[] alphaRiseValues)
    {
        // Clamp the blockedDistance to the range of known distances
        blockedDistance = Mathf.Clamp(blockedDistance, distances[0], distances[^1]);

        // Iterate over distances to find the closest segment
        for (int i = 0; i < distances.Length - 1; i++)
        {
            if (blockedDistance >= distances[i] && blockedDistance <= distances[i + 1])
            {
                // Perform linear interpolation between the two points
                float t = (blockedDistance - distances[i]) / (distances[i + 1] - distances[i]);
                return Mathf.Lerp(alphaRiseValues[i], alphaRiseValues[i + 1], t);
            }
        }

        // If no segment is found, return the last value (shouldn't happen due to clamping)
        return alphaRiseValues[^1];
    }

    /// <summary>
    /// Toggles the beam on and off when called
    /// </summary>
    public void ToggleBeam()
    {
        _toggled = !_toggled;
        if (_toggled)
        {
            foreach (var particleSystem in _beamParticleSystems)
            {
                particleSystem.Play();
            }
        }
        else
        {
            foreach (var particleSystem in _beamParticleSystems)
            {
                particleSystem.Stop();
            }
        }
        DisableWallCollisionEffect();
        RemoveObsoleteEnemyEffects();
        AudioManager.Instance.PauseSound(_beamKey, _toggled);
    }

    /// <summary>
    /// Updates the beam based on the parameter passed, instead of automatically checking the toggle state.
    /// </summary>
    /// <param name="toggle"></param>
    public void ToggleBeam(bool toggle)
    {
        if (toggle)
        {
            if (_toggled)
                return;

            foreach (var particleSystem in _beamParticleSystems)
            {
                particleSystem.Play();
            }
        }
        else
        {
            if (!_toggled)
                return;

            foreach (var particleSystem in _beamParticleSystems)
            {
                particleSystem.Stop();
            }

            DisableWallCollisionEffect();
            RemoveObsoleteEnemyEffects();
        }

        _toggled = toggle;

        AudioManager.Instance.PauseSound(_beamKey, _toggled);
    }
}