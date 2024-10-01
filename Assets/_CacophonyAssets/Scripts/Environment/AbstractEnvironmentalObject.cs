using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cacophony
{
    public abstract class AbstractEnvironmentalObject : MonoBehaviour, IEnvironmentHazard
    {
        [SerializeField] Transform viabilityCheckParent;

        private void OnEnable()
        {
            //This was having errors because it was happening too early, this is a bandaid fix. Improve later - Ryan
            Invoke("TestFunc", .1f);
        }

        private void TestFunc()
        {
            GameplayManagers.Instance.Turn.EnvironmentTurnEvent += OnEnvironmentTurn;
        }

        private void OnDisable()
        {
            GameplayManagers.Instance.Turn.EnvironmentTurnEvent -= OnEnvironmentTurn;
        }

        public void OnEnvironmentTurn()
        {
            if (IsNextMoveViable())
            {
                AssignAllTilesBeneath(GridAvailability.Empty);
                Move();
            }
        }

        protected abstract void Move();

        /// <summary>
        /// Checks if the object can move the way it intends to
        /// </summary>
        /// <returns></returns>
        public bool IsNextMoveViable()
        {
            GridBehavior grid = GameplayManagers.Instance.Grid;
            LayerMask layerMask = grid.GetCharactersLayerMask();
            Vector3 addedY = new(0, 3, 0);

            foreach (Transform t in viabilityCheckParent)
            {
                Vector3 newPos = t.position + addedY;
                if (Physics.Raycast(newPos, Vector3.down, out _, 5, layerMask))
                {
                    //Debug.Log("Found Character: " + hit.collider.name);

                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Assigns all tiles underneath this object's collider to be the associated availability
        /// </summary>
        /// <param name="availability">Availability to be set</param>
        public void AssignAllTilesBeneath(GridAvailability availability)
        {
            //Debug.Log("Setting Availability");
            GridBehavior grid = GameplayManagers.Instance.Grid;

            BoxCollider boxCollider = GetComponent<BoxCollider>();
            Vector3 center = boxCollider.center;

            GameObject objectOnTile = availability == GridAvailability.Occupied ? gameObject : null;


            int xBound = (int) (boxCollider.size.x / 2);
            int zBound = (int) (boxCollider.size.z / 2);
            //Debug.Log("Xbound = " + xBound + ", zBound = " + zBound);

            for (int x = -xBound; x <= xBound; x++)
            {
                for (int z = -zBound; z <= zBound; z++)
                {
                    Vector3 point = transform.TransformPoint(center + new Vector3(x, 0, z));
                    //Debug.Log(new Vector3(x, 0, z));
                    GridStats tile = grid.DetermineGridSpaceFromPosition(point);
                    if (tile == null)
                        continue;

                    tile.SetObjectOnTile(objectOnTile);
                    tile.SetGridAvailability(availability);
                }
            }
            //Debug.Log("------------------------------------------");
        }

        private void OnDrawGizmos()
        {
            if (viabilityCheckParent == null)
                return;

            Gizmos.color = Color.black;
            foreach (Transform t in viabilityCheckParent)
                Gizmos.DrawSphere(t.position, 0.25f);
        }
    }
}