/******************************************************************
*    Author: Nick Grinstead
*    Contributors: Nick Grinstead, Mitchell Young
*    Date Created: 10/28/24
*    Description: Checks for collisions with the player and then 
*       calls the TimeSignatureManager to update the time signature.
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetronomeBehavior : MonoBehaviour
{
    [SerializeField] private GameObject model;
    [SerializeField] private bool tickingSlow = false;
    [SerializeField] private float durationTime = 2f;

    Animator getAnimator;

    private void Awake()
    {
        getAnimator = model.GetComponent<Animator>();
    }

    /// <summary>
    /// Toggles the time signature on the manager if there is one
    /// </summary>
    private void ActivateMetronome()
    {
        if (TimeSignatureManager.Instance != null)
            TimeSignatureManager.Instance.ToggleTimeSignature();
    }

    /// <summary>
    /// Activates the metronome and stops player movement in response 
    /// to a collision with the player.
    /// </summary>
    /// <param name="other">Data from a collision</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            ActivateMetronome();

            if (!tickingSlow)
            {
                getAnimator.SetBool("TickingFast", true);
                StartCoroutine(waitForDuration());
                tickingSlow = false;
            }
            else
            {
                getAnimator.SetBool("TickingSlow", true);
                StartCoroutine(waitForDuration());
                tickingSlow = true;
            }

            PlayerMovement playerMovement;
            if (other.gameObject.TryGetComponent<PlayerMovement>(out playerMovement))
            {
                playerMovement.ForceTurnEnd();
            }
        }
    }

    /// <summary>
    /// Waits for the duration float time amount before setting the animator variables to false. 
    /// This returns the model to its default animation.
    /// </summary>
    /// <returns></returns>
    IEnumerator waitForDuration()
    {
        yield return new WaitForSeconds(durationTime);
        getAnimator.SetBool("TickingFast", false);
        getAnimator.SetBool("TickingSlow", false);
    }

}
