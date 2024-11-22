/******************************************************************
*    Author: Mitchell Young
*    Contributors: Mitchell Young
*    Date Created: 11/17/24
*    Description: Script that casts a ray from the mouse to check
*    for set path enemies (turns on/off destination point visuals
*    on collision).
*******************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using PrimeTween;
using Unity.VisualScripting;
using FMODUnity;

public class MouseManagerBehavior : MonoBehaviour
{
    private Ray _ray;
    private RaycastHit hit;
    private GameObject lastEnemyHit;
    private EnemyBehavior lastEnemyBehavior;
    private Camera _cam;

    /// <summary>
    /// Sets the lastEnemyHit and lastEnemyBehavior values to null.
    /// Sets _cam to the main camera.
    /// </summary>
    // Start is called before the first frame update
    private void Start()
    {
        lastEnemyHit = null;
        lastEnemyBehavior = null;

        _cam = Camera.main;
    }

    /// <summary>
    /// Updates the ray's position to the current mouse position every frame.
    /// Checks for enemy collision with Physics.Raycast.
    /// </summary>
    // Update is called once per frame
    private void Update()
    {
        _ray = _cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        //If the raycast hits the enemy object it's marker with change alpha value calling the ChangeMarkerColor function
        if (Physics.Raycast(_ray, out hit) && hit.collider.gameObject.TryGetComponent<EnemyBehavior>(out EnemyBehavior enemyBehavior))
        {
            lastEnemyHit = hit.collider.gameObject;
            lastEnemyBehavior = enemyBehavior;
            lastEnemyBehavior.CollidingWithRay = true;
            lastEnemyBehavior.DestinationPath();
        }
        else
        {
            if (lastEnemyHit != null)
            {
                lastEnemyBehavior.CollidingWithRay = false;
                lastEnemyBehavior.DestinationPath();
            }
        }
    }
}
