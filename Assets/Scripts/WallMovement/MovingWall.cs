/******************************************************************
*    Author: Josephine Qualls
*    Contributors: Josh Eddy, Alec Pizziferro, Trinity Hutson
*    Date Created: 10/10/2024
*    Description: Controls what walls sink and rise after switch is triggered.
*******************************************************************/

using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEngine;

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

    public GameObject GetGameObject => gameObject;

    public Vector3 Position => transform.position;

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
    /// Only works if there is nothing obstucting the transparent wall's tile
    /// </summary>
    public void SwitchActivation()
    {
        if (GridBase.Instance.CellIsTransparent(_originGhost))
        {
            _worked = true;

            Tween.PositionY(transform, endValue: _groundHeight, duration: _duration, ease: _easeType);
            Tween.PositionY(_wallGhost.transform, endValue: _activatedHeight, duration: _duration, ease: _easeType);

            bool wallGridActive = false;

            _wallGrid.IsTransparent = !wallGridActive;
            _ghostPlacer.IsTransparent = wallGridActive;

            _wallGrid.BlocksHarmonyBeam = wallGridActive;
            _ghostPlacer.BlocksHarmonyBeam = !wallGridActive;

            _wallCollider.enabled = wallGridActive;
            _ghostCollider.enabled = !wallGridActive;

        }
        else
        {
            _worked = false;
        }        
    }

    /// <summary>
    /// Performs an animation that sinks the ghost wall and raises the wall
    /// Only works if there is nothing obstucting the transparent wall's tile
    /// </summary>
    public void SwitchDeactivation()
    {
        if (GridBase.Instance.CellIsTransparent(_originWall))
        {
            _worked = true;

            Tween.PositionY(transform, endValue: _activatedHeight, duration: _duration, ease: _easeType);
            Tween.PositionY(_wallGhost.transform, endValue: _groundHeight, duration: _duration, ease: _easeType);

            bool wallGridActive = false;

            _wallGrid.IsTransparent = wallGridActive;
            _ghostPlacer.IsTransparent = !wallGridActive;

            _wallGrid.BlocksHarmonyBeam = !wallGridActive;
            _ghostPlacer.BlocksHarmonyBeam = wallGridActive;

            _wallCollider.enabled = !wallGridActive;
            _ghostCollider.enabled = wallGridActive;

        }
        else
        {
            _worked = false;
        }
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
}
