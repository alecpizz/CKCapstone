using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// using UnityEngine.InputSystem;

namespace Cacophony
{
    /// <summary>
    /// Author: Ryan
    /// Description: Manages anything relating the the player
    /// </summary>
    public class PlayerManager : MonoBehaviour
    {
        [Header("Variables")] [SerializeField] float _moveTime;
        [SerializeField] float _turnTime;
        [SerializeField] float _moveTimeOnRoomSolved;
        private Vector2 _movementDirection;

        [Space] [Header("Components")] [SerializeField]
        HarmonyWave _playerWave;

        [Header("Visuals")] [SerializeField] Animator animator;
        private const string walkAnim = "StartWalk";

        [HideInInspector] public UnityEvent SelectedTileChangeEvent;

        #pragma warning disable
        private Coroutine _playerMovementCoroutine;
        bool isRoomSolved = false;

        private void Start()
        {
            GameplayManagers.Instance.Room.RoomVictoryEvent += OnRoomVictory;
            GameplayManagers.Instance.Room.RoomStartEvent += OnRoomStart;
            //Invoke("PlayerStartingLocation",.2f);
            PlayerStartingLocation();
            //GameplayManagers.Instance.Grid.DetermineGridSpaceFromPosition(transform.position).StartingPositionHighlight();

            GameplayManagers.Instance.Enemy.SetLastPlayerPosition(transform.position);
        }

        private void PlayerStartingLocation()
        {
            if (SaveSceneData.Instance.GetLastSceneDirection() == Vector2.zero) return;
/*        Debug.Log("PlayerStartingLocationAssigned");
        Vector3Int a = GameplayManagers.Instance.Room.FindExitFromDirection(SaveSceneData.Instance.GetLastSceneDirection()).playerSpawnFromEntrance;*/
            ExitLocationData Eld =
                GameplayManagers.Instance.Room.FindExitFromDirection(SaveSceneData.Instance.GetLastSceneDirection());
            GameplayManagers.Instance.Room.SetEntranceExit(Eld);

            transform.position = Eld.playerSpawnFromEntrance;
            GameplayManagers.Instance.Enemy.SetPlayerTile(
                GameplayManagers.Instance.Grid.DetermineGridSpaceFromPosition(transform.position));

            //GameplayManagers.Instance.Grid.DetermineGridSpaceFromPosition(transform.position).StartingPositionHighlight();
        }

        /// <summary>
        /// Takes in an input for the direction to move the player
        /// </summary>
        /// <param name="obj"></param>
        // public void DirectionalInput(InputAction.CallbackContext obj)
        // {
        //     if (GameplayManagers.Instance.Turn.GetTurnState() != TurnState.Player || _playerMovementCoroutine !=null)
        //         return;
        //
        //     if (obj.ReadValue<Vector2>().x != 0 && Mathf.Abs(obj.ReadValue<Vector2>().x) != 1) return;
        //
        //     _movementDirection = obj.ReadValue<Vector2>();
        //     /*if (_movementDirection.x != 0 && _movementDirection.x != 1)
        //         Debug.Log("InvalidMoveDir");
        //     _movementDirection = PreventDiagonalInput(_movementDirection);*/
        //     
        //     
        //     CheckForExit();
        //
        //     if(isRoomSolved && !GameplayManagers.Instance.Room.GetRoomFailed())
        //         _playerMovementCoroutine = StartCoroutine(StartPlayerMovement(_moveTimeOnRoomSolved));
        //
        //     RotatePlayer();
        // }

        /// <summary>
        /// Press a key to confirm the direction you want to move
        /// </summary>
        /// <param name="obj"></param>
        // public void ConfirmDirection(InputAction.CallbackContext obj)
        // {
        //     if (GameplayManagers.Instance.Turn.GetTurnState() != TurnState.Player)
        //         return;
        //
        //     if (!isRoomSolved && !GameplayManagers.Instance.Room.GetRoomFailed())
        //     {
        //         _playerMovementCoroutine = StartCoroutine(StartPlayerMovement(_moveTime));
        //         AudioManager.Instance.PlaySoundEffect("Forward");
        //     }
        //         
        //
        //     StartCoroutine(DelayedHighlightNext());
        // }

