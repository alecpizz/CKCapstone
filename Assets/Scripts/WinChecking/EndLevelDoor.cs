/******************************************************************
*    Author: Nick Grinstead
*    Contributors: 
*    Date Created: 9/24/24
*    Description: Door unlocks when action on WinChecker is invoked.
*       If door is unlocked and player walks into it, a new scene will load.
*******************************************************************/
using UnityEngine;
using UnityEngine.SceneManagement;
using NaughtyAttributes;

public class EndLevelDoor : MonoBehaviour
{
    [Scene]
    [SerializeField] private int _levelIndexToLoad = 0;
    [SerializeField]
    bool _isUnlocked = false;
    DoorGlow doorGlow;

    private void Awake()
    {
        doorGlow = GetComponent<DoorGlow>();
    }

    /// <summary>
    /// Registers to win checker action
    /// </summary>
    private void Start()
    {
        WinChecker.GotCorrectSequence += UnlockDoor;
    }

    /// <summary>
    /// Unregisters from win checker action
    /// </summary>
    private void OnDisable()
    {
        WinChecker.GotCorrectSequence -= UnlockDoor;
    }

    /// <summary>
    /// Called when correct sequence is created to open door
    /// </summary>
    public void UnlockDoor()
    {
        _isUnlocked = true;
         if (doorGlow != null)
        {
            // Call the UnlockDoor method from EndLevelDoor
            doorGlow.GlowAndUnlockDoor();
        }
    }

    // TODO: this OnTriggerEnter method can be replaced with grid data checking

    /// <summary>
    /// If player collides with door and it's unlocked, load the next scene
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (_isUnlocked && other.CompareTag("Player"))
        {
            SceneController.Instance.LoadNewScene(_levelIndexToLoad);
        }
        else if (other.CompareTag("Player"))
        {
            // TODO: provide feedback for trying to enter locked door here
        }
    }
}
