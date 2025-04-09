/******************************************************************
*    Author: Josephine Qualls
*    Contributors: Josh Eddy, Alec Pizziferro, Nick Grinstead
*    Date Created: 10/10/2024
*    Description: Switch that moves mechanics.
*    Only moves registered mechanics.
*******************************************************************/


using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using FMODUnity;
using FMOD.Studio;

/// <summary>
/// Class for actions taken once switch is triggered
/// </summary>
public class SwitchTrigger : MonoBehaviour, IGridEntry
{
    // for registering mechanics to a switch
    [SerializeReference] private List<MovingWall> _affectedWalls = new List<MovingWall>();
    [SerializeReference] private List<ReflectionSwitch> _affectedReflectors = new List<ReflectionSwitch>();
    [SerializeReference] private List<HarmonySwitch> _affectedBeams = new List<HarmonySwitch>();

    // Reference to Animator component
    [SerializeReference] private Animator _animator;

    //reference for sound of switch
    [SerializeField] private EventReference _switchSound = default;

    /// <summary>
    /// Positions the switch to be at a height where it doesn't clip into the ground
    /// </summary>
    private void Awake()
    {
        SnapToGridSpace();
    }
    
    /// <summary>
    /// For when the switch desyncs and the button is still pressed inwards
    /// </summary>
    /// <param name="other">the should be non-existent collider</param>
    private void OnTriggerStay(Collider other)
    {
        if (_animator.GetBool("Pressed") && other == null)
        {
            _animator.SetTrigger("Pressed");
        }
    }
    
    /// <summary>
    /// Turns the switch on/off every time the Player steps on it
    /// Moves walls when switch is on and back when it's off
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("SonEnemy") || other.CompareTag("Enemy"))
        {
            if (!_animator.GetBool("Pressed"))
            {
                //changes the walls and plays a sound
                for (int i = 0; i < _affectedWalls.Count; i++)
                {
                    _affectedWalls[i].SwitchActivation();

                    AudioManager.Instance.PlaySound(_switchSound);
                }
            }
            //changes the reflection cubes and plays a sound
            for (int i = 0; i < _affectedReflectors.Count; i++)
            {
                _affectedReflectors[i].SwitchActivation();

                AudioManager.Instance.PlaySound(_switchSound);
            }

            //changes the harmony beams and plays a sound
            for (int i = 0; i < _affectedBeams.Count; i++)
            {
                _affectedBeams[i].SwitchActivation();

                AudioManager.Instance.PlaySound(_switchSound);
            }

            if (_animator != null)
            {
                _animator.SetTrigger("Pressed");
            }
        }
    }

    /// <summary>
    /// Visually raises the pressure plate when the player steps off
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("SonEnemy") || other.CompareTag("Enemy"))
        {
            if (_animator != null)
            {
                _animator.SetTrigger("Pressed");
            }
        }
    }

    public bool IsTransparent => true;
    public bool BlocksHarmonyBeam => false;
    public Vector3 Position => transform.position;
    public GameObject EntryObject => gameObject;
    public void SnapToGridSpace()
    {
        Vector3Int cellPos = GridBase.Instance.WorldToCell(transform.position);
        Vector3 worldPos = GridBase.Instance.CellToWorld(cellPos);
        transform.position = worldPos + CKOffsetsReference.SwitchOffset;
    }
}


