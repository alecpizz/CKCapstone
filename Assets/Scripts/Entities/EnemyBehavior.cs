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

public class EnemyBehavior : MonoBehaviour
{
    [SerializeField] private GameObject player;
    PlayerMovement playerMoveRef;
    [SerializeField] private int tilesMoved;
    [SerializeField] private Transform start;
    [SerializeField] private Transform destination;


    // Start is called before the first frame update
    void Start()
    {
        playerMoveRef = player.GetComponent<PlayerMovement>();
        GridBase.Instance.AddEntry(gameObject);
    }

    private void Update()
    {
        if(playerMoveRef.playerMoved)
        {
            for(int i = 0; i < tilesMoved; i++)
            {
                EnemyMove();
            }
        }
    }

    public void EnemyMove()
    {
        var upMove = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, Vector3.forward);
        var downMove = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, Vector3.back);
        var leftMove = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, Vector3.left);
        var rightMove = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, Vector3.right);

        var startPos = GridBase.Instance.CellToWorld(GridBase.Instance.WorldToCell(start.transform.position));
        var desPos = GridBase.Instance.CellToWorld(GridBase.Instance.WorldToCell(destination.transform.position));
        gameObject.transform.position = desPos;

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
