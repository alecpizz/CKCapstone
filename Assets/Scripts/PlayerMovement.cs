/******************************************************************
*    Author: Cole Stranczek
*    Contributors: Cole Stranczek, Nick Grinstead
*    Date Created: 9/22/24
*    Description: Script that handles the player's movement along
*    the grid
*******************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerMovement : MonoBehaviour, IGridEntry
{
    public Vector3 FacingDirection { get; private set; }
    
    private PlayerControls _input;

    [SerializeField] private GameObject _upPointer;
    [SerializeField] private GameObject _downPointer;
    [SerializeField] private GameObject _leftPointer;
    [SerializeField] private GameObject _rightPointer;

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

        // Gets rid of all the pointers by default
        _upPointer.SetActive(false);
        _downPointer.SetActive(false);
        _leftPointer.SetActive(false);
        _rightPointer.SetActive(false);

    }

    /// <summary>
    /// Handles the downward movement of the player when the respective control
    /// binding is triggered
    /// </summary>
    /// <param name="obj"></param>
    private void MoveDownPerformed(InputAction.CallbackContext obj)
    {
        FacingDirection = Vector3.back;

        // Sets the Down directional pointer if this is the first button
        // press
        if (!_downPointer.activeInHierarchy)
        {
            _downPointer.SetActive(true);

            _upPointer.SetActive(false);
            _leftPointer.SetActive(false);
            _rightPointer.SetActive(false);
        }
        else
        {
            // Move down and remove the pointer
            var downMove = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, Vector3.back);
            if (GridBase.Instance.CellIsTransparent(downMove) || DebugMenuManager.instance.ghostMode)
            {
                gameObject.transform.position = downMove;
                GridBase.Instance.UpdateEntry(this);
            }
            _downPointer.SetActive(false);
        }
    }

    /// <summary>
    /// Handles the upward movement of the player when the respective control
    /// binding is triggered
    /// </summary>
    /// <param name="obj"></param>
    private void MoveUpPerformed(InputAction.CallbackContext obj)
    {
        FacingDirection = Vector3.forward;

        // Sets the Up directional pointer if this is the first button
        // press
        if (!_upPointer.activeInHierarchy)
        {
            _upPointer.SetActive(true);

            _downPointer.SetActive(false);
            _leftPointer.SetActive(false);
            _rightPointer.SetActive(false);
        }
        else
        {
            // Move up and remove the pointer
            var upMove = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, Vector3.forward);
            if (GridBase.Instance.CellIsTransparent(upMove) || DebugMenuManager.instance.ghostMode)
            {
                gameObject.transform.position = upMove;
                GridBase.Instance.UpdateEntry(this);
            }
            _upPointer.SetActive(false);
        }
    }

    /// <summary>
    /// Handles the leftward movement of the player when the respective control
    /// binding is triggered
    /// </summary>
    /// <param name="obj"></param>
    private void MoveLeftPerformed(InputAction.CallbackContext obj)
    {
        FacingDirection = Vector3.left;

        // Sets the Left directional pointer if this is the first button
        // press
        if (!_leftPointer.activeInHierarchy)
        {
            _leftPointer.SetActive(true);

            _upPointer.SetActive(false);
            _downPointer.SetActive(false);
            _rightPointer.SetActive(false);
        }
        else
        {
            // Move left and remove the pointer
            var leftMove = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, Vector3.left);
            if (GridBase.Instance.CellIsTransparent(leftMove) || DebugMenuManager.instance.ghostMode)
            {
                gameObject.transform.position = leftMove;
                GridBase.Instance.UpdateEntry(this);
            }
            _leftPointer.SetActive(false);
        }
    }

    /// <summary>
    /// Handles the rightward movement of the player when the respective control
    /// binding is triggered
    /// </summary>
    /// <param name="obj"></param>
    private void MoveRightPerformed(InputAction.CallbackContext obj)
    {
        FacingDirection = Vector3.right;

        // Sets the Right directional pointer if this is the first button
        // press
        if (!_rightPointer.activeInHierarchy)
        {
            _rightPointer.SetActive(true);

            _upPointer.SetActive(false);
            _downPointer.SetActive(false);
            _leftPointer.SetActive(false);
        }
        else
        {
            // Move Right and remove the pointer
            var rightMove = GridBase.Instance.GetCellPositionInDirection(gameObject.transform.position, Vector3.right);
            if(GridBase.Instance.CellIsTransparent(rightMove) || DebugMenuManager.instance.ghostMode)
            {
                gameObject.transform.position = rightMove;
                GridBase.Instance.UpdateEntry(this);   
            }
            _rightPointer.SetActive(false);
        }
    }

    public bool IsTransparent { get => true; }
    public Vector3 Position { get => transform.position; }
    public GameObject GetGameObject { get => gameObject; }
}
