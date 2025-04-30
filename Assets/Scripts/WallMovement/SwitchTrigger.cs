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
using PrimeTween;
using SaintsField;

/// <summary>
/// Class for actions taken once switch is triggered
/// </summary>
public class SwitchTrigger : MonoBehaviour, IGridEntry
{
    // for registering mechanics to a switch
    [SerializeReference] private List<MovingWall> _affectedWalls = new List<MovingWall>();
    [SerializeReference] private List<ReflectionSwitch> _affectedReflectors = new List<ReflectionSwitch>();
    [SerializeReference] private List<HarmonySwitch> _affectedBeams = new List<HarmonySwitch>();

    [SepTitle("Switch Tweening", EColor.Cyan)]
    [SerializeField] [Required] private Transform _switchMovingObject;
    [SerializeField] private float _switchDepth = 0.2f;
    [SerializeField] private float _switchPressTime = 0.1f;
    [SerializeField] private float _switchDePressTime = 0.1f;
    [SerializeField] private Ease _switchPressEase = Ease.Linear;
    [SerializeField] private Ease _switchDePressEase = Ease.Linear;

    //reference for sound of switch
    [SerializeField] private EventReference _switchSound = default;
    [SerializeField] private string _switchParam = default;
    private float _initialSwitchYPos;
    private ParamRef _param;

    /// <summary>
    /// Positions the switch to be at a height where it doesn't clip into the ground
    /// </summary>
    private void Awake()
    {
        SnapToGridSpace();
        GridBase.Instance.AddEntry(this);
        _initialSwitchYPos = _switchMovingObject.localPosition.y;
        _param = new ParamRef()
        {
            Name = _switchParam,
            Value = 0f
        };
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
            //changes the walls and plays a sound
            for (int i = 0; i < _affectedWalls.Count; i++)
            {
                _affectedWalls[i].SwitchActivation();
            }

            //changes the reflection cubes and plays a sound
            for (int i = 0; i < _affectedReflectors.Count; i++)
            {
                _affectedReflectors[i].SwitchActivation();
            }

            //changes the harmony beams and plays a sound
            for (int i = 0; i < _affectedBeams.Count; i++)
            {
                _affectedBeams[i].SwitchActivation();

            }

            _param.Value = 0f;
            AudioManager.Instance.PlaySound(_switchSound, _param);

            Tween.LocalPositionY(_switchMovingObject, endValue: _initialSwitchYPos - _switchDepth, 
                duration: _switchPressTime,
                ease: _switchPressEase);
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
            Tween.LocalPositionY(_switchMovingObject, endValue: _initialSwitchYPos, duration: _switchDePressTime,
                ease: _switchDePressEase);
            _param.Value = 1f;
            AudioManager.Instance.PlaySound(_switchSound, _param);
        }
    }

    public bool IsTransparent => true;
    public bool BlocksHarmonyBeam => false;
    public bool BlocksMovingWall => true;
    public Vector3 Position => transform.position;
    public GameObject EntryObject => gameObject;

    public void SnapToGridSpace()
    {
        Vector3Int cellPos = GridBase.Instance.WorldToCell(transform.position);
        Vector3 worldPos = GridBase.Instance.CellToWorld(cellPos);
        transform.position = worldPos + CKOffsetsReference.SwitchOffset;
    }
}