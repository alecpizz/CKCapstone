/******************************************************************
*    Author: Nick Grinstead
*    Contributors:
*    Date Created: 9/24/24
*    Description: Tracks what notes the player has collected and
*       handles checking if the player has the correct sequence.
*******************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class WinChecker : MonoBehaviour
{
    public static WinChecker Instance { get; private set; }

    public static Action<int> CollectedNote;
    public static Action GotCorrectSequence;
    public static Action GotWrongSequence;

    [SerializeField] private ParticleSystem _unlockedParticles;

    [field: SerializeField] public List<int> TargetNoteSequence { get; private set; } = new List<int>();

    [InfoBox("For visualization purposes, don't edit this",
        EInfoBoxType.Normal)]
    [SerializeField] private List<int> _collectedSequence = new List<int>();

    /// <summary>
    /// Called by collectables to determine if they are able to be picked up
    /// </summary>
    /// <param name="noteToCollect">int representing note of collectable</param>
    /// <returns>true if note matches next one in sequence</returns>
    public bool CheckForCollection(int noteToCollect)
    {
        if (TargetNoteSequence.Count == 0)
            return true;

        if (_collectedSequence.Count < TargetNoteSequence.Count)
        {
            return TargetNoteSequence[_collectedSequence.Count] == noteToCollect;
        }

        return false;
    }

    /// <summary>
    /// Establish singleton
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }

        Instance = this;
    }

    /// <summary>
    /// Registering to action
    /// </summary>
    private void Start()
    {
        CollectedNote += CollectNote;
    }

    /// <summary>
    /// Unregistering from action
    /// </summary>
    private void OnDisable()
    {
        CollectedNote -= CollectedNote;
    }

    /// <summary>
    /// Adds collected note to List then compares it to target sequence if both
    ///     are the same length
    /// </summary>
    /// <param name="note">int representation of note collected</param>
    private void CollectNote(int note)
    {
        _collectedSequence.Add(note);

        if (_collectedSequence.Count == TargetNoteSequence.Count)
        {
            bool doesSequenceMatch = true;

            for (int i = 0; i < _collectedSequence.Count && i < TargetNoteSequence.Count; ++i)
            {
                if (_collectedSequence[i] != TargetNoteSequence[i])
                {
                    doesSequenceMatch = false;
                    break;
                }
            }

            if (doesSequenceMatch)
            {
                Debug.Log("Correct Sequence");
                GotCorrectSequence?.Invoke();
            }
            else
            {
                ClearCollectedSequence();
                Debug.Log("Wrong Sequence");
                GotWrongSequence?.Invoke();
            }
        }
    }

    /// <summary>
    /// Clears list of notes player has collected
    /// </summary>
    [Button]
    private void ClearCollectedSequence()
    {
        _collectedSequence.Clear();
    }

#if UNITY_EDITOR
    // The following three methods are used for testing in editor

    [Button]
    private void TestCollectNoteZero()
    {
        CollectedNote(0);
    }

    [Button]
    private void TestCollectNoteOne()
    {
        CollectedNote(1);
    }

    [Button]
    private void TestCollectNoteTwo()
    {
        CollectedNote(2);
    }

    [Button]
    private void TestCollectNoteThree()
    {
        CollectedNote(3);
    }
#endif
}