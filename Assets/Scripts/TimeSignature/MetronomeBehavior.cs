/******************************************************************
*    Author: Nick Grinstead
*    Contributors: David Galmines
*    Date Created: 10/28/24
*    Description: Checks for collisions with the player and then 
*       calls the TimeSignatureManager to update the time signature.
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

public class MetronomeBehavior : MonoBehaviour
{
    private const string _WEIGHT_ANIM_UP = "Armature|WeightSlide_Up";
    private const string _WEIGHT_ANIM_DOWN = "Armature|WeightSlide_Down";

    //the ripple effect when touching the metronome
    [SerializeField] private ParticleSystem _contactIndicator;
    //the circle that flashes around the HUD time signature
    [SerializeField] private GameObject _HUDEffect;
    //is this the tutorial tmetronome puzzle?
    [SerializeField] private bool _isThisTheTutorial;
    //is the metronome initially on the slow setting?
    [SerializeField] private bool _initiallySlow;
    
    //the animator for the metronome
    [SerializeField]
    private Animator _anim;

    [SerializeField] private AnimationClip _change;
    [Header("Speed Settings")]
    [SerializeField]
    float fastSpeed = 2;
    [SerializeField]
    float slowSpeed = 1;

    bool isSlow = true;

    /// <summary>
    /// Keeps the particle effects rfom playing right away.
    /// </summary>
    private void Awake()
    {
        _contactIndicator.Pause();
        //_anim = GetComponentInParent<Animator>();

        isSlow = _initiallySlow;
    }

    /// <summary>
    /// Toggles the time signature on the manager if there is one
    /// </summary>
    private void ActivateMetronome()
    {
        if (TimeSignatureManager.Instance != null)
            TimeSignatureManager.Instance.ToggleTimeSignature();

        SetAnimSpeed();
    }

    public void SetAnimSpeed()
    {
        isSlow = !isSlow;
        
        _anim.speed = isSlow ? slowSpeed : fastSpeed;
        print("Updated Speed: " + _anim.speed);

        //_anim.SetBool("WeightUp", isSlow);
    }

    /// <summary>
    /// Play's the HUD indicator effect on a delay after the player 
    /// touches the metronome.
    /// </summary>
   /* private IEnumerator HUDIndicator()
    {
        WaitForSeconds wait = new(0.5f);

        if (_initiallySlow)
        {
            _anim.SetBool("GoFaster", true);
        }
        else
        {
            _anim.SetBool("GoFaster", false);
        }

        yield return wait;
        _HUDEffect.SetActive(true);
        yield return wait;
        _HUDEffect.SetActive(false);
        yield return wait;
        _HUDEffect.SetActive(true);
        yield return wait;
        _HUDEffect.SetActive(false);
        yield return wait;
        _HUDEffect.SetActive(true);
        yield return wait;
        _HUDEffect.SetActive(false);
        yield return null;
    }*/

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
                //StopAllCoroutines();
                //StartCoroutine("HUDIndicator");
            }

            _contactIndicator.Play();
            _HUDEffect.SetActive(false);

            PlayerMovement playerMovement;
            if (other.gameObject.TryGetComponent<PlayerMovement>(out playerMovement))
            {
                playerMovement.ForceTurnEnd();
            }
        }
    }
}
