/******************************************************************
*    Author: Josephine Qualls
*    Contributors: Josh Eddy, Alec Pizziferro, Trinity Hutson
*    Date Created: 10/10/2024
*    Description: Controls where walls move after switch is triggered.
*******************************************************************/


//using PrimeTween;
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

    //the offset for the tween animation
    [SerializeField] private Vector3 _positionOffset;

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
        _originWall = transform.position;
        // Maintains same height to ensure consistency when swapping
        _originWall.y = _wallGhost.transform.position.y;

        _originGhost = _wallGhost.transform.position;
        // Maintains same height to ensure consistency when swapping
        _originGhost.y = transform.position.y;
    }

    /// <summary>
    /// Swaps the positions of the wall and the ghost
    /// When switch is turned on
    /// Allows Player to move where wall once was
    /// </summary>
    public void SwitchActivation()
    {
        if (GridBase.Instance.CellIsTransparent(_originGhost))
        {
            transform.position = _originGhost;
            _wallGhost.transform.position = _originWall;
            //yield return Tween.PositionY(transform, _originWall.y + _positionOffset.y, duration: 0.5f, Ease.OutBack).ToYieldInstruction();
            _wallGrid.UpdatePosition();
            
            _worked = true;
        }
        else
        {
            _worked = false;
        }
    }

    /// <summary>
    /// Swaps wall and ghost back to original positions
    /// Now that switch is off
    /// Allows Player to move where wall once was
    /// </summary>
    public void SwitchDeactivation()
    {
        if (GridBase.Instance.CellIsTransparent(_originWall)) 
        {
            transform.position = _originWall;
            _wallGhost.transform.position = _originGhost;
            //yield return Tween.Position(transform, _originGhost + _positionOffset, duration: 0.5f, Ease.OutBack).ToYieldInstruction();
            _wallGrid.UpdatePosition();

            _worked = true;

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
}
