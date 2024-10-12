/******************************************************************
*    Author: Claire Noto
*    Contributors: Claire Noto
*    Date Created: 10/12/24
*    Description: Script that handles harmony beam reflections
*******************************************************************/

using UnityEngine;

public class ReflectiveObject : MonoBehaviour
{
    [SerializeField] private bool _reflectLeft = true;

    private HarmonyBeam _harmonyBeam;
    private LineRenderer _lineRenderer;
    private bool _isHit = false;

    void Start()
    {
        _harmonyBeam = GetComponent<HarmonyBeam>();
        _lineRenderer = GetComponent<LineRenderer>();

        if (_harmonyBeam != null)
        {
            _harmonyBeam.enabled = false;
        }

        if (_lineRenderer != null)
        {
            _lineRenderer.enabled = false;
        }
    }

    void FixedUpdate()
    {
        // If the object is not hit, disable the HarmonyBeam and LineRenderer
        if (!_isHit && _harmonyBeam != null && _harmonyBeam.enabled)
        {
            _harmonyBeam.enabled = false;
            if (_lineRenderer != null)
            {
                _lineRenderer.enabled = false;
            }
        }

        _isHit = false;
    }

    /// <summary>
    /// Reflects the harmony beam laser in the specified direction
    /// </summary>
    /// <param name="incomingDirection">direction the original laser is coming from</param>
    /// <param name="remainingDistance">distance the beam can go</param>
    public void Reflect(Vector3 incomingDirection, float remainingDistance)
    {
        _isHit = true;

        if (_harmonyBeam != null && !_harmonyBeam.enabled)
        {
            Vector3 newDirection;
            if (_reflectLeft)
            {
                newDirection = Quaternion.Euler(0, -90, 0) * incomingDirection;  // Rotate -90 degrees (left)
            }
            else
            {
                newDirection = Quaternion.Euler(0, 90, 0) * incomingDirection;   // Rotate 90 degrees (right)
            }

            transform.forward = newDirection;

            _harmonyBeam.enabled = true;
            if (_lineRenderer != null)
            {
                _lineRenderer.enabled = true;
            }

            _harmonyBeam.ShootLaser(transform.position, newDirection, remainingDistance);
        }
    }
}
