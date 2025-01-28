/******************************************************************
*    Author: Taylor Sims
*    Contributors: None
*    Date Created: 10-13-24
*    Description: This script destroys the notes in a glow effect.
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class DestroyGlowEffect : MonoBehaviour
{
    // variables
    [FormerlySerializedAs("glowEffectPrefab")]
    [SerializeField] private GameObject _glowEffectPrefab;

    /// <summary>
    /// This method is called the player collides 
    /// with a collectible. It shows a glow effect 
    /// and destroys the object.
    /// </summary>
    public void DestroyCollectible()
    {
        if (_glowEffectPrefab != null)
        {
            Instantiate(_glowEffectPrefab, transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }
}
