/******************************************************************
*    Author: Taylor Sims
*    Contributors: Alex Laubenstein, Alec Pizziferro, Trinity Hutson
*    Date Created: 09-24-24
*    Description: This script is the collection system for Notes.
*******************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaintsField;
using FMODUnity;
using UnityEngine.Serialization;

public class Collectibles : MonoBehaviour, IGridEntry
{

    // variables
    [FormerlySerializedAs("destroyGlowEffect")]
    [SerializeField] private DestroyGlowEffect _destroyGlowEffect;
    [MinValue(0), MaxValue(10)]
    [FormerlySerializedAs("_collectableNumber")]
    [SerializeField] private int _collectibleNumber;
    [SerializeField] private EventReference _sound;
    [FormerlySerializedAs("NoteGlow")] [SerializeField] private GameObject _noteGlow;

    public int CollectibleNumber { get => _collectibleNumber; } 

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
        if (other.CompareTag("Player") && WinChecker.Instance.CheckForCollection(_collectibleNumber)) 
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
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(_sound);
        }
        WinChecker.CollectedNote?.Invoke(_collectibleNumber);
        _destroyGlowEffect.DestroyCollectible();
    }

    /// <summary>
    /// Sets up checking what note needs to be collected next for the note's glow effect
    /// </summary>
    public void GlowCheck(int noteCollected)
    {
        if (noteCollected +1 == _collectibleNumber)
        {
            _noteGlow.SetActive(true);
        }
        else
        {
            _noteGlow.SetActive(false);
        }
    }

    public bool IsTransparent => true;
    public bool BlocksHarmonyBeam => false;
    public bool BlocksMovingWall => false;
    public Vector3 Position => transform.position;
    public GameObject EntryObject => gameObject;
    public void SnapToGridSpace()
    {
        Vector3Int cellPos = GridBase.Instance.WorldToCell(transform.position);
        Vector3 worldPos = GridBase.Instance.CellToWorld(cellPos) + CKOffsetsReference.NoteOffset;
        transform.position = worldPos;
    }
}
