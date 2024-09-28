using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TEMP_ProgressUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _sequenceUI;
    [SerializeField] private TextMeshProUGUI _collectedNotesUI;
    [SerializeField] private GameObject _incorrectMessage;
    [SerializeField] private GameObject _doorUnlockMessage;
    [SerializeField] private float _messageWaitTime;

    private List<int> _notes;

    private const string BaseSequenceText = "Melody:";
    private const string BaseCollectedText = "Collected Notes:";

    private void Start()
    {
        if (_messageWaitTime <= 0) { _messageWaitTime = 5f; }

        _sequenceUI.text = BaseSequenceText;
        _collectedNotesUI.text = BaseCollectedText;

        WinChecker.CollectedNote += UpdateCollectedNotesText;
        WinChecker.GotCorrectSequence += DisplayDoorUnlockMessage;
        WinChecker.GotWrongSequence += DisplayIncorrectMessage;

        WinChecker winChecker = FindObjectOfType<WinChecker>();
        _notes = winChecker.TargetNoteSequence;
        foreach(int note in _notes)
        {
            _sequenceUI.text += " " + note;
        }
    }

    private void OnDisable()
    {
        WinChecker.CollectedNote -= UpdateCollectedNotesText;
        WinChecker.GotCorrectSequence -= DisplayDoorUnlockMessage;
        WinChecker.GotWrongSequence -= DisplayIncorrectMessage;
    }

    private void UpdateCollectedNotesText(int collectedNote)
    {
        _collectedNotesUI.text += " " + collectedNote;
    }

    private void ResetCollectedNotesText()
    {
        _collectedNotesUI.text = BaseCollectedText;
    }

    private void DisplayIncorrectMessage()
    {
        ResetCollectedNotesText();
        StopAllCoroutines();
        _doorUnlockMessage.SetActive(false);
        _incorrectMessage.SetActive(false);
        StartCoroutine(DisplayTimedMessage(_incorrectMessage));
    }

    private void DisplayDoorUnlockMessage()
    {
        _incorrectMessage.SetActive(false);
        _doorUnlockMessage.SetActive(true);
    }

    private IEnumerator DisplayTimedMessage(GameObject message)
    {
        message.SetActive(true);

        yield return new WaitForSeconds(_messageWaitTime);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
