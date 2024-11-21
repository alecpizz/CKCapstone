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
    private Ray ray;
    private RaycastHit hit;
    private GameObject lastEnemyHit;
    private EnemyBehavior lastEnemyBehavior;

    // Start is called before the first frame update
    void Start()
    {
        lastEnemyHit = null;
        lastEnemyBehavior = null;
    }

    // Update is called once per frame
    void Update()
    {
        ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        //If the raycast hits the enemy object it's marker with change alpha value calling the ChangeMarkerColor function
        if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.GetComponent<EnemyBehavior>() != null)
        {
            lastEnemyHit = hit.collider.gameObject;
            lastEnemyBehavior = lastEnemyHit.GetComponent<EnemyBehavior>();
            lastEnemyBehavior.collidingWithRay = true;
            lastEnemyBehavior.DestinationPath();
        }
        else
        {
            if (lastEnemyHit != null)
            {
                lastEnemyBehavior.collidingWithRay = false;
                lastEnemyBehavior.DestinationPath();
            }
        }
    }
}
