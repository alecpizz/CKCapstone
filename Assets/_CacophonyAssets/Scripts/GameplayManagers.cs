using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cacophony
{
    /// <summary>
    /// Author: Ryan
    /// Description: Holds the managers of anything needed in the scene in order to opperate
    /// </summary>
    public class GameplayManagers : MonoBehaviour
    {
        public static GameplayManagers Instance;
        public GridBehavior Grid;
        public MoveOnGrid Mover;
        public TurnManager Turn;
        public RoomManager Room;
        public HarmonizableManager Harmonizer;
        public EnemyManager Enemy;

        // Start is called before the first frame update
        void Awake()
        {
            Instance = this;
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}