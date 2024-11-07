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
                model.GetComponent<Animator>().SetBool("TickingFast", true);
                StartCoroutine(waitASec());
                tickingSlow = false;
            }
            else
            {
                model.GetComponent<Animator>().SetBool("TickingSlow", true);
                StartCoroutine(waitASec());
                tickingSlow = true;
            }

            PlayerMovement playerMovement;
            if (other.gameObject.TryGetComponent<PlayerMovement>(out playerMovement))
            {
                playerMovement.ForceTurnEnd();
            }
        }
    }

    IEnumerator waitASec()
    {
        yield return new WaitForSeconds(2f);
        model.GetComponent<Animator>().SetBool("TickingFast", false);
        model.GetComponent<Animator>().SetBool("TickingSlow", false);
    }

}
