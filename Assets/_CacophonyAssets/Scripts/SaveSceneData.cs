using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cacophony
{
    public class SaveSceneData : MonoBehaviour
    {
        public static SaveSceneData Instance;

        [SerializeField] List<GameplayScenes> _gameplayScenes;
        List<GameplayScenes> _resetGameplayScenes = new List<GameplayScenes>();

        private Vector2 _lastSceneDirection;

        // Start is called before the first frame update
        void Awake()
        {
            EstablishSingleton();
        }

        /// <summary>
        /// Creates a singleton of SaveSceneData
        /// </summary>
        private void EstablishSingleton()
        {
            if (Instance == null && Instance != this)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            CopyList();
            //_resetGameplayScenes = new List<GameplayScenes>(_gameplayScenes);
            DontDestroyOnLoad(gameObject);
        }

        private void CopyList()
        {
            //So as it turns out you can't just set 1 list equal to another list
            //Because if you do if you update the original list it updates the copy
            //So I have to do all this to unlink the lists
            //If there is a better way please tell me

            foreach (GameplayScenes gameScenes in _gameplayScenes)
            {
                GameplayScenes copyScene = new GameplayScenes();
                copyScene.sceneID = gameScenes.sceneID;
                copyScene.positionOnSceneGrid = gameScenes.positionOnSceneGrid;
                copyScene.completed = gameScenes.completed;
                _resetGameplayScenes.Add(copyScene);
            }
        }

        /// <summary>
        /// Marks a scene as completed
        /// </summary>
        /// <param name="inSceneID"></param>
        /// <param name="sceneComplete"></param>
        public void SetSceneAsComplete(int inSceneID, bool sceneComplete)
        {
            foreach (GameplayScenes gameScene in _gameplayScenes)
            {
                if (gameScene.sceneID == inSceneID)
                    gameScene.completed = sceneComplete;
            }
        }

        /// <summary>
        /// Gets whether a scene is complete
        /// </summary>
        /// <param name="inSceneID"></param>
        /// <returns></returns>
        public bool GetSceneCompletion(int inSceneID)
        {
            foreach (GameplayScenes gameScene in _gameplayScenes)
            {
                if (gameScene.sceneID == inSceneID)
                    return gameScene.completed;
            }

            return false;
        }

        /// <summary>
        /// Gets the GameplayScene variable for the current scene
        /// </summary>
        /// <returns></returns>
        public GameplayScenes GetSceneDataOfCurrentScene()
        {
            foreach (GameplayScenes gameScene in _gameplayScenes)
            {
                if (gameScene.sceneID == UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex)
                    return gameScene;
            }

            return null;
        }

        /// <summary>
        /// Finds the data of a scene in a direction on the scene grid
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public GameplayScenes FindSceneDataInDirection(Vector2Int dir)
        {
            foreach (GameplayScenes gameScene in _gameplayScenes)
            {
                if (gameScene.positionOnSceneGrid == GameplayManagers.Instance.Room.positionOnLevelGrid + dir)
                {
                    return gameScene;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds the data of a scene in the given direction, based on the inputed scene ID
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public GameplayScenes FindSceneDataInDirection(GameplayScenes scene, Vector2Int dir)
        {
            foreach (GameplayScenes gameScene in _gameplayScenes)
            {
                if (gameScene.positionOnSceneGrid == scene.positionOnSceneGrid + dir)
                {
                    return gameScene;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the amount of levels completed
        /// </summary>
        /// <returns></returns>
        public int LevelCompleteCount()
        {
            int levelsCompleted = 0;
            foreach (GameplayScenes gameScene in _gameplayScenes)
                if (gameScene.completed)
                    levelsCompleted++;
            if (levelsCompleted == _gameplayScenes.Count)
            {
                //All levels are completed. Have fun Rudy!
            }

            return levelsCompleted;
        }

        /*public bool CheckAllLevelsComplete()
        {
            foreach (GameplayScenes gameScene in _gameplayScenes)
                if (!gameScene.completed) return false;
            return true;
        }*/

        /// <summary>
        /// Resets the data to the default
        /// </summary>
        public void ResetData()
        {
            TutorialManager.Instance.ResetTutorials();

            _gameplayScenes = new List<GameplayScenes>();
            foreach (GameplayScenes gameScenes in _resetGameplayScenes)
            {
                GameplayScenes copyScene = new GameplayScenes();
                copyScene.sceneID = gameScenes.sceneID;
                copyScene.positionOnSceneGrid = gameScenes.positionOnSceneGrid;
                copyScene.completed = gameScenes.completed;
                copyScene.themeMusic = gameScenes.themeMusic;
                _gameplayScenes.Add(copyScene);
            }
        }

        public void SetLastSceneDirection(Vector2 lastDir)
        {
            _lastSceneDirection = lastDir;
        }

        public Vector2 GetLastSceneDirection()
        {
            return _lastSceneDirection;
        }

        public List<GameplayScenes> GetScenes()
        {
            return _gameplayScenes;
        }
    }

    [System.Serializable]
    public class GameplayScenes
    {
        public int sceneID;
        public Vector2Int positionOnSceneGrid;
        public bool completed;
        public bool hasVisited;
        public string themeMusic;
    }
}