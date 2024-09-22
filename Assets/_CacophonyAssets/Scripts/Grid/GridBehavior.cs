using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: Ryan
/// Editor(s): Trinity
/// Description: Makes a grid, and provides functions for moving on it
/// </summary>
namespace Cacophony
{
    public class GridBehavior : MonoBehaviour
    {
        [Header("Variables")] public int _columns;
        public int _rows;
        public float _scale = 1;

        [Space] [Header("References")] [SerializeField]
        GameObject gridSpace;

        [SerializeField] LayerMask _gridLayer;
        [SerializeField] LayerMask _objectsOnGrid;
        [SerializeField] LayerMask _playerLayer;
        [SerializeField] LayerMask _charactersLayer;

        [Space] [Header("Grid Materials")] [SerializeField]
        Material baseMaterial;

        [SerializeField] Material highlightedMaterial;
        [SerializeField] Material _startingPositionMaterial;

        private GameObject[,] _gridArray;

        // Start is called before the first frame update
        void Start()
        {
            StartingLocation();
            CreateGrid();

            GameplayManagers.Instance.Room.RoomVictoryEvent += OnRoomVictory;
        }

        void StartingLocation()
        {
            transform.position = new Vector3(transform.position.x - (float) _columns / 2, transform.position.y,
                transform.position.z - (float) _rows / 2);
            if (_columns % 2 != 0)
                transform.position += new Vector3(.5f, 0, 0);
            if (_rows % 2 != 0)
                transform.position += new Vector3(0, 0, .5f);
        }

        void CreateGrid()
        {
            _gridArray = new GameObject[_columns, _rows];
            for (int xGrid = 0; xGrid < _columns; xGrid++)
            {
                for (int yGrid = 0; yGrid < _rows; yGrid++)
                {
                    InstantiateGridTile(xGrid, yGrid);
                }
            }
        }

        public GridStats InstantiateGridTile(int x, int y)
        {
            Vector3 gridLocation = WorldPositionFromCoordinates(x, y);
            GameObject newGridTile = Instantiate(gridSpace, gridLocation, Quaternion.identity);
            newGridTile.transform.SetParent(gameObject.transform);

            GridStats gridTileStats = newGridTile.GetComponent<GridStats>();
            gridTileStats.SetXPos(x);
            gridTileStats.SetYPos(y);

            _gridArray[x, y] = newGridTile;

            return newGridTile.GetComponent<GridStats>();
        }

        public GridStats InstantiateGridTile(int x, int y, GridAvailability availability)
        {
            GridStats gridTile = InstantiateGridTile(x, y);
            gridTile.SetGridAvailability(availability);

            return gridTile;
        }

        /// <summary>
        /// Gets the position a gridspace would be at, if it had the given coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>World position of submitted grid coordinates</returns>
        private Vector3 WorldPositionFromCoordinates(int x, int y)
        {
            return new Vector3(transform.position.x + (x * _scale), transform.position.y,
                transform.position.z + (y * _scale));
        }

        public Vector3 WorldPositionFromCoordinates(Vector2Int coords)
        {
            return WorldPositionFromCoordinates(coords.x, coords.y);
        }

        /// <summary>
        /// Finds the grid at a position
        /// </summary>
        /// <param name="startPos"></param>
        /// <returns></returns>
        public GridStats DetermineGridSpaceFromPosition(Vector3 startPos)
        {
            //Debug.Log("StartRay" + startPos);
            RaycastHit rayHit;
            //Debug.DrawRay(startPos, Vector3.down, Color.red, 1);
            if (Physics.Raycast(startPos, Vector3.down, out rayHit, 5, _gridLayer))
            {
                return rayHit.collider.gameObject.GetComponentInParent<GridStats>();
            }

            return null;
        }

        /// <summary>
        /// Determines if you can move in a direction
        /// </summary>
        /// <param name="moveDirection"></param>
        /// <param name="currentGrid"></param>
        /// <returns></returns>
        public bool DetermineValidMovementDirection(Vector2 moveDirection, GridStats currentGrid)
        {
            return DetermineIfOutOfBounds(moveDirection, currentGrid);
        }

        /// <summary>
        /// Determines if a space is out of bounds
        /// </summary>
        /// <param name="moveDirection"></param>
        /// <param name="currentGrid"></param>
        /// <returns></returns>
        public bool DetermineIfOutOfBounds(Vector2 moveDirection, GridStats currentGrid)
        {
            //Vector2 newGridPosition = FindGridInDirection()
            //GridStats newGrid = FindGridInDirection(moveDirection, currentGrid);
            float newXPos = currentGrid.GetXPos() + moveDirection.x;
            float newYPos = currentGrid.GetYPos() + moveDirection.y;
            if (newXPos > _columns - 1 || newXPos < 0)
            {
                //Debug.Log("That movement would go out of bounds in the x");
                return false;
            }

            if (newYPos > _rows - 1 || newYPos < 0)
            {
                //Debug.Log("That movement would go out of bounds in the Y");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Finds a grid space in a specific direction if it exists
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="startGrid"></param>
        /// <returns></returns>
        public GridStats FindGridInDirection(Vector2 direction, GridStats startGrid)
        {
            //If that would place you out of bounds stop
            if (!DetermineIfOutOfBounds(direction, startGrid)) return null;
            //Returns the grid spot in a direction
            return _gridArray[startGrid.GetXPos() + (int) direction.x, startGrid.GetYPos() + (int) direction.y]
                .GetComponentInChildren<GridStats>();
        }


        /// <summary>
        /// To be called when the room is cleared
        /// </summary>
        private void OnRoomVictory()
        {
            //ToggleExits(true);
            GameplayManagers.Instance.Room.RoomVictoryEvent -= OnRoomVictory;
        }

        public bool CheckForObjectAtGrid(GridStats moveToGrid)
        {
            //moveToGrid.GetObjectOnTile().gameObject.name
            if (moveToGrid.GetObjectOnTile() != null)
            {
                return true;
            }

            return false;
        }

        public int GetRows()
        {
            return _rows;
        }

        public int GetColumns()
        {
            return _columns;
        }

        public int ReturnGreaterOfRowsColumns()
        {
            if (_rows > _columns) return _rows;
            return _columns;
        }

        public LayerMask GetObjectsOnTilesLayerMask()
        {
            return _objectsOnGrid;
        }

        public LayerMask GetPlayerLayerMask()
        {
            return _playerLayer;
        }

        public LayerMask GetCharactersLayerMask()
        {
            return _charactersLayer;
        }

        public Material GetBaseTileMaterial()
        {
            return baseMaterial;
        }

        public Material GetHighlightedTileMaterial()
        {
            return highlightedMaterial;
        }

        public Material GetStartingTileMaterial()
        {
            return _startingPositionMaterial;
        }
    }
}