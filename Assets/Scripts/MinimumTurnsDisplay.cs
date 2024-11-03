/******************************************************************
*    Author: Josephine Qualls
*    Contributors: Trinity Hutson
*    Date Created: 10/31/24
*    Description: Tracks the number of turns a player makes and displays it.
*    Also displays minimum number of turns is needed for a level.
*******************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Updates the display counters for minimum turns and player turns
/// </summary>
public class MinimumTurnsDisplay : MonoBehaviour
{
    //designers can assign the minimum turn number they want for their level
    [SerializeField] private int _minimumTurns;

    //assign the text refering to minimum turns and player turns
    [SerializeField] private TMP_Text _minTurnsText;
    [SerializeField] private TMP_Text _playerCounter;

    //listener for player turn completion in PlayerMovement.cs
    public int TurnsMade {  get; private set; }

    /// <summary>
    /// displays the minimum turns needed for level completion
    /// </summary>
    private void Start()
    {
        _minTurnsText.text = _minimumTurns.ToString();
    }

    /// <summary>
    /// Updates number of turns player makes and displays new number
    /// </summary>
    public void TurnIncrementation()
    {
        TurnsMade++;
        TurnText();
    }

    /// <summary>
    /// Converts counter number to string
    /// </summary>
    private void TurnText()
    {
        _playerCounter.text = TurnsMade.ToString();
    }
}
