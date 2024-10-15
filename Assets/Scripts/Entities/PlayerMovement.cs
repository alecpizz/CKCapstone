/******************************************************************
*    Author: Cole Stranczek
*    Contributors: Cole Stranczek, Nick Grinstead, Alex Laubenstein, Trinity Hutson
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

public class PlayerMovement : MonoBehaviour, IGridEntry
{


    private PlayerControls _input;
    public Vector3 FacingDirection { get; private set; }

    public bool playerMoved;
    public bool enemiesMoved = true;
    public bool IsTransparent { get => true; }
    public Vector3 Position { get => transform.position; }
    public GameObject GetGameObject { get => gameObject; }

    [SerializeField]
    private Vector3 _positionOffset;

    // Start is called before the first frame update
    void Start()
    {
        FacingDirection = new Vector3(0, 0, 0);

        GridBase.Instance.AddEntry(this);

        // Referencing and setup of the Input Action functions
        _input = new PlayerControls();
        _input.InGame.Enable();
        _input.InGame.MoveUp.performed += MoveUpPerformed;
        _input.InGame.MoveDown.performed += MoveDownPerformed;
        _input.InGame.MoveLeft.performed += MoveLeftPerformed;
        _input.InGame.MoveRight.performed += MoveRightPerformed;
    }

    /// <summary>
    /// Unregistering from input actions
    /// </summary>
    private void OnDisable()
    {
        _input.InGame.Disable();
        _input.InGame.MoveUp.performed -= MoveUpPerformed;
        _input.InGame.MoveDown.performed -= MoveDownPerformed;
        _input.InGame.MoveLeft.performed -= MoveLeftPerformed;
        _input.InGame.MoveRight.performed -= MoveRightPerformed;
    }

    /// <summary>
    /// Handles the downward movement of the player when the respective control
    /// binding is triggered
    /// </summary>
    /// <param name="obj"></param>
    public void MoveDownPerformed(InputAction.CallbackContext obj)
    {
        FacingDirection = Vector3.back;

        // Move down if there is no wall below the player or if ghost mode is enabled
        var downMove = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, Vector3.back);
        if ((GridBase.Instance.CellIsEmpty(downMove) && enemiesMoved == true) || 
            (DebugMenuManager.instance.ghostMode && enemiesMoved == true))
        {
            gameObject.transform.position = downMove + _positionOffset;
            GridBase.Instance.UpdateEntry(this);
        }
        else
            Debug.Log(enemiesMoved);
        Debug.Log("IS empty: " + GridBase.Instance.CellIsEmpty(downMove));
    }

    /// <summary>
    /// Handles the upward movement of the player when the respective control
    /// binding is triggered
    /// </summary>
    /// <param name="obj"></param>
    private void MoveUpPerformed(InputAction.CallbackContext obj)
    {
        FacingDirection = Vector3.forward;

        // Move up if there is no wall above the player or if ghost mode is enabled
        var upMove = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, Vector3.forward);
        if ((GridBase.Instance.CellIsEmpty(upMove) && enemiesMoved == true) || 
            (DebugMenuManager.instance.ghostMode && enemiesMoved == true))
        {
            gameObject.transform.position = upMove + _positionOffset;
            GridBase.Instance.UpdateEntry(this);
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
        FacingDirection = Vector3.left;

        // Move left if there is no wall to the left of the player or if ghost mode is enabled
        var leftMove = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, Vector3.left);
        if ((GridBase.Instance.CellIsEmpty(leftMove) && enemiesMoved == true) || 
            (DebugMenuManager.instance.ghostMode && enemiesMoved == true))
        {
           gameObject.transform.position = leftMove + _positionOffset;
           GridBase.Instance.UpdateEntry(this);
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
        FacingDirection = Vector3.right;

        // Move Right if there is no wall to the right of the player or if ghost mode is enabled
        var rightMove = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, Vector3.right);
        if((GridBase.Instance.CellIsEmpty(rightMove) && enemiesMoved == true) || 
            (DebugMenuManager.instance.ghostMode && enemiesMoved == true))
        {
           gameObject.transform.position = rightMove + _positionOffset;
           GridBase.Instance.UpdateEntry(this);   
        }

        playerMoved = true; 
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Checks if the enemy is frozen; if they are, doesn't reload the scene
            EnemyBehavior enemy = collision.collider.GetComponent<EnemyBehavior>();
            if (enemy == null || enemy.enemyFrozen)
                return;

            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }
    }
}
