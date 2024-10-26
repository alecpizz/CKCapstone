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

    private void Start()
    {
        _harmonyBeam = GetComponent<HarmonyBeam>();

        // Rotate the cube based on _reflectLeft boolean
        if (_reflectLeft)
        {
            transform.rotation = Quaternion.Euler(0, -90, 0); // Rotate -90 degrees if reflecting left
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 90, 0);  // Rotate 90 degrees if reflecting right
        }

        ToggleBeam();
    }

    /// <summary>
    /// Returns the new direction for the beam after reflecting
    /// </summary>
    /// <param name="incomingDirection">direction the original laser is coming from</param>
    public Vector3 GetReflectionDirection(Vector3 incomingDirection)
    {
        if (_reflectLeft)
        {
            return Quaternion.Euler(0, -90, 0) * incomingDirection;  // Rotate -90 degrees (left)
        }
        else
        {
            return Quaternion.Euler(0, 90, 0) * incomingDirection;   // Rotate 90 degrees (right)
        }
    }

    /// <summary>
    /// Enables the reflective HarmonyBeam instance.
    /// </summary>
    public void ToggleBeam()
    {
        _harmonyBeam.ToggleBeam();
    }

    /// <summary>
    /// Toggles the direction of the reflection
    /// </summary>
    /// <param name="left">true for left, false for right</param>
    public void ChangeDirection(bool left = true)
    {
        _reflectLeft = left;
        if (_reflectLeft)
        {
            transform.rotation = Quaternion.Euler(0, -90, 0); // Rotate -90 degrees (left)
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 90, 0); // Rotate -90 degrees (left)
        }
    }
}