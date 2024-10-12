/******************************************************************
*    Author: Claire Noto
*    Contributors: Claire Noto
*    Date Created: 10/10/24
*    Description: Script that handles the harmony
*******************************************************************/
using System.Collections.Generic;
using UnityEngine;

public class HarmonyBeam : MonoBehaviour
{
    private float _laserDistance = 50f;
    private LineRenderer _lineRenderer;
    private List<GameObject> _previouslyHitEnemies = new();

    void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    void FixedUpdate()
    {
        ShootLaser(transform.position, transform.forward, _laserDistance);
    }

    /// <summary>
    /// Shoots the line renderer and raycast.
    /// </summary>
    /// <param name="startPosition">position the laser starts from</param>
    /// <param name="direction">direction the laser is going</param>
    /// <param name="distance">distance the laser will go</param>
    public void ShootLaser(Vector3 startPosition, Vector3 direction, float distance)
    {
        _lineRenderer.positionCount = 2;  // Ensure the line renderer has 2 points (start and end)
        _lineRenderer.SetPosition(0, startPosition);  // Starting point of the laser

        RaycastHit[] hits = Physics.RaycastAll(startPosition, direction, distance);
        System.Array.Sort(hits, (hit1, hit2) => hit1.distance.CompareTo(hit2.distance));  // Sort hits by distance

        Vector3 laserEndPoint = startPosition + direction * distance;  // Default endpoint of the laser
        List<GameObject> currentlyHitEnemies = new();

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                GameObject enemy = hit.collider.gameObject;
                Debug.Log("Froze enemy");

                // Add to the currently hit list
                currentlyHitEnemies.Add(enemy);
            }
            else if (hit.collider.CompareTag("Reflective"))
            {
                // Reflect the laser using the ReflectiveObject's Reflect method
                var reflectiveObject = hit.collider.GetComponent<ReflectiveObject>();
                if (reflectiveObject)
                {
                    reflectiveObject.Reflect(direction, distance - hit.distance);
                }

                // Stop the main laser at the reflective object
                laserEndPoint = hit.point;
                break;
            }
            else
            {
                // Stop the laser at a non-enemy, non-reflective object
                laserEndPoint = hit.point;
                break;
            }
        }

        _lineRenderer.SetPosition(1, laserEndPoint);

        // Unfreeze any enemies that were previously hit but not hit now
        foreach (GameObject enemy in _previouslyHitEnemies)
        {
            if (!currentlyHitEnemies.Contains(enemy))
            {
                Debug.Log("Unfroze enemy");
            }
        }

        // Update the previously hit enemies list
        _previouslyHitEnemies.Clear();
        _previouslyHitEnemies.AddRange(currentlyHitEnemies);
    }
}
