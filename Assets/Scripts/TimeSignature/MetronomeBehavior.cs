/******************************************************************
*    Author: Nick Grinstead
*    Contributors: David Galmines, Trinity Hutson, Mitchell Young
*    Date Created: 10/28/24
*    Description: Checks for collisions with the player and then 
*       calls the TimeSignatureManager to update the time signature.
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using FMOD.Studio;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MetronomeBehavior : MonoBehaviour, ITimeListener
{
    public UnityEvent MetronomeTriggered;

    
    //animations for the weight on the metronome moving up and down
    private const string _WEIGHT_ANIM_UP = "Armature|WeightSlide_Up";
    private const string _WEIGHT_ANIM_DOWN = "Armature|WeightSlide_Down";

    //the ripple effect when touching the metronome
    [SerializeField] private ParticleSystem _contactIndicator;
    //the ripple effect on HUD when touching the metronome
    [SerializeField] private ParticleSystem _hudIndicator;
    //the circle that flashes around the HUD time signature
    [SerializeField] private GameObject _HUDEffect;
    //is this the tutorial tmetronome puzzle?
    [SerializeField] private bool _isThisTheTutorial;
    //is the metronome initially on the slow setting?
    [SerializeField] private bool _initiallySlow;
    //the animator for the metronome
    [SerializeField]
    private Animator _anim;
    //the animation clip for the weight moving up and down (currently not assigned)
    [SerializeField] private AnimationClip _change;
    //the number of times you want the circle to flash on the HUD
    [SerializeField] private int _howManyFlashes;

    [Header("Speed Settings")]
    [SerializeField]
    private float _fastSpeed = 2;
    [SerializeField]
    private float _slowSpeed = 1;
    [SerializeField]
    private float _flashSpeed = 0.5f;

    private bool _isSlow = true;
    private static readonly int GoFaster = Animator.StringToHash("GoFaster");

    [SerializeField] private EventReference _slowMetronome;
    [SerializeField] private EventReference _fastMetronome;
    [SerializeField] private EventReference _metronomeChange;
    

    [SerializeField]
    private TMP_Text _metronomePredictor;

    /// <summary>
    /// Keeps the particle effects from playing right away.
    /// </summary>
    private void Awake()
    {
        _contactIndicator.Pause();
        //_anim = GetComponentInParent<Animator>();

        // rotate to always be readable
        _metronomePredictor.rectTransform.forward = Vector3.forward;
        _metronomePredictor.rectTransform.Rotate(Vector3.right * 90);

        _isSlow = _initiallySlow;

        AudioManager.Instance.PlaySound(_metronomeChange);
        
    }

    /// <summary>
    /// Updates metronome predictor text
    /// </summary>
    private void Start()
    {
        if (TimeSignatureManager.Instance != null)
        {
            TimeSignatureManager.Instance.RegisterTimeListener(this);
        }

        Vector2Int nextTimeSig = TimeSignatureManager.Instance.GetNextTimeSignature();
        _metronomePredictor.text = nextTimeSig.x + "/" + nextTimeSig.y;

        if (_hudIndicator == null)
        {
            _hudIndicator = GameObject.Find("TimeSigParticles").transform.GetChild(1).gameObject.GetComponent<ParticleSystem>();
        }
    }

    /// <summary>
    /// Toggles the time signature on the manager if there is one
    /// </summary>
    private void ActivateMetronome()
    {
        if (TimeSignatureManager.Instance != null)
            TimeSignatureManager.Instance.ToggleTimeSignature();

        MetronomeTriggered?.Invoke();

        SetAnimSpeed();
    }

    /// <summary>
    /// Changes the speed of the metronome when toggled.
    /// </summary>
    public void SetAnimSpeed()
    {
        _isSlow = !_isSlow;
        
        if (_isSlow)
        {
            AudioManager.Instance.PlaySound(_fastMetronome);
        }
        if (!_isSlow)
        {
            AudioManager.Instance.PlaySound(_slowMetronome);
        }
        
        _anim.speed = _isSlow ? _slowSpeed : _fastSpeed;
    }

    /// <summary>
    /// Play's the HUD indicator effect on a delay after the player 
    /// touches the metronome (for tutorial level only).
    /// </summary>
    private IEnumerator HudIndicator()
    {
        WaitForSeconds wait = new(_flashSpeed);

        if (_initiallySlow)
        {
            _anim.SetBool(GoFaster, true);
        }
        else
        {
            _anim.SetBool(GoFaster, false);
        }

        //loops 6 times
        for (int i = 0; i < _howManyFlashes; i++)
        {
            if (i % 2 == 0)
            {
                _HUDEffect.SetActive(true);
                yield return wait;
            }
            else
            {
                _HUDEffect.SetActive(false);
                yield return wait;
            }
        }
    }

    /// <summary>
    /// Activates the metronome and stops player movement in response 
    /// to a collision with the player.
    /// </summary>
    /// <param name="other">Data from a collision</param>
    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player") && !other.gameObject.CompareTag("Enemy") && !other.gameObject.CompareTag("SonEnemy"))
        {
            return;
        }

        ActivateMetronome();

        if (_isThisTheTutorial)
        {
            StopAllCoroutines();
            StartCoroutine(HudIndicator());
        }

        _contactIndicator.Play();
        _hudIndicator.Play();
        if (_HUDEffect != null)
        _HUDEffect.SetActive(false);

       /* PlayerMovement playerMovement;
        if (other.gameObject.TryGetComponent<PlayerMovement>(out  playerMovement))
        {
            playerMovement.ForceTurnEnd();
        }

        EnemyBehavior enemyBehavior;
        if (other.gameObject.TryGetComponent<EnemyBehavior>(out enemyBehavior))
        {
            enemyBehavior.ForceTurnEnd();
        }

        MirrorAndCopyBehavior mirrorAndCopyBehavior;
        if (other.gameObject.TryGetComponent<MirrorAndCopyBehavior>(out mirrorAndCopyBehavior))
        {
            mirrorAndCopyBehavior.ForceTurnEnd();
        }*/
    }

    /// <summary>
    /// Implementation of ITimeListener. Updates text indicator
    /// </summary>
    /// <param name="newTimeSignature"></param>
    public void UpdateTimingFromSignature(Vector2Int newTimeSignature)
    {
        if (TimeSignatureManager.Instance == null)
            return;

        Vector2Int nextTimeSig = TimeSignatureManager.Instance.GetNextTimeSignature();
        _metronomePredictor.text = nextTimeSig.x + "/" + nextTimeSig.y;
    }
}
