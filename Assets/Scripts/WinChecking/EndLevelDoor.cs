/******************************************************************
*    Author: Nick Grinstead
*    Contributors: David Galmines, Cole Stranczek
*    Date Created: 9/24/24
*    Description: Door unlocks when action on WinChecker is invoked.
*       If door is unlocked and player walks into it, a new scene will load.
*******************************************************************/

using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;
using SaintsField;

public class EndLevelDoor : MonoBehaviour
{
    [Scene]
    [SerializeField] private int _levelIndexToLoad = 0;
    [SerializeField] private bool _isUnlocked = false;
    private DoorGlow _doorGlow;
    [SerializeField] private GameObject _doorPortalVFX;

    [SerializeField] private ParticleSystem _unlockedParticles;

    [SerializeField] private Animator _anim;

    [SerializeField] private DoorLightBehaviour _lanternScript;

    [SerializeField] private EventReference _doorSound;

    /// <summary>
    /// Door glow is assigned a value when the function awakens
    /// </summary>
    private void Awake()
    {
        _doorGlow = GetComponent<DoorGlow>();
        _doorPortalVFX.SetActive(false);
    }

    /// <summary>
    /// Registers to win checker action
    /// </summary>
    private void Start()
    {
        WinChecker.GotCorrectSequence += UnlockDoor;

        if (_isUnlocked)
        {
            UnlockDoor();
        }
        else
        {
            //prevent VFX particles from playing immediately if
            //notes are not collected
            _unlockedParticles.Pause();
        }
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
         if (_doorGlow != null)
        {
            // Call the UnlockDoor method from EndLevelDoor
            _doorGlow.GlowAndUnlockDoor();

            //play "door unlocked" VFX
            _unlockedParticles.Play();

            //make the door open
            _anim.Play("ANIM_DoorOpen");

            //light the lantern (null check for cutscene skip)
            if(_lanternScript != null)
                _lanternScript.TurnLightOn();
            
            _doorPortalVFX.SetActive(true);
        }
         //End level door opening sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(_doorSound);
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
            // Edit from Cole, this is to make sure the game speed is immediately normalized
            // upon completing a level in case the player has the game sped up or slowed down
            // so that any post-level isn't messed up. Just to make sure my game speed changes
            // don't screw up anything.
            Time.timeScale = 1f;

            PlayerMovement playerMovement;
            if (other.gameObject.TryGetComponent<PlayerMovement>(out playerMovement))
            {
                playerMovement.ForceTurnEnd();
                playerMovement.enabled = false;
            }
            string scenePath = SceneUtility.GetScenePathByBuildIndex(_levelIndexToLoad);
            SaveDataManager.SetLastFinishedLevel(scenePath);
            SaveDataManager.SetLevelCompleted(SceneManager.GetActiveScene().path);
            SceneController.Instance.LoadNewScene(_levelIndexToLoad);
        }
        else if (other.CompareTag("Player"))
        {
            // TODO: provide feedback for trying to enter locked door here
        }
    }
}
