/******************************************************************
*    Author: Nick Grinstead
*    Contributors: David Galmines
*    Date Created: 10/28/24
*    Description: Checks for collisions with the player and then 
*       calls the TimeSignatureManager to update the time signature.
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetronomeBehavior : MonoBehaviour
{
    [SerializeField] private ParticleSystem _contactIndicator;
    [SerializeField] private GameObject _HUDEffect;
    [SerializeField] private bool _isThisTheTutorial;
    private Animator _anim;

    /// <summary>
    /// Keeps the particle effects rfom playing right away.
    /// </summary>
    private void Awake()
    {
        _contactIndicator.Pause();
        _anim = GetComponentInParent<Animator>();
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
    /// Play's the HUD indicator effect on a delay after the player 
    /// touches the metronome.
    /// </summary>
    private IEnumerator HUDIndicator()
    {
        yield return new WaitForSeconds(0.5f);
        _HUDEffect.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        _HUDEffect.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        _HUDEffect.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        _HUDEffect.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        _HUDEffect.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        _HUDEffect.SetActive(false);
        yield return null;
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

            if (_isThisTheTutorial)
            {
                StopAllCoroutines();
                StartCoroutine("HUDIndicator");
            }

            _contactIndicator.Play();
            _HUDEffect.SetActive(false);
            _anim.speed *= 0.5f;

            PlayerMovement playerMovement;
            if (other.gameObject.TryGetComponent<PlayerMovement>(out playerMovement))
            {
                playerMovement.ForceTurnEnd();
            }
        }
    }
}
