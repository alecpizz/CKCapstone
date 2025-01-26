/********************************************************************
*    Author: David Henvick
*    Contributors:
*    Date Created: 9/30/24
*    Description: This is the script that is used for the interact input
*******************************************************************/
using UnityEngine;
using UnityEngine.InputSystem;

// TODO: merge this script together with PlayerInteraction
public class PlayerInteract : MonoBehaviour
{
    //npc
    private NpcDialogueController _npc;
    private PlayerControls _input;

    /// <summary>
    /// Start is called before the first frame update
    /// used to assign needed input variables
    /// </summary>
    void Start()
    {
        // Referencing and setup of the Input Action functions
        _input = new PlayerControls();
        _input.InGame.Enable();

        _input.InGame.Interact.performed += InteractPerformed;
    }

    /// <summary>
    /// Called on collision, is used to log interactables within range
    /// </summary>
    /// <param name="collision">Data from a collision</param>
    void OnCollisionEnter(Collision collision)
    {
        _npc = collision.gameObject.GetComponent<NpcDialogueController>();
        if ( _npc != null)
        {
            //_npc.ShowDialogue();
        }
    }

    /// <summary>
    /// Called on collision, is used to remove logged interactables when they leave the range
    /// </summary>
    /// <param name="collision">Data from a collision</param>
    void OnCollisionExit(Collision collision)
    {
        _npc = collision.gameObject.GetComponent<NpcDialogueController>();
        if (_npc != null)
        {
            _npc.HideDialogue();
        }
        _npc = null;
    }

    /// <summary>
    /// Activated when the player uses the interact action
    /// is used for npc interaction
    /// </summary>
    /// <param name="context">Input action callback</param>
    private void InteractPerformed(InputAction.CallbackContext context)
    {
        if (_npc != null)
        {
            _npc.AdvanceDialogue();
        }
    }
}
