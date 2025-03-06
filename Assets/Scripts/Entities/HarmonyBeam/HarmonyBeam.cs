/******************************************************************
 *    Author: Claire Noto
 *    Contributors: Claire Noto, Trinity Hutson, Alec Pizziferro,
 *    Nick Grinstead
 *    Date Created: 10/10/24
 *    Description: Script that handles the harmony beam
 *******************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// A script that handles a harmony beam, and invoking
/// exiting and entering for the harmony beam.
/// </summary>
public class HarmonyBeam : MonoBehaviour
{
    [SerializeField] private EventReference _harmonySound = default;
    [SerializeField] private EventReference _enemyHarmonization = default;
    [Space] [SerializeField] private bool _beamActive = true;
    [SerializeField] private float _beamDetectionWaitTime = 0.1f;

    // Array for managing the multiple child particle systems
    [Header("Particles")] [SerializeField] private ParticleSystem[] _beamParticleSystems;
    [SerializeField] private GameObject _enemyHitEffectPrefab;
    [SerializeField] private Vector3 _enemyHitOffset;
    [SerializeField] private GameObject _wallCollisionEffectPrefab;

    private GameObject _activeWallEffect; // Instance of the active wall collision effect
    private readonly List<IHarmonyBeamEntity> _hitEntities = new();
    private readonly HashSet<IHarmonyBeamEntity> _prevHitEntities = new();
    private readonly Dictionary<IHarmonyBeamEntity, GameObject> _wrappedEnemyFX = new();
    private bool _enemyGrabbedAudioPlaying = false;
    private EventInstance _beamInstance;
    private EventInstance _enemyGrabbedInstance;

    public static Action TriggerHarmonyScan;

    /// <summary>
    /// Starts sound playback and instantiates a wall effect if possible.
    /// </summary>
    private void Start()
    {
        TriggerHarmonyScan += ScanForObjects;

        _beamInstance = AudioManager.Instance.PlaySound(_harmonySound);
        if (_wallCollisionEffectPrefab != null)
        {
            _activeWallEffect = Instantiate(_wallCollisionEffectPrefab);
            _activeWallEffect.SetActive(false);
        }
        
        Invoke(nameof(ScanForObjects), 0.1f);
    }

    /// <summary>
    /// Unregisters event callback
    /// </summary>
    private void OnDisable()
    {
        TriggerHarmonyScan -= ScanForObjects;
    }

    /// <summary>
    /// Toggles the beam on and off.
    /// </summary>
    /// <param name="toggle">Whether the beam should be on or off.</param>
    public void ToggleBeam(bool toggle)
    {
        if (toggle == _beamActive)
        {
            return;
        }

        _beamActive = toggle;

        if (toggle)
        {
            foreach (var particleSystem in _beamParticleSystems)
            {
                particleSystem.Play();
            }

            ScanForObjects();
        }
        else
        {
            foreach (var particleSystem in _beamParticleSystems)
            {
                particleSystem.Stop();
            }
        }

        AudioManager.Instance.PauseSound(_beamInstance, _beamActive);
    }

    /// <summary>
    /// Checks for objects ahead of the beam and looks ahead.
    /// </summary>
    internal void ScanForObjects()
    {
        _hitEntities.Clear();
        bool enemyHit = false;
        Vector3? hitPoint = null;
        if (_beamActive)
        {
            var currTilePos = (transform.position);
            var fwd = transform.forward;
            bool stop = false;
            while (!stop)
            {
                var nextCell = GridBase.Instance.GetCellPositionInDirection(currTilePos, fwd);
                if (currTilePos == nextCell) //no where to go :(
                {
                    stop = true;
                }

                currTilePos = nextCell;

                var entries = GridBase.Instance.GetCellEntries(nextCell);
                foreach (var gridEntry in entries) //check each cell
                {
                    if (gridEntry == null) continue;
                    //the entry has a harmony beam type :)
                    if (gridEntry.EntryObject.TryGetComponent(out IHarmonyBeamEntity entity))
                    {
                        //we haven't seen this one before, hit it!
                        if (!_prevHitEntities.Contains(entity))
                        {
                           entity.OnLaserHit();                                     
                        }

                        _hitEntities.Add(entity);
                        if (!entity.AllowLaserPassThrough)
                        {
                            stop = true;
                        }

                        //spawn some enemy vfx
                        if (entity.HitWrapAround)
                        {
                            if (!_wrappedEnemyFX.ContainsKey(entity))
                            {
                                GameObject enemyFX = Instantiate(_enemyHitEffectPrefab, entity.Position
                                    + _enemyHitOffset, Quaternion.identity);
                                _wrappedEnemyFX.TryAdd(entity, enemyFX);
                                _wrappedEnemyFX[entity] = enemyFX;
                                enemyFX.transform.parent = entity.EntityTransform;
                                _enemyGrabbedInstance = 
                                    AudioManager.Instance.PlaySound(_enemyHarmonization);
                            }

                            enemyHit = true;
                        }
                    }
                    //no entry, but a cell that blocks harmony beams. pass through.
                    else if (gridEntry.BlocksHarmonyBeam)
                    {
                        stop = true;
                        hitPoint = gridEntry.Position;
                    }
                }
            }
        }

        CheckEntityExits();
        UpdateEnemyGrabbedAudio(enemyHit);
        UpdateWallEffect(hitPoint.HasValue, hitPoint);
        _prevHitEntities.Clear();
        _hitEntities.ForEach(entity => _prevHitEntities.Add(entity));
    }

    /// <summary>
    /// Handles the visual effect when the beam hits a wall.
    /// </summary>
    /// <param name="active">Whether or not the wall effect should be visible.</param>
    /// <param name="hitPosition">The position where the wall was hit</param>
    private void UpdateWallEffect(bool active, Vector3? hitPosition = null)
    {
        if (_activeWallEffect == null) return;
        // Offset the hit position slightly in the direction of
        // the normal to ensure the effect is on the surface
        if (hitPosition.HasValue)
        {
            Vector3 surfacePosition = hitPosition.Value;
            _activeWallEffect.transform.position = surfacePosition;
        }

        if (_activeWallEffect.activeSelf != active)
        {
            _activeWallEffect.SetActive(active);
        }
    }

    /// <summary>
    /// Attempts to turn on/off the enemy grabbed audio.
    /// </summary>
    /// <param name="enemyHit">Whether we should play audio.</param>
    private void UpdateEnemyGrabbedAudio(bool enemyHit)
    {
        if (enemyHit != _enemyGrabbedAudioPlaying)
        {
            _enemyGrabbedAudioPlaying = enemyHit;
            if (_enemyGrabbedAudioPlaying)
            {
                _enemyGrabbedInstance = AudioManager.Instance.PlaySound(_enemyHarmonization);
            }
            else
            {
                AudioManager.Instance.StopSound(_enemyGrabbedInstance, true);
            }
        }
    }

    /// <summary>
    /// Checks for entities who have exited the beam via comparison of two sets.
    /// </summary>
    private void CheckEntityExits()
    {
        foreach (var harmonyBeamEntity in _prevHitEntities.ToList<IHarmonyBeamEntity>())
        {
            if (harmonyBeamEntity == null) continue;
            if (_hitEntities.Contains(harmonyBeamEntity)) continue;
            //disable any hit enemy vfx here too :)
            harmonyBeamEntity.OnLaserExit();
            if (!_wrappedEnemyFX.TryGetValue(harmonyBeamEntity, out var fx)) continue;
            Destroy(fx.gameObject);
            _wrappedEnemyFX.Remove(harmonyBeamEntity);
        }
    }
}