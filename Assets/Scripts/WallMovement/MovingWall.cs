/******************************************************************
*    Author: Josephine Qualls
*    Contributors: Josh Eddy
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
    //original position of wall
    private Vector3 _originTransform;

    //used to give coordinates for new wall position
    [SerializeField] private Vector3 _wallTransform;

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
    /// And wall indicator is put in it's initial position
    /// </summary>
    void Start()
    {
        _originTransform = transform.position;

        _wallGhost.transform.position = _wallTransform;
    }

    /// <summary>
    /// Moves the wall to the new registered position
    /// Moves indicator to where the wall was before
    /// Allows Player to move where wall once was
    /// </summary>
    public void WallIsMoved()
    {
        transform.position = _wallTransform;
        _wallGhost.transform.position = _originTransform;
        _wallGrid.UpdatePosition();
    }

    /// <summary>
    /// Moves wall back to its first position
    /// Moves indicator to where wall can move to
    /// Allows Player to move where wall once was
    /// </summary>
    public void WallMoveBack()
    {
        transform.position = _originTransform;
        _wallGhost.transform.position = _wallTransform;
        _wallGrid.UpdatePosition();
    }
}
