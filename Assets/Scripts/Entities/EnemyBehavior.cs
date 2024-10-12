/******************************************************************
*    Author: Cole Stranczek
*    Contributors: Cole Stranczek, Mitchell Young
*    Date Created: 10/3/24
*    Description: Script that handles the behavior of the enemy,
*    from movement to causing a failstate with the player
*******************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class EnemyBehavior : MonoBehaviour
{
    private PlayerControls _input;

    [SerializeField] private GameObject player;
    PlayerMovement playerMoveRef;
    [SerializeField] private int tilesMoved;
    [SerializeField] private Transform start;
    [SerializeField] private Transform destination;

    [SerializeField] private bool atStart;
    [SerializeField] private int currentPoint = 0;

    private List<GameObject> movePoints = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        _input = new PlayerControls();
        _input.InGame.Enable();
        _input.InGame.MoveUp.performed += EnemyMove;
        _input.InGame.MoveDown.performed += EnemyMove;
        _input.InGame.MoveLeft.performed += EnemyMove;
        _input.InGame.MoveRight.performed += EnemyMove;

        playerMoveRef = player.GetComponent<PlayerMovement>();
        GridBase.Instance.AddEntry(gameObject);

        movePoints.Add(GameObject.Find("StartDestination"));
        movePoints.Add(GameObject.Find("SecondDestination"));
        movePoints.Add(GameObject.Find("ThirdDestination"));

        atStart = true;
    }

    public void EnemyMove(InputAction.CallbackContext obj)
    {
        var upMove = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, Vector3.forward);
        var downMove = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, Vector3.back);
        var leftMove = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, Vector3.left);
        var rightMove = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, Vector3.right);

        var startPos = GridBase.Instance.CellToWorld(GridBase.Instance.WorldToCell(start.transform.position));
        var desPos = GridBase.Instance.CellToWorld(GridBase.Instance.WorldToCell(destination.transform.position));

        if(atStart)
        {
            currentPoint++;
            gameObject.transform.position = movePoints[currentPoint].transform.position;

            if (currentPoint == movePoints.Count - 1)
            {
                atStart = false;
            }
        }
        else
        {
            currentPoint--;
            gameObject.transform.position = movePoints[currentPoint].transform.position;

            if (currentPoint == 0)
            {
                atStart = true;
            }
        }

        //if (GridBase.Instance.CellIsEmpty(rightMove))
        //{
        //    gameObject.transform.position = rightMove;
        //    GridBase.Instance.UpdateEntry(gameObject);

        //    playerMoveRef.playerMoved = false;
        //}
        //else if(GridBase.Instance.CellIsEmpty(downMove))
        //{
        //    gameObject.transform.position = downMove;
        //    GridBase.Instance.UpdateEntry(gameObject);

        //    playerMoveRef.playerMoved = false;
        //}
    }
}
