using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Author: Ryan
/// Editor(s): Trinity
/// Description: Holds all data for a room
/// </summary>
public class RoomManager : MonoBehaviour
{
    public event Action RoomVictoryEvent;
    public event Action RoomStartEvent;
    public event Action RoomFailEvent;


    [Space]
    [Header("Exits")]
    public Vector2Int positionOnLevelGrid;
    [SerializeField] GameObject exitPrefab;
    [SerializeField] List<ExitLocationData> exitSpawns = new List<ExitLocationData>();
    [SerializeField] LayerMask _exitLayerMask;
    private List<GameObject> exitObjects = new();
    private ExitLocationData _exitIEnteredFrom;

    private bool _roomFailed;

    private void Start()
    {
        
        AssignPositionOnGrid();
        NewCreateExits();
        RoomVictoryEvent += ActivateExits;
        RoomVictoryEvent += MusicController.Instance.OnRoomVictory;
        //RoomVictoryEvent += FindObjectOfType<GameMenuController>().SetLevelText;

        //Debug.Log(SaveSceneData.Instance.GetSceneCompletion(SceneManager.GetActiveScene().buildIndex));
        //Debug.Log(SceneManager.GetActiveScene().buildIndex);
        if (GetRoomSolved())
            OnRoomVictory();
    }

    private void Update()
    {
        //This is a debug key left by Ryan, if you find this then Ryan is a moron who forgot to remove it.
        //Send a picture of this to Ryan to make him cry
        /*if (Input.GetKeyDown(KeyCode.Y))
            RoomVictory();*/
    }

    public void AssignPositionOnGrid()
    {
        positionOnLevelGrid = SaveSceneData.Instance.GetSceneDataOfCurrentScene().positionOnSceneGrid;
    }

    /// <summary>
    /// Is called when the room is complete, uses an event to activate everything on completetion of the room
    /// </summary>
    public void RoomVictory()
    {
        if (GetRoomSolved())
            return;

        SetRoomSolved(true);
        OnRoomVictory();

        AudioManager.Instance.PlaySoundEffect("SolvePuzzle");
    }

    private void OnRoomVictory()
    {
        RoomVictoryEvent?.Invoke();
    }

    public void RoomFail()
    {
        _roomFailed = true;

        RoomFailEvent?.Invoke();

        AudioManager.Instance.PlaySoundEffect("FailPuzzle");

        StartCoroutine(DelayedRoomReset());
    }

    private IEnumerator DelayedRoomReset()
    {
        yield return new WaitForSeconds(0.15f);
        ResetRoom();
    }
    
    public void ResetRoom()
    {
        FindObjectOfType<PauseScreenUI>().LoadScene(SceneTransitions.Instance.GetBuildIndex());
    }
    //This should probably be part of the scene loader script - Ryan
    /*IEnumerator DelayedLoad()
    {
        yield return new WaitForSeconds(0.2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        RoomStartEvent?.Invoke();
    }*/


    //Old Function for creating exits
    void CreateExits()
    {
        /*foreach (ExitLocationData coords in exitSpawns)
        {
            GameObject newestExit = Instantiate(exitPrefab);

            newestExit.SetActive(false);
            newestExit.transform.position = GameplayManagers.Instance.Grid.WorldPositionFromCoordinates(coords.exitSpawnLocation);
            newestExit.GetComponent<Exit>().SetSceneIDToLoad(coords.exitSpawnSceneID);

            exitObjects.Add(newestExit);
        }*/
    }

    /// <summary>
    /// Creates exits on the sides of the level in any direction that there is another level
    /// </summary>
    void NewCreateExits()
    {
        List<Vector2Int> directions =  new()
        {
            new Vector2Int(0, 1),
            new Vector2Int(-1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0)
        };

        //Goes through every direction
        foreach(Vector2Int currentDir in directions)
        {
            //Finds what scene is in that direction
            GameplayScenes currentSaveData = SaveSceneData.Instance.FindSceneDataInDirection(currentDir);
            //Checks if the scene exists in that direction
            if (currentSaveData != null && currentSaveData.sceneID != -1)
            {
                //Spawns an exit
                GameObject newestExit = Instantiate(exitPrefab);

                //Sets it inactive
                newestExit.GetComponent<Exit>().ExitActivationStatus(false);

                foreach (ExitLocationData coords in exitSpawns)
                    if(coords.exitSpawnDirection == currentDir)
                    {
                        //Assigns the position of the portal based on where the ExitLocationData wants it in that direction
                        newestExit.transform.position = GameplayManagers.Instance.Grid.WorldPositionFromCoordinates(coords.exitSpawnLocation);
                        //newestExit.transform.position = new Vector3(newestExit.transform.position.x, 1 , newestExit.transform.position.z);
                        newestExit.GetComponent<Exit>().GiveTransition(coords.ExitTransition);
                        coords.AssociatedExit = newestExit.gameObject;
                        if(coords.exitSpawnDirection == SaveSceneData.Instance.GetLastSceneDirection())
                        {
                            newestExit.GetComponent<Exit>().ExitActivationStatus(true);
                        }
                    }

                Vector3 targetRotationEuler = new(0, Quaternion.LookRotation(new Vector3(currentDir.x, 0, currentDir.y)).eulerAngles.y - 90, 0);
                newestExit.transform.rotation = Quaternion.Euler(targetRotationEuler);
                //Assigns what scene to load        
                newestExit.GetComponent<Exit>().SetSceneIDToLoad(currentSaveData.sceneID);
                newestExit.GetComponent<Exit>().SetDirectionOfNextScene(currentDir);
                //Adds the exit to the list of exits
                exitObjects.Add(newestExit);
            }
        }
    }

    public ExitLocationData FindExitFromDirection(Vector2 dir)
    {
        foreach (ExitLocationData coords in exitSpawns)
            if (coords.exitSpawnDirection == dir)
                return coords;
        return null;
    }


    /// <summary>
    /// Toggles whether the exits are active or not
    /// </summary>
    /// <param name="active"></param>
    private void ToggleExits(bool active)
    {
        foreach (GameObject exit in exitObjects)
        {
            exit.GetComponent<Exit>().ExitActivationStatus(true);
        }
            
    }

    public void ActivateExits()
    {
        ToggleExits(true);
    }

    public void SetEntranceExit(ExitLocationData exit)
    {
        _exitIEnteredFrom = exit;
    }

    public ExitLocationData GetEntranceExit()
    {
        return _exitIEnteredFrom;
    }

    public LayerMask GetExitLayerMask()
    {
        return _exitLayerMask;
    }

    public void SetRoomSolved(bool solved)
    {
        SaveSceneData.Instance.SetSceneAsComplete(SceneManager.GetActiveScene().buildIndex, solved);
    }

    public bool GetRoomSolved()
    {
        return SaveSceneData.Instance.GetSceneCompletion(SceneManager.GetActiveScene().buildIndex);

    }

    public bool GetRoomFailed()
    {
        return _roomFailed;
    }
}

[System.Serializable]
public class ExitLocationData
{
    internal GameObject AssociatedExit;
    public Vector2Int exitSpawnDirection;
    public Vector2Int exitSpawnLocation;
    public Vector3Int playerSpawnFromEntrance;
    public SceneTransitions.TransitionType ExitTransition;
}
