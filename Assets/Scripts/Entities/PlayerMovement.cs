/******************************************************************
*    Author: Cole Stranczek
*    Contributors: Cole Stranczek
*    Date Created: 9/22/24
*    Description: Script that handles the player's movement along
*    the grid
*******************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;

public class PlayerMovement : MonoBehaviour
{
    private PlayerControls _input;

    public bool playerMoved;

    // Start is called before the first frame update
    void Start()
    {
        GridBase.Instance.AddEntry(gameObject);

        // Referencing and setup of the Input Action functions
        _input = new PlayerControls();
        _input.InGame.Enable();
        _input.InGame.MoveUp.performed += MoveUpPerformed;
        _input.InGame.MoveDown.performed += MoveDownPerformed;
        _input.InGame.MoveLeft.performed += MoveLeftPerformed;
        _input.InGame.MoveRight.performed += MoveRightPerformed;

    }

    /// <summary>
    /// Handles the downward movement of the player when the respective control
    /// binding is triggered
    /// </summary>
    /// <param name="obj"></param>
    public void MoveDownPerformed(InputAction.CallbackContext obj)
    {
        
        // Move down and remove the pointer
        var downMove = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, Vector3.back);
        if (GridBase.Instance.CellIsEmpty(downMove))
        {
            gameObject.transform.position = downMove;
            GridBase.Instance.UpdateEntry(gameObject);
        }
    }

    /// <summary>
    /// Handles the upward movement of the player when the respective control
    /// binding is triggered
    /// </summary>
    /// <param name="obj"></param>
    private void MoveUpPerformed(InputAction.CallbackContext obj)
    {
        // Move up and remove the pointer
        var upMove = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, Vector3.forward);
        if (GridBase.Instance.CellIsEmpty(upMove))
        {
            gameObject.transform.position = upMove;
            GridBase.Instance.UpdateEntry(gameObject);
        }
        playerMoved = true;
        
    }

    /// <summary>
    /// Handles the leftward movement of the player when the respective control
    /// binding is triggered
    /// </summary>
    /// <param name="obj"></param>
    private void MoveLeftPerformed(InputAction.CallbackContext obj)
    {
        // Move left and remove the pointer
        var leftMove = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, Vector3.left);
        if (GridBase.Instance.CellIsEmpty(leftMove))
        {
           gameObject.transform.position = leftMove;
           GridBase.Instance.UpdateEntry(gameObject);
        }

        playerMoved = true;
    }

    /// <summary>
    /// Handles the rightward movement of the player when the respective control
    /// binding is triggered
    /// </summary>
    /// <param name="obj"></param>
    private void MoveRightPerformed(InputAction.CallbackContext obj)
    {
        // Move Right and remove the pointer
        var rightMove = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, Vector3.right);
        if(GridBase.Instance.CellIsEmpty(rightMove))
        {
           gameObject.transform.position = rightMove;
           GridBase.Instance.UpdateEntry(gameObject);   
        }

        playerMoved = true; 
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }
    }
}
