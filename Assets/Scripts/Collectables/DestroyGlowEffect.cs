/******************************************************************
*    Author: Taylor Sims
*    Contributors: None
*    Date Created: 10-13-24
*    Description: This script destroys the notes in a glow effect.
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyGlowEffect : MonoBehaviour
{
     // variables 
    public GameObject GlowEffectPrefab;

    /// <summary>
    /// This method is called the player collides 
    /// with a collectible. It shows a glow effect 
    /// and destroys the object.
    /// </summary>
    public void DestroyCollectible()
    {
        if (GlowEffectPrefab != null)
        {
            Instantiate(GlowEffectPrefab, transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }
}
