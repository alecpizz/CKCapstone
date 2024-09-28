using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TEMP_ProgressUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _sequenceUI;
    [SerializeField] private TextMeshProUGUI _collectedNotesUI;

    private List<int> _notes;

    private const string BaseSequenceText = "Melody:";
    private const string BaseCollectedText = "Collected Notes:";

    private void Start()
    {
        _sequenceUI.text = BaseSequenceText;
        _collectedNotesUI.text = BaseCollectedText;

        WinChecker.CollectedNote += UpdateCollectedNotesText;

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
    }

    private void UpdateCollectedNotesText(int collectedNote)
    {
        _collectedNotesUI.text += " " + collectedNote;
    }

    private void ResetCollectedNotesText()
    {
        _collectedNotesUI.text = BaseCollectedText;
    }
}
