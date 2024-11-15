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
    ParticleSystem _particles;
    [SerializeField]
    Camera _uiCamera;
    [Space]
    [SerializeField]
    Transform _emissionTransform;
    [SerializeField]
    Transform _targetTransform;
    [Space]
    [SerializeField]
    RectTransform _targetRectTransform;
    [SerializeField]
    float _camDepth = 6;

    [Header("Emitter")]
    [SerializeField]
    float _emissionDuration = 0.6f;
    [SerializeField]
    float _emissionRate = 20;

    [Header("Force")]
    [SerializeField]
    float _forceSpeed = 10;
    [SerializeField]
    float _forceDelay = 1;
    [SerializeField]
    float _forceDuration = 1;

    WaitForSeconds _waitDelay;

    /// <summary>
    /// Initializes the particle position, and particleSystem settings
    /// </summary>
    private void Awake()
    {
        Transform mainCameraTrans = Camera.main.transform;
        _uiCamera.transform.SetPositionAndRotation(mainCameraTrans.position, mainCameraTrans.rotation);

        _particles.transform.position = _emissionTransform.position;

        var emission = _particles.emission;
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(_emissionRate, _emissionRate);
        var main = _particles.main;
        main.duration = _emissionDuration;

        _waitDelay = new WaitForSeconds(_forceDelay);
    }

    /// <summary>
    /// Plays the particles and sends them from the emission transform to the target transform
    /// </summary>

    [Button("Play Particles (Runtime Only)")]
    public void PlayParticles()
    {
        StartCoroutine(ParticleSequence());
    }

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
    private IEnumerator ParticleSequence()
    {
        _particles.Play();

        ParticleSystem.Particle[] particleArr = new ParticleSystem.Particle[_particles.main.maxParticles];

        for (float step = 0; step < _forceDuration; step += Time.deltaTime)
        {
            _particles.GetParticles(particleArr);

            for (int i = 0; i < particleArr.Length; i++)
            {
                // Ignore if particle is dead or if the particle hasn't lived longer than the force delay
                if (particleArr[i].remainingLifetime <= 0 || 
                    particleArr[i].remainingLifetime > particleArr[i].startLifetime - _forceDelay)
                    continue;
                // Delete self upon colliding with target
                else if (particleArr[i].position == _targetTransform.position)
                {
                    particleArr[i].remainingLifetime = 0;
                    continue;
                }

                // Particles don't have a transform so can't tween :(
                // Have to reset velocity and position each time or else they wander off
                particleArr[i].velocity = Vector3.zero;
                particleArr[i].position = Vector3.MoveTowards(particleArr[i].position, _targetTransform.position, _forceSpeed);
            }

            _particles.SetParticles(particleArr);

            yield return null;
        }

    }

    private IEnumerator ParticleUISequence()
    {
        _particles.Play();

        ParticleSystem.Particle[] particleArr = new ParticleSystem.Particle[_particles.main.maxParticles];

        Vector3 uiPosition = _targetRectTransform.position;
        uiPosition.y = _camDepth;

        Vector3 uiTarget = _uiCamera.ScreenToWorldPoint(uiPosition, Camera.MonoOrStereoscopicEye.Mono);
        print("UI Target: " + uiTarget);

        Vector3 particleVelocity = uiTarget - _emissionTransform.position;
        particleVelocity = _forceSpeed * particleVelocity.normalized;

        yield return _waitDelay;

        _particles.GetParticles(particleArr);

        for (int i = 0; i < particleArr.Length; i++)
        {
            particleArr[i].velocity = particleVelocity;
        }

    }
}
