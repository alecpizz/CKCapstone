/******************************************************************
*    Author: Josephine Qualls
*    Contributors: Josh Eddy, Alec Pizziferro
*    Date Created: 10/10/2024
*    Description: Controls where walls move after switch is triggered.
*******************************************************************/


using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Class that determines how the walls move
/// Also operates the ghost wall indicators
/// </summary>
public class MovingWall : MonoBehaviour, IGridEntry
{
    //original position of wall and ghost
    private Vector3 _originWall;
    private Vector3 _originGhost;

    //used to determine the GridPlacer of specific wall
    [SerializeField] private GridPlacer _wallGrid;

    //indicator for where wall will be moved
    [SerializeField] private GameObject _wallGhost;

    //classes required from Alec's IGridEntry Interface
    public bool IsTransparent => false;

    public GameObject GetGameObject => gameObject;

    public Vector3 Position => transform.position;



    /// <summary>
    /// Original position of the wall is given
    /// And ghost wall is put in it's initial position
    /// </summary>
    void Start()
    {
        _originWall = transform.position;

        _originGhost = _wallGhost.transform.position;
    }

    /// <summary>
    /// Swaps the positions of the wall and the ghost
    /// When switch is turned on
    /// Allows Player to move where wall once was
    /// </summary>
    public void WallIsMoved()
    {
        transform.position = _originGhost;
        _wallGhost.transform.position = _originWall;
        _wallGrid.UpdatePosition();
    }

    /// <summary>
    /// Swaps wall and ghost back to original positions
    /// Now that switch is off
    /// Allows Player to move where wall once was
    /// </summary>
    public void WallMoveBack()
    {
        transform.position = _originWall;
        _wallGhost.transform.position = _originGhost;
        _wallGrid.UpdatePosition();
    }
}
