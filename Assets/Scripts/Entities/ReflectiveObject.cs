/******************************************************************
*    Author: Claire Noto
*    Contributors: Claire Noto
*    Date Created: 10/12/24
*    Description: Script that handles harmony beam reflections
*******************************************************************/
using UnityEngine;

public class ReflectiveObject : MonoBehaviour, IHarmonyBeamEntity, ITurnListener
{
    public bool AllowLaserPassThrough => true;
    public bool HitWrapAround => false;
    public Vector3 Position => transform.position;

    public TurnState TurnState => TurnState.Player;

    private HarmonyBeam _harmonyBeam;

    private bool _isBeingHitByBeam = false;
    private int _scansPerformed = 0;
    private const int _maxScansPerRound = 3;

    /// <summary>
    /// Sets up references to harmony beam attached to this object
    /// </summary>
    private void Start()
    {
        _harmonyBeam = GetComponent<HarmonyBeam>();
        _harmonyBeam.ToggleBeam(false);

        RoundManager.Instance.RegisterListener(this);
    }

    /// <summary>
    /// Unregisters from the round manager
    /// </summary>
    private void OnDisable()
    {
        RoundManager.Instance.UnRegisterListener(this);
    }

    /// <summary>
    /// Returns the new direction for the beam after reflecting
    /// </summary>
    /// <param name="incomingDirection">direction the original laser is coming from</param>
    public Vector3 GetReflectionDirection()
    {
        return transform.forward;
    }

    /// <summary>
    /// Enables the reflective HarmonyBeam instance.
    /// </summary>
    public void ToggleBeam(bool toggle)
    {
        _harmonyBeam.ToggleBeam(toggle);
    }

    /// <summary>
    /// When this object is hit by a laser, turn on the beam, and check for objects.
    /// </summary>
    public void OnLaserHit()
    {
        _isBeingHitByBeam = true;
        _harmonyBeam.ToggleBeam(true);

        if (_scansPerformed < _maxScansPerRound)
        {
            _scansPerformed++;
            _harmonyBeam.ScanForObjects();
        }
    }

    /// <summary>
    /// When this object is non longer being hit by a laser, turn off the beam
    /// and check for any exited objects.
    /// </summary>
    public void OnLaserExit()
    {
        _isBeingHitByBeam = false;
        _harmonyBeam.ToggleBeam(false);

        if (_scansPerformed < _maxScansPerRound)
        {
            _scansPerformed++;
            _harmonyBeam.ScanForObjects();
        }
    }

    /// <summary>
    /// Resets this reflector's ability to scan for objects again
    /// </summary>
    /// <param name="direction">Direction of the player's movement</param>
    public void BeginTurn(Vector3 direction)
    {
        _scansPerformed = 0;
        RoundManager.Instance.CompleteTurn(this);
    }

    /// <summary>
    /// Resets this reflector's ability to scan for objects again
    /// </summary>
    public void ForceTurnEnd()
    {
        _scansPerformed = 0;
        RoundManager.Instance.CompleteTurn(this);
    }

    /// <summary>
    /// Used to reactivate the beam after the reflector rotates
    /// </summary>
    public void CheckForBeamPostRotation()
    {
        if (_isBeingHitByBeam)
        {
            _harmonyBeam.ToggleBeam(true);
        }
    }
}