        /// <summary>
        /// Moves the player, and after it is complete update the turn state
        /// </summary>
        /// <param name="moveLength"></param>
        /// <returns></returns>
        private IEnumerator StartPlayerMovement(float moveLength)
        {
            StartWalkAnim();
            StartCoroutine(MoveWaveWithPlayer(moveLength));
            GameplayManagers.Instance.Enemy.SetLastPlayerPosition(transform.position);
            if (GameplayManagers.Instance.Mover.MoveCharacter(gameObject, _movementDirection, moveLength))
            {
                yield return new WaitForSeconds(moveLength);
                GameplayManagers.Instance.Enemy.SetPlayerTile(
                    GameplayManagers.Instance.Grid.DetermineGridSpaceFromPosition(transform.position));
                GameplayManagers.Instance.Turn.Progress();
                _playerMovementCoroutine = null;
            }
        }

        /// <summary>
        /// Moves the wave as the player is moving to their next position
        /// </summary>
        /// <param name="moveLength"></param>
        /// <returns></returns>
        private IEnumerator MoveWaveWithPlayer(float moveLength)
        {
            while (moveLength > 0)
            {
                SendHarmonyWaveInMoveDirection();
                moveLength -= Time.deltaTime;
                yield return null;
            }
        }

        /// <summary>
        /// Highlights the player's next tile after their specified move time
        /// </summary>
        /// <returns></returns>
        private IEnumerator DelayedHighlightNext()
        {
            yield return new WaitForSeconds(_moveTime);
            HighlightNextMove();
        }

        /// <summary>
        /// Prevents the player from looking diagonally
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private Vector2 PreventDiagonalInput(Vector2 input)
        {
            int isDiagonal = input.x * input.y != 0 ? 0 : 1;
            return _movementDirection *= isDiagonal;
        }

        /// <summary>
        /// Rotates the player to look in the direction they want to move
        /// </summary>
        private void RotatePlayer()
        {
            if (GameplayManagers.Instance.Room.GetRoomFailed())
                return;
            //Vector3 newDir = transform.position + new Vector3(_movementDirection.x, 0,_movementDirection.y);

            //transform.LookAt(newDir, Vector3.up);
            if (!isRoomSolved)
                AudioManager.Instance.PlaySoundEffect("DirectionSelect");
            StartCoroutine(RotateOverTime(_turnTime, _movementDirection));
            SendHarmonyWaveInMoveDirection();
            HighlightNextMove();

            GameplayManagers.Instance.Harmonizer.TriggerAllWaves();
        }

        private void CheckForExit()
        {
            RaycastHit rayHit;
            if (Physics.Raycast(transform.position, new Vector3(_movementDirection.x, 0, _movementDirection.y)
                    , out rayHit, 1, GameplayManagers.Instance.Room.GetExitLayerMask()))
            {
                rayHit.collider.gameObject.GetComponent<Exit>().UseExit();
            }
        }

        /// <summary>
        /// Highlights the tile the player is facing
        /// </summary>
        private void HighlightNextMove()
        {
            SelectedTileChangeEvent.Invoke();

            if (isRoomSolved)
                return;

            Vector3 newDir = transform.position + new Vector3(_movementDirection.x, 0, _movementDirection.y);
            if (!GameplayManagers.Instance.Grid.DetermineIfOutOfBounds(_movementDirection,
                    GameplayManagers.Instance.Grid.DetermineGridSpaceFromPosition(transform.position))) return;
            GameplayManagers.Instance.Grid.DetermineGridSpaceFromPosition(newDir).Highlight(SelectedTileChangeEvent);
        }

        private IEnumerator RotateOverTime(float rotationTime, Vector2 direction)
        {
            Vector3 targetRotationEuler = new(0,
                Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y)).eulerAngles.y, 0);
            Quaternion targetRotation = Quaternion.Euler(targetRotationEuler);

            float time = 0;
            while (time < 1)
            {
                time += Time.deltaTime / rotationTime;
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, time);
                yield return new WaitForEndOfFrame();
            }
        }

        /// <summary>
        /// Starts the walk animation
        /// </summary>
        private void StartWalkAnim()
        {
            animator.SetTrigger(walkAnim);
        }

        /// <summary>
        /// Sends the harmony wave on the player in the direction you are moving
        /// </summary>
        public void SendHarmonyWaveInMoveDirection()
        {
            _playerWave.ProjectHarmonyWave(new Vector3(_movementDirection.x, 0, _movementDirection.y));
        }

        private void OnRoomVictory()
        {
            isRoomSolved = true;
            //_playerWave.gameObject.SetActive(false);
            GameplayManagers.Instance.Room.RoomVictoryEvent -= OnRoomVictory;
        }

        private void OnRoomStart()
        {
            isRoomSolved = false;
            _playerWave.gameObject.SetActive(true);
            GameplayManagers.Instance.Room.RoomVictoryEvent -= OnRoomStart;
        }
    }
}