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

public class EnemyBehavior : MonoBehaviour, IGridEntry
{
    public bool IsTransparent { get => true; }
    public Vector3 Position { get => transform.position; }
    public GameObject GetGameObject { get => gameObject; }

    private PlayerControls _input;

    [SerializeField] private GameObject player;
    [SerializeField] private int tilesMoved;
    [SerializeField] private Transform start;
    [SerializeField] private Transform destination;

    [SerializeField] private bool atStart;
    [SerializeField] private int currentPoint = 0;

    PlayerMovement playerMoveRef;
    [SerializeField] private float moveRate = 1f;
    [SerializeField] private float waitTime = 1f;

    public List<GameObject> movePoints = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        // Getting player input for the sake of triggering enemy movement.
        // There's 100% a better way to do this but every attempt Mitchell
        // and I made to just get a bool from the player script to signal
        // when it's moving didn't work, so this was just done to get 
        // SOMETHING working
        _input = new PlayerControls();
        _input.InGame.Enable();
        _input.InGame.MoveUp.performed += EnemyMove;
        _input.InGame.MoveDown.performed += EnemyMove;
        _input.InGame.MoveLeft.performed += EnemyMove;
        _input.InGame.MoveRight.performed += EnemyMove;

        playerMoveRef = player.GetComponent<PlayerMovement>();
        GridBase.Instance.AddEntry(this);

        // Make sure enemiess are always seen at the start
        atStart = true;
    }

    /// <summary>
    /// Function that handles the enemy's movement along the provided points in the list
    /// </summary>
    /// <param name="obj"></param>
    public void EnemyMove(InputAction.CallbackContext obj)
    {
        var upMove = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, Vector3.forward);
        var downMove = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, Vector3.back);
        var leftMove = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, Vector3.left);
        var rightMove = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, Vector3.right);

        var nextPos = movePoints[currentPoint].transform.position;
        var currentPos = gameObject.transform.position;
        if (playerMoveRef.enemiesMoved == true)
        {
            if (atStart)
            {
                currentPoint++;
                playerMoveRef.enemiesMoved = false;
                StartCoroutine(MoveOverTime2());

                if (currentPoint == movePoints.Count - 1)
                {
                    atStart = false;
                }
            }
            else
            {
                currentPoint--;
                playerMoveRef.enemiesMoved = false;
                StartCoroutine(MoveOverTime2());

                if (currentPoint == 0)
                {
                    atStart = true;
                }
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

    /// <summary>
    /// Function moves enemy from either current x or z position on the grid to next x or z position on the grid.
    /// </summary>
    /// <returns></returns>

    IEnumerator MoveOverTime2()
    {
        Vector3 startPos = transform.position;

        for (float i = 0; i <= waitTime; i += 0.05f)
        {
            yield return new WaitForSeconds(0.05f);
            transform.position = Vector3.Lerp(startPos, movePoints[currentPoint].transform.position, i / waitTime);
            Debug.Log(i);
        }

        transform.position = movePoints[currentPoint].transform.position;
        playerMoveRef.enemiesMoved = true;
    }
}
