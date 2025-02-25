/******************************************************************
*    Author: Josephine Qualls
*    Contributors: Josh Eddy, Alec Pizziferro, Trinity Hutson, Nick Grinstead
*    Date Created: 10/10/2024
*    Description: Controls what walls sink and rise after switch is triggered.
*******************************************************************/

using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEngine;
using SaintsField.Playa;

/// <summary>
/// Class that determines how the walls and ghost walls move
/// Inherits from IParentSwitch and IGridEntry
/// </summary>
public class MovingWall : MonoBehaviour, IParentSwitch, IGridEntry
{
    //original position of wall and ghost
    private Vector3 _originWall;
    private Vector3 _originGhost;

    //used to determine the GridPlacer of specific wall
    [SerializeField] private GridPlacer _wallGrid;

    //indicator for where wall will be moved
    [SerializeField] private GameObject _wallGhost;

    //height the walls will sink to
    [SerializeField] private float _groundHeight;

    //height the walls will rise to
    [SerializeField] private float _activatedHeight;

    //time it takes for tween to finish
    [PlayaInfoBox("Time it takes for the tween between positions to finish.")]
    [SerializeField] private float _duration;

    //type of tween animation for walls
    [SerializeField] private Ease _easeType;

    //to decide if switch should be true or not
    private bool _worked = true;

    //collider for the wall
    private Collider _wallCollider;

    //collider for the ghost wall
    private Collider _ghostCollider;

    //wall ghost grid placer reference
    private GridPlacer _ghostPlacer;

    //classes required from Alec's IGridEntry Interface
    public bool IsTransparent => false;

    public bool BlocksHarmonyBeam { get => true; }

    public GameObject EntryObject => gameObject;

    public Vector3 Position => transform.position;

    private bool _shouldActivate = false;

    /// <summary>
    /// References the GridPlacer on the wall ghost
    /// </summary>
    private void Awake()
    {
        _ghostPlacer = _wallGhost.GetComponent<GridPlacer>(); 
        _wallCollider = GetComponent<Collider>();
        _ghostCollider = _wallGhost.GetComponent<Collider>();
    }

    /// <summary>
    /// Original position of the wall is given
    /// And ghost wall is put in it's initial position
    /// </summary>
    void Start()
    {
        SnapToGridSpace();

        _originWall = transform.position;
        // Maintains same height to ensure consistency when swapping
        _originWall.y = _wallGhost.transform.position.y;

        _originGhost = _wallGhost.transform.position;
        // Maintains same height to ensure consistency when swapping
        _originGhost.y = transform.position.y;
    }


    /// <summary>
    /// Performs an animation that sinks the wall and raises the ghost wall
    /// Only works if there is nothing obstructing the transparent wall's tile
    /// </summary>
    public void SwitchActivation()
    {
        MoveObject();
        _shouldActivate = !_shouldActivate;
    }

    /// <summary>
    /// Getter for _worked variable
    /// </summary>
    /// <returns></returns>
    public bool GetWorked()
    {
        return _worked;
    }

    /// <summary>
    /// Places this object in the center of its grid cell
    /// </summary>
    public void SnapToGridSpace()
    {
        Vector3Int cellPos = GridBase.Instance.WorldToCell(transform.position);
        Vector3 worldPos = GridBase.Instance.CellToWorld(cellPos);
        transform.position = new Vector3(worldPos.x, transform.position.y, worldPos.z);
    }

    /// <summary>
    /// Invoked to start the wall's turn. Will only move if it's switch was pressed.
    /// </summary>
    /// <param name="direction">Direction of player movement</param>
    public void MoveObject()
    {
        MoveWall();
    }

    /// <summary>
    /// Helper method to move the wall to its active or inactive state
    /// </summary>
    private void MoveWall()
    {
        // Check for object blocking the wall's target space
        if (_shouldActivate ? GridBase.Instance.CellIsTransparent(_originGhost) :
            GridBase.Instance.CellIsTransparent(_originWall))
        {
            _worked = true;

            if (_shouldActivate)
            {
                Tween.PositionY(transform, endValue: _groundHeight, 
                    duration: _duration, ease: _easeType).Group(
                    Tween.PositionY(_wallGhost.transform, endValue: _activatedHeight, 
                    duration: _duration, ease: _easeType)).OnComplete(TriggerHarmonyScan);
            }
            else
            {
                Tween.PositionY(transform, endValue: _activatedHeight, 
                    duration: _duration, ease: _easeType).Group(
                    Tween.PositionY(_wallGhost.transform, endValue: _groundHeight, 
                    duration: _duration, ease: _easeType)).OnComplete(TriggerHarmonyScan);
            }

            _wallGrid.IsTransparent = _shouldActivate;
            _ghostPlacer.IsTransparent = !_shouldActivate;

            _wallGrid.BlocksHarmonyBeam = !_shouldActivate;
            _ghostPlacer.BlocksHarmonyBeam = _shouldActivate;

            _wallCollider.enabled = !_shouldActivate;
            _ghostCollider.enabled = _shouldActivate;
        }
        else
        {
            TriggerHarmonyScan();
            _worked = false;
        }
    }

    /// <summary>
    /// Completes this object's turn and swaps it to a new turn
    /// </summary>
    private void TriggerHarmonyScan()
    {
        HarmonyBeam.TriggerHarmonyScan?.Invoke();
    }
}
