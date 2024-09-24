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
    public static Action<int> CollectedNote;
    public static Action GotCorrectSequence;

    [InfoBox("This is the sequence the player must collect notes in, represented by ints", 
        EInfoBoxType.Normal)]
    [SerializeField] private List<int> _targetNoteSequence;

    [InfoBox("For visualization purposes, don't edit this",
        EInfoBoxType.Normal)]
    [SerializeField] private List<int> _collectedSequence = new List<int>();

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

        if (_collectedSequence.Count == _targetNoteSequence.Count)
        {
            bool doesSequenceMatch = true;

            for (int i = 0; i < _collectedSequence.Count && i < _targetNoteSequence.Count; ++i)
            {
                if (_collectedSequence[i] != _targetNoteSequence[i])
                {
                    doesSequenceMatch = false;
                }
            }

            if (doesSequenceMatch)
            {
                GotCorrectSequence?.Invoke();
            }
            else
            {
                ClearCollectedSequence();
                // TODO: implement what happens when player fails sequence
                Debug.Log("Wrong Sequence");
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
#endif
}
