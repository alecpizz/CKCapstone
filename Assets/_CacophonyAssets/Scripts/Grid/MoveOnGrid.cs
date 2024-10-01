using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: Ryan
/// Description: Moves a character along the grid
/// </summary>
namespace Cacophony
{
    public class MoveOnGrid : MonoBehaviour
    {
        [SerializeField] float rotationTimeModifier = 0.5f;

        /// <summary>
        /// Moves a character on the grid given the object to move, the direction, and the time to move
        /// </summary>
        /// <param name="objectToMove"></param>
        /// <param name="directionToMove"></param>
        /// <param name="moveTime"></param>
        public bool MoveCharacter(GameObject objectToMove, Vector2 directionToMove, float moveTime)
        {
            GridStats toGrid;
            GridStats standingSpace;
            if (CheckNewLocationValidity(objectToMove.transform.position, directionToMove, out standingSpace,
                    out toGrid))
            {
                //Starts moving the character
                StartCoroutine(MoveCharacterOverTime(objectToMove, toGrid, moveTime));

                if (!objectToMove.CompareTag("Player") && !objectToMove.CompareTag("Enemy"))
                    AssignTileOccupation(standingSpace, toGrid);

                //Debug.Log(objectToMove.name + " to -> x: " + toGrid.GetXPos() + " y: " + toGrid.GetYPos());
                return true;
            }

            return false;
        }

        public bool MoveCharacter(GameObject objectToMove, GridStats toGrid, float moveTime)
        {
            //Finds the space you are standing on
            GridStats standingSpace =
                GameplayManagers.Instance.Grid.DetermineGridSpaceFromPosition(objectToMove.transform.position);
            Vector3 moveDirection = (toGrid.transform.position - standingSpace.transform.position).normalized;
            moveDirection = new Vector2(moveDirection.x, moveDirection.z);

            if (CheckNewLocationValidity(objectToMove.transform.position, moveDirection, out standingSpace, out toGrid))
            {
                //Starts moving the character
                StartCoroutine(MoveCharacterOverTime(objectToMove, toGrid, moveTime));

                if (!objectToMove.CompareTag("Player") && !objectToMove.CompareTag("Enemy"))
                    AssignTileOccupation(standingSpace, toGrid);

                //Debug.Log(objectToMove.name + " to -> x: " + toGrid.GetXPos() + " y: " + toGrid.GetYPos());
                return true;
            }

            return false;
        }

        public bool CheckNewLocationValidity(Vector3 startpos, Vector2 directionToMove, out GridStats standingSpace,
            out GridStats toGrid)
        {
            standingSpace = GameplayManagers.Instance.Grid.DetermineGridSpaceFromPosition(startpos);
            if (standingSpace != null)
            {
                //Determines if you can move in the direction
                if (GameplayManagers.Instance.Grid.DetermineValidMovementDirection(directionToMove, standingSpace))
                {
                    //Finds the grid to move to
                    toGrid = GameplayManagers.Instance.Grid.FindGridInDirection(directionToMove, standingSpace);
                    if (toGrid != null)
                    {
                        //Verifies that there isn't something at the location
                        if (!GameplayManagers.Instance.Grid.CheckForObjectAtGrid(toGrid))
                        {
                            return true;
                        }
                    }
                }
            }

            toGrid = null;
            return false;
        }

        private void AssignTileOccupation(GridStats oldTile, GridStats newTile)
        {
            oldTile.SetGridAvailability(GridAvailability.Empty);
            newTile.SetGridAvailability(GridAvailability.Occupied);
        }

        private IEnumerator MoveCharacterOverTime(GameObject objectToMove, GridStats gridToMoveTo, float moveTime)
        {
            AbstractEnvironmentalObject environmentalObject = objectToMove.GetComponent<AbstractEnvironmentalObject>();
            if (environmentalObject != null)
                environmentalObject.AssignAllTilesBeneath(GridAvailability.Empty);

            //Determines the position the character is moving to
            Vector3 gridPosY = new Vector3(gridToMoveTo.gameObject.transform.position.x,
                objectToMove.transform.position.y, gridToMoveTo.gameObject.transform.position.z);
            //Vector3 direction = gridPosY - new Vector3(transform.position.x, 0, transform.position.z).normalized;

            //Moves the character from one position to another
            float time = 0;
            Vector3 startPos = objectToMove.transform.position;
            while (time < 1)
            {
                time += Time.deltaTime / moveTime;
                objectToMove.transform.position = Vector3.Lerp(startPos, gridPosY, time);
                yield return null;
            }

            if (environmentalObject != null)
                environmentalObject.AssignAllTilesBeneath(GridAvailability.Occupied);

            gridToMoveTo.FindObjectAbove();
        }

        public IEnumerator RotateCharacterOverTime(GameObject objectToMove, GridStats gridToMoveTo, float moveTime)
        {
            Vector3 startPosition = objectToMove.transform.position;
            Vector3 targetPosition = gridToMoveTo.transform.position;

            startPosition.y = 0;
            targetPosition.y = 0;

            Vector3 targetRotationEuler =
                new(0, Quaternion.LookRotation(targetPosition - startPosition).eulerAngles.y, 0);
            Quaternion targetRotation = Quaternion.Euler(targetRotationEuler);

            float time = 0;
            while (time < 1)
            {
                time += Time.deltaTime / moveTime;
                objectToMove.transform.rotation =
                    Quaternion.Lerp(objectToMove.transform.rotation, targetRotation, time);
                yield return new WaitForEndOfFrame();
            }
        }

        public float GetRotationTimeModifier()
        {
            return rotationTimeModifier;
        }
    }
}