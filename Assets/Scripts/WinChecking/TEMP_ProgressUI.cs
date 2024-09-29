/******************************************************************
*    Author: Nick Grinstead
*    Contributors: 
*    Date Created: 9/28/24
*    Description: Temporary script that updates UI to reflect the 
*       player's progress with collecting the correct melody.
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using NaughtyAttributes;

public class TEMP_ProgressUI : MonoBehaviour
{
    [Required] [SerializeField] private TextMeshProUGUI _sequenceUI;
    [Required] [SerializeField] private TextMeshProUGUI _collectedNotesUI;
    [Required] [SerializeField] private GameObject _incorrectMessage;
    [Required] [SerializeField] private GameObject _doorUnlockMessage;
    [SerializeField] private float _messageWaitTime;

    private List<int> _notes;

    private const string BaseSequenceText = "Melody:";
    private const string BaseCollectedText = "Collected Notes:";

    /// <summary>
    /// Initializing values and registering to actions
    /// </summary>
    private void Start()
    {
        if (_messageWaitTime <= 0) { _messageWaitTime = 5f; }

        _sequenceUI.text = BaseSequenceText;
        _collectedNotesUI.text = BaseCollectedText;

        if (WinChecker.Instance != null)
        {
            WinChecker winChecker = WinChecker.Instance;
            _notes = winChecker.TargetNoteSequence;
            foreach (int note in _notes)
            {
                _sequenceUI.text += " " + note;
            }
        }

        WinChecker.CollectedNote += UpdateCollectedNotesText;
        WinChecker.GotCorrectSequence += DisplayDoorUnlockMessage;
        WinChecker.GotWrongSequence += DisplayIncorrectMessage;
    }

    /// <summary>
    /// Unregistering from actions
    /// </summary>
    private void OnDisable()
    {
        WinChecker.CollectedNote -= UpdateCollectedNotesText;
        WinChecker.GotCorrectSequence -= DisplayDoorUnlockMessage;
        WinChecker.GotWrongSequence -= DisplayIncorrectMessage;
    }

    /// <summary>
    /// Display newly collected note in UI onces collected
    /// </summary>
    /// <param name="collectedNote">new note as int</param>
    private void UpdateCollectedNotesText(int collectedNote)
    {
        _collectedNotesUI.text += " " + collectedNote;
    }

    /// <summary>
    /// Returns collected notes UI to its default text
    /// </summary>
    private void ResetCollectedNotesText()
    {
        _collectedNotesUI.text = BaseCollectedText;
    }

    /// <summary>
    /// Invoked to display the wrong sequence screen
    /// </summary>
    private void DisplayIncorrectMessage()
    {
        ResetCollectedNotesText();
        StopAllCoroutines();
        _doorUnlockMessage.SetActive(false);
        _incorrectMessage.SetActive(false);
        StartCoroutine(IncorrectMessageResetTimer(_incorrectMessage));
    }

    /// <summary>
    /// Displays message that door has been unlocked
    /// </summary>
    private void DisplayDoorUnlockMessage()
    {
        _doorUnlockMessage.SetActive(true);
    }

    /// <summary>
    /// Displays a screen or message for a short time before resetting scene
    /// </summary>
    /// <param name="message">UI to toggle on</param>
    /// <returns>Waits for seconds based on message wait time</returns>
    private IEnumerator IncorrectMessageResetTimer(GameObject message)
    {
        message.SetActive(true);

        yield return new WaitForSeconds(_messageWaitTime);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
