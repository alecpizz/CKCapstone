using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: Trinity
/// Description: Controls the movement of rotational environmental objects
/// </summary>
public class RotationalObject : AbstractEnvironmentalObject
{
    [SerializeField]
    int rotationAmt = 90;

    [SerializeField]
    bool reverse = false;

    [SerializeField]
    float moveLength = 0.2f;

    protected override void Move()
    {
        Rotate();
    }

    /// <summary>
    /// Rotates the object in the given direction
    /// </summary>
    private void Rotate()
    {
        Vector3 rotationToAdd = new(0, rotationAmt, 0);
        int direction = reverse ? -1 : 1;

        rotationToAdd *= direction;

        StartCoroutine(RotateOverTime(Quaternion.Euler(rotationToAdd + transform.rotation.eulerAngles), moveLength));
    }

    /// <summary>
    /// Rotates the object over the course of moveTime to the target rotation
    /// </summary>
    /// <param name="targetRotation"></param>
    /// <param name="moveTime"></param>
    /// <returns></returns>
    private IEnumerator RotateOverTime(Quaternion targetRotation, float moveTime)
    {
        //Moves the character from one position to another
        float time = 0;
        Quaternion startRotation = transform.rotation;
        while (time < 1)
        {
            time += Time.deltaTime / moveTime;
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, time);
            yield return new WaitForEndOfFrame();
        }

        AssignAllTilesBeneath(GridAvailability.Occupied);
    }
}
