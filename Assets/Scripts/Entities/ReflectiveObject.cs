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
}
