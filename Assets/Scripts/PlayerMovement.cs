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
using UnityEngine.Windows;

public class PlayerMovement : MonoBehaviour
{
    private PlayerControls _input;

    [SerializeField] private GameObject _upPointer;
    [SerializeField] private GameObject _downPointer;
    [SerializeField] private GameObject _leftPointer;
    [SerializeField] private GameObject _rightPointer;

    // Start is called before the first frame update
    void Start()
    {
        // Referenceing and setup of the Input Action functions
        _input = new PlayerControls();
        _input.InGame.Enable();
        _input.InGame.MoveUp.performed += MoveUp_performed;
        _input.InGame.MoveDown.performed += MoveDown_performed;
        _input.InGame.MoveLeft.performed += MoveLeft_performed;
        _input.InGame.MoveRight.performed += MoveRight_performed;

        // Get references to the directional pointers for later use
        _upPointer = GameObject.Find("Up Pointer");
        _downPointer = GameObject.Find("Down Pointer");
        _leftPointer = GameObject.Find("Left Pointer");
        _rightPointer = GameObject.Find("Right Pointer");

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
    private void MoveDown_performed(InputAction.CallbackContext obj)
    {
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
                if (GridBase.Instance.CellIsEmpty(downMove))
                {
                    gameObject.transform.position = downMove;
                }
                _downPointer.SetActive(false);
            }
    }

    /// <summary>
    /// Handles the upward movement of the player when the respective control
    /// binding is triggered
    /// </summary>
    /// <param name="obj"></param>
    private void MoveUp_performed(InputAction.CallbackContext obj)
    {
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
                if (GridBase.Instance.CellIsEmpty(upMove))
                {
                    gameObject.transform.position = upMove;
                }
                _upPointer.SetActive(false);
            }
    }

    /// <summary>
    /// Handles the leftward movement of the player when the respective control
    /// binding is triggered
    /// </summary>
    /// <param name="obj"></param>
    private void MoveLeft_performed(InputAction.CallbackContext obj)
    {
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
                if (GridBase.Instance.CellIsEmpty(leftMove))
                {
                    gameObject.transform.position = leftMove;
                }
                _leftPointer.SetActive(false);
            }
    }

    /// <summary>
    /// Handles the rightward movement of the player when the respective control
    /// binding is triggered
    /// </summary>
    /// <param name="obj"></param>
    private void MoveRight_performed(InputAction.CallbackContext obj)
    {
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
                if(GridBase.Instance.CellIsEmpty(rightMove))
                {
                    gameObject.transform.position = rightMove;
                }
                _rightPointer.SetActive(false);
            }
    }

    private void Update()
    {
        
    }
}
