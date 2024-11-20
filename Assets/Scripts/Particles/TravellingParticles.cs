/******************************************************************
 *    Author: Trinity Hutson
 *    Contributors: 
 *    Date Created: 11/14/24
 *    Description: Spawns particles and, after some delay, moves them from the emission point to a target point
 *******************************************************************/

using System.Collections;
using UnityEngine;
using SaintsField;
using SaintsField.Playa;

public class TravellingParticles : MonoBehaviour
{
    [Header("Assignments")]
    [SerializeField]
    private ParticleSystem _particles;
    [SerializeField]
    private Camera _uiCamera;
    [Space]
    [SerializeField]
    private Transform _emissionTransform;
    [SerializeField]
    private RectTransform _targetRectTransform;

    [Header("Screen Space Rendering")]
    [SerializeField]
    private Vector2 _screenSpaceCoordinates;
    [SerializeField]
    private float _camDepth = 6;

    [Header("Force")]
    [SerializeField]
    private float _forceSpeed = 10;
    [SerializeField]
    private float _forceDelay = 1;
    [SerializeField]
    private float _forceDuration = 1;

    private int _emissionAmount = -1;

    private Vector3 _uiTarget;

    private WaitForSeconds _waitDelay;

    /// <summary>
    /// Initializes particle position, target position, UI Camera, wait delay, and emission amount
    /// </summary>
    private void Awake()
    {
        Transform mainCameraTrans = Camera.main.transform;
        _uiCamera.transform.SetPositionAndRotation(mainCameraTrans.position, 
            mainCameraTrans.rotation);

        _uiTarget = _screenSpaceCoordinates;
        _uiTarget.z = _camDepth;

        _particles.transform.position = _emissionTransform.position;

        _waitDelay = new WaitForSeconds(_forceDelay);

        
        _emissionAmount = Mathf.RoundToInt(_particles.emission.rateOverTime.constant * _particles.main.duration);
        print(_emissionAmount);
    }

    /// <summary>
    /// Plays the particles and sends them from the emission transform to the target transform
    /// </summary>
    [Button("Play UI Particles (Runtime Only)")]
    public void PlayUIParticles()
    {
        StartCoroutine(ParticleUISequence());
    }

    /// <summary>
    /// Plays the particles, then iterates over them waiting for them to have lived longer than the delay. 
    /// Once they have reached that point in their lifetime, they are moved towards their target.
    /// When they reach the target, they die.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ParticleUISequence()
    {
        _particles.Play();

        ParticleSystem.Particle[] particleArr = new ParticleSystem.Particle[1];
        
        Vector3 target = _uiCamera.ScreenToWorldPoint(_uiTarget, Camera.MonoOrStereoscopicEye.Mono);

        yield return _waitDelay;

        // Update particle position every frame
        for (float step = 0; step < _forceDuration; step += Time.deltaTime)
        {
            TranslateParticles(particleArr, target);

            yield return null;
        }
    }

    private void TranslateParticles(ParticleSystem.Particle[] particleArr, Vector3 target)
    {
        _particles.GetParticles(particleArr);

        for (int i = 0; i < particleArr.Length; i++)
        {
            // Ignore if particle is dead or if the particle hasn't lived longer than the force delay
            if (particleArr[i].remainingLifetime <= 0 ||
                particleArr[i].remainingLifetime > particleArr[i].startLifetime - _forceDelay)
                continue;
            // Delete self upon colliding with target
            else if (particleArr[i].position == target)
            {
                particleArr[i].remainingLifetime = 0;
                continue;
            }

            // Particles don't have a transform so can't tween :(
            // Have to reset velocity and position each time or else they wander off
            particleArr[i].velocity = Vector3.zero;
            particleArr[i].position = Vector3.MoveTowards(particleArr[i].position, target, _forceSpeed);
        }

        // Updates the particle system to register the changes made
        _particles.SetParticles(particleArr);
    }
}
