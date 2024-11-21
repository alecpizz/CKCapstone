/******************************************************************
*    Author: Josephine Qualls
*    Contributors: Josh Eddy, Alec Pizziferro, Trinity Hutson
*    Date Created: 10/10/2024
*    Description: Controls where walls move after switch is triggered.
*******************************************************************/

using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Class that determines how the walls move
/// Also operates the ghost wall indicators
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

    //to decide if switch should be true or not
    private bool _worked = true;

    //classes required from Alec's IGridEntry Interface
    public bool IsTransparent => false;

    public bool BlocksHarmonyBeam { get => true; }

    public GameObject GetGameObject => gameObject;

    public Vector3 Position => transform.position;

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
    /// Performs the switch of the wall and ghost wall
    /// When the switch is activated
    /// Allows Player to move where wall once was
    /// </summary>
    public void ActivationAction()
    {


        transform.position = new Vector3(_originGhost.x, transform.position.y, _originGhost.z);
        Tween.PositionY(_wallGhost.transform, endValue: 0, duration: 1, ease: Ease.InOutSine).OnComplete(() => _wallGhost.transform.position = _originWall);

        Tween.PositionY(transform, endValue: _originGhost.y, duration: 1, ease: Ease.InOutSine).OnComplete(() => _wallGrid.UpdatePosition());
    }

    /// <summary>
    /// Performs the switch of the wall and ghost wall
    /// When the switch is deactivated
    /// Allows Player to move where wall once was
    /// </summary>
    public void DeactivationAction()
    {
        transform.position = new Vector3(_originWall.x, transform.position.y, _originWall.z);
        Tween.PositionY(_wallGhost.transform, endValue: 0, duration: 1, ease: Ease.InOutSine).OnComplete(() => _wallGhost.transform.position = _originGhost);

        Tween.PositionY(transform, endValue: _originWall.y, duration: 1, ease: Ease.InOutSine).OnComplete(() => _wallGrid.UpdatePosition());
    }

    /// <summary>
    /// Performs an animation that lifts the wall and drops it where the ghost wall was
    /// Calls ActivationAction() to swap the wall and ghost wall positions
    /// Only works if there is nothing obstucting the ghost wall's tile
    /// </summary>
    public void SwitchActivation()
    {
        if (GridBase.Instance.CellIsTransparent(_originGhost))
        {
            _worked = true;

            Tween.PositionY(transform, endValue: 10, duration: 1, ease: Ease.InOutSine).OnComplete(() => ActivationAction());

        }
        else
        {
            _worked = false;
        }        
    }

    /// <summary>
    /// Performs an animation that lifts the wall and drops it where the ghost wall was
    /// Calls DeactivationAction() to swap the wall and ghost wall positions
    /// Only works if there is nothing obstucting the ghost wall's tile
    /// </summary>
    public void SwitchDeactivation()
    {
        if (GridBase.Instance.CellIsTransparent(_originWall))
        {
            _worked = true;

            Tween.PositionY(transform, endValue: 10, duration: 1, ease: Ease.InOutSine).OnComplete(() => DeactivationAction());

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
