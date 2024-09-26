using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Author: Trinity
/// Description: Controls the movement of moveable environment objects
/// </summary>
namespace Cacophony
{
    public class MovingObject : AbstractEnvironmentalObject
    {
        [SerializeField] Transform waypointParent;

        [SerializeField] float animSpeed = 3;

        [SerializeField] float moveLength = 0.2f;

        Animator anim;

        Queue<Vector3> queue = new();

        private void Awake()
        {
            anim = GetComponent<Animator>();
            anim.speed = animSpeed;
        }

        private void Start()
        {
            foreach (Transform child in waypointParent)
                queue.Enqueue(child.position);
        }

        /// <summary>
        /// Moves the object along the next point of its waypoints, then re-adds that point so that the movement loops
        /// </summary>
        protected override void Move()
        {
            Vector3 nextPos = queue.Peek();
            GridStats currentTile = GameplayManagers.Instance.Grid.DetermineGridSpaceFromPosition(transform.position);
            GridStats nextTile = GameplayManagers.Instance.Grid.DetermineGridSpaceFromPosition(nextPos);

            if (nextTile == null || nextTile.GetGridAvailability() == GridAvailability.Occupied)
            {
                //Debug.Log("Grr");
                anim.Play("Frustrated");

                return;
            }

            //Debug.Log("Moving to " + nextPos);
            nextPos = queue.Dequeue();

            GameplayManagers.Instance.Mover.MoveCharacter(gameObject, nextTile, moveLength);
            currentTile.SetObjectOnTile(null);

            queue.Enqueue(nextPos);
            //Debug.Log("Queue size: " + queue.Count);
        }
    }
}