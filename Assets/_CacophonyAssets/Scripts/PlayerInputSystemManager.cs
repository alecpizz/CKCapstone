// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
//
// /// <summary>
// /// Author: Ryan
// /// Description: Handles the input system
// /// </summary>
// public class PlayerInputSystemManager : MonoBehaviour
// {
//     [SerializeField] PlayerManager _pm;
//     // private PlayerActions _input;
//
//     private void Start()
//     {
//         //Creates the input system
//         _input = new PlayerActions();
//
//         //Enables the input system and subscribes movement
//         _input.PlayerMovement.Enable();
//         _input.PlayerMovement.Move.performed += _pm.DirectionalInput;
//         _input.PlayerMovement.ConfirmDirection.started += _pm.ConfirmDirection;
//     }
//
//
//     private void OnDisable()
//     {
//         //Unsubscribes movement and disables input system
//         _input.PlayerMovement.Move.performed -= _pm.DirectionalInput;
//         _input.PlayerMovement.ConfirmDirection.started -= _pm.ConfirmDirection;
//         _input.PlayerMovement.Disable();
//     }
// }
