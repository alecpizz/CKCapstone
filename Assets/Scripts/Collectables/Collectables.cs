/******************************************************************
*    Author: Taylor Sims
*    Contributors: Alex Laubenstein
*    Date Created: 09-24-24
*    Description: This script is the collection system for Notes.
*******************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaintsField;
using FMODUnity;

public class Collectables : MonoBehaviour
{
    public static Collectables Instance { get; private set; }

    // variables
    public DestroyGlowEffect destroyGlowEffect;
    [MinValue(0), MaxValue(10)]
    [SerializeField] private int _collectableNumber;
    [SerializeField] private EventReference _sound;
    [SerializeField] private GameObject NoteGlow;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        WinChecker.CollectedNote += GlowCheck;
        GlowCheck(-1);
    }

    private void OnDisable()
    {
        WinChecker.CollectedNote -= GlowCheck;
    }

    /// <summary>
    /// This method triggers the Collect method when the
    /// player collides with a collectable.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && WinChecker.Instance.CheckForCollection(_collectableNumber)) 
        {
            Collect();
        }        
    }

    /// <summary>
    /// This method debugs when the player collects
    /// a collectable and checks that the player
    /// has won by collecting the right sequence of Notes. 
    /// Once the player collects a note, the note disappears
    /// in a glow effect. 
    /// </summary>
    private void Collect() 
    {
        Debug.Log("You got a collectable!");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(_sound);
        }
        WinChecker.CollectedNote?.Invoke(_collectableNumber);
        destroyGlowEffect.DestroyCollectible();
    }

    /// <summary>
    /// Sets up checking what note needs to be collected next for the note's glow effect
    /// </summary>
    public void GlowCheck(int noteCollected)
    {
        if (noteCollected +1 == _collectableNumber)
        {
            NoteGlow.SetActive(true);
        }
        else
        {
            NoteGlow.SetActive(false);
        }
    }
}
