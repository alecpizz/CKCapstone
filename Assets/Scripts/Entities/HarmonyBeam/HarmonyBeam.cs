/******************************************************************
 *    Author: Claire Noto
 *    Contributors: Claire Noto, Trinity Hutson, Alec Pizziferro
 *    Date Created: 10/10/24
 *    Description: Script that handles the harmony beam
 *******************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// A script that handles a harmony beam, and invoking
/// exiting and entering for the harmony beam.
/// </summary>
public class HarmonyBeam : MonoBehaviour, ITurnListener
{
    [SerializeField] private EventReference _harmonySound = default;
    [SerializeField] private EventReference _enemyHarmonization = default;
    [Space] [SerializeField] private bool beamActive = true;
    [SerializeField] private float beamDetectionWaitTime = 0.1f;

    // Array for managing the multiple child particle systems
    [Header("Particles")] [SerializeField] private ParticleSystem[] _beamParticleSystems;
    [SerializeField] private GameObject _enemyHitEffectPrefab;
    [SerializeField] private GameObject _wallCollisionEffectPrefab;

    private GameObject _activeWallEffect; // Instance of the active wall collision effect


    private readonly List<IHarmonyBeamEntity> _hitEntities = new();
    private readonly HashSet<IHarmonyBeamEntity> _prevHitEntities = new();
    private Dictionary<IHarmonyBeamEntity, GameObject> _hitEffectEntities = new();
    private List<Vector3> _debugTiles = new();
    private bool _enemyGrabbedAudioPlaying = false;
    private EventInstance _beamInstance;
    private EventInstance _enemyGrabbedInstance;
    private Collider _prevHitCollider;
    

    /// <summary>
    /// Starts sound playback and instantiates a wall effect if possible.
    /// </summary>
    private void Start()
    {
        _beamInstance = AudioManager.Instance.PlaySound(_harmonySound);
        if (_wallCollisionEffectPrefab != null)
        {
            _activeWallEffect = Instantiate(_wallCollisionEffectPrefab);
            _activeWallEffect.SetActive(false);
        }
    }

    private void OnEnable()
    {
        RoundManager.Instance.RegisterListener(this);
    }

    private void OnDisable()
    {
        RoundManager.Instance.RegisterListener(this);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        _debugTiles.ForEach(tile => Gizmos.DrawSphere(tile, 0.5f));
    }

    /// <summary>
    /// Shoots the line renderer and raycast.
    /// </summary>
    /// <param name="startPosition">position the laser starts from</param>
    /// <param name="direction">direction the laser is going</param>
    /// <param name="distance">distance the laser will go</param>
    private void ShootLaser(Vector3 startPosition, Vector3 direction, float distance)
    {
        Vector3 currentStartPosition = startPosition;
        Vector3 currentDirection = direction;
        float remainingDistance = distance;

        

        _hitEntities.Clear();
        bool enemyHit = false;
        if (beamActive)
        {
            //we hit some things
            // if (size > 0)
            // {
            //we hit some things
            // for (int i = 0; i < size; i++)
            // {
            //     var hit = _raycastHitCache[i];
            //     var entity = hit.collider.GetComponentInParent<IHarmonyBeamEntity>();
            //     if (entity != null)
            //     {
            //         UpdateWallEffect(false);
            //         //TODO: prevent this from firing over and over.
            //         entity.OnLaserHit(hit);
            //         _hitEntities.Add(entity);
            //         if (entity.HitWrapAround)
            //         {
            //             //enemy vfx, gross due to aformentioned TODO
            //             if (!_hitEffectEntities.ContainsKey(entity) || 
            //                 (_hitEffectEntities.ContainsKey(entity) && _hitEffectEntities[entity] == null))
            //             {
            //                 GameObject enemyFX = Instantiate(_enemyHitEffectPrefab, entity.Position,
            //                     Quaternion.identity);
            //                 _hitEffectEntities.TryAdd(entity, enemyFX);
            //                 _hitEffectEntities[entity] = enemyFX;
            //                 _enemyGrabbedInstance = AudioManager.Instance.PlaySound(_enemyHarmonization);
            //             }
            //
            //             enemyHit = true;
            //         }
            //
            //         if (!entity.AllowLaserPassThrough)
            //         {
            //             break;
            //         }
            //     }
            //     else
            //     {
            //         //we hit something else, but it's untagged/no component, assume stopping.
            //         UpdateWallEffect(true, hit.point, hit.normal);
            //         break;
            //     }
            // }
            // }
            // else
            

            {
                //didn't hit anything
                UpdateWallEffect(false);
            }
        }

        //check for anyone who's left.
        foreach (var harmonyBeamEntity in _prevHitEntities)
        {
            if (_hitEntities.Contains(harmonyBeamEntity)) continue;
            //disable any hit enemy vfx here too :)
            harmonyBeamEntity.OnLaserExit();
            if (!_hitEffectEntities.TryGetValue(harmonyBeamEntity, out var fx)) continue;
            Destroy(fx.gameObject);
            _hitEntities.Remove(harmonyBeamEntity);
        }

        //try and play enemy audio if it state changed.
        if (enemyHit != _enemyGrabbedAudioPlaying)
        {
            _enemyGrabbedAudioPlaying = enemyHit;
            if (_enemyGrabbedAudioPlaying)
            {
                _enemyGrabbedInstance = AudioManager.Instance.PlaySound(_enemyHarmonization);
            }
            else
            {
                AudioManager.Instance.StopSound(_enemyGrabbedInstance);
            }
        }

        _prevHitEntities.Clear();

        _hitEntities.ForEach(entity => { _prevHitEntities.Add(entity); });
    }


    /// <summary>
    /// Updates the beam based on the parameter passed, instead of automatically checking the toggle state.
    /// </summary>
    /// <param name="toggle"></param>
    public void ToggleBeam(bool toggle)
    {
        if (toggle == beamActive)
        {
            return;
        }

        beamActive = toggle;

        if (toggle)
        {
            foreach (var particleSystem in _beamParticleSystems)
            {
                particleSystem.Play();
            }
            _prevHitEntities.Clear();
        }
        else
        {
            foreach (var particleSystem in _beamParticleSystems)
            {
                particleSystem.Stop();
            }
        }

        AudioManager.Instance.PauseSound(_beamInstance, beamActive);
    }
    

    /// <summary>
    /// Handles the visual effect when the beam hits a wall.
    /// </summary>
    /// <param name="hitPosition">The position where the wall was hit</param>
    /// <param name="hitNormal">The normal of the wall at the hit point</param>
    private void UpdateWallEffect(bool active, Vector3? hitPosition = null, Vector3? hitNormal = null)
    {
        if (_activeWallEffect == null) return;
        // Offset the hit position slightly in the direction of the normal to ensure the effect is on the surface
        if (hitPosition.HasValue && hitNormal.HasValue)
        {
            Vector3 surfacePosition = hitPosition.Value + hitNormal.Value * 0.05f;
            _activeWallEffect.transform.position = surfacePosition;
        }

        if (_activeWallEffect.activeSelf != active)
        {
            _activeWallEffect.SetActive(active);
        }
    }

    public TurnState TurnState => TurnState.World;

    public void BeginTurn(Vector3 direction)
    {
        StartCoroutine(WaitForPotentialBlockers());
    }

    private IEnumerator WaitForPotentialBlockers()
    {
        //gross! we shouldn't have to wait, but the moving walls/platforms aren't turn based currently... 
        yield return new WaitForSeconds(beamDetectionWaitTime);
        DetectObjects();

        RoundManager.Instance.CompleteTurn(this);
    }

    public void DetectObjects()
    {
        _hitEntities.Clear();
        _debugTiles.Clear();
        if (beamActive)
        {
            var currTilePos = (transform.position);
            var fwd = transform.forward;
            bool stop = false;
            while (!stop)
            {
                var nextCell = GridBase.Instance.GetCellPositionInDirection(currTilePos, fwd);
                _debugTiles.Add(currTilePos);
                if (currTilePos == nextCell) //no where to go :(
                {
                    stop = true;
                }
                currTilePos = nextCell;

                var entries = GridBase.Instance.GetCellEntries(nextCell);
                foreach (var gridEntry in entries)
                {
                    if (gridEntry == null) continue;
                    if (gridEntry.GetGameObject.TryGetComponent(out IHarmonyBeamEntity entity))
                    {
                        if (!_prevHitEntities.Contains(entity))
                        {
                            entity.OnLaserHit(default);
                        }
                        _hitEntities.Add(entity);
                        if (!entity.AllowLaserPassThrough)
                        {
                            stop = true;
                        }

                        if (entity.HitWrapAround)
                        {
                        }
                    }
                    else if (!gridEntry.IsTransparent)
                    {
                        stop = true;
                    }
                }

            }
        }
        else
        {
            
        }
        
        foreach (var harmonyBeamEntity in _prevHitEntities)
        {
            if (!_hitEntities.Contains(harmonyBeamEntity))
            {
                harmonyBeamEntity.OnLaserExit();
            }
        }
        _prevHitEntities.Clear();
        _hitEntities.ForEach(entity => _prevHitEntities.Add(entity));
    }

    public void ForceTurnEnd()
    {
       
    }
}