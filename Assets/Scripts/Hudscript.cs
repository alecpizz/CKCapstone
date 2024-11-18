/******************************************************************
*    Author: Nick Grinstead 
*    Contributors:  Rider Hagen
*    Date Created: 9/28/24
*    Description: Temporary script that updates UI to reflect the 
*       player's progress with collecting the correct melody.
*       (Copied then edited from TEMP_ProgressUI which was 
*       originally written by Nick)
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using SaintsField;
using Unity.VisualScripting;

public class HUDscript : MonoBehaviour, ITimeListener
{
    [Required][SerializeField] private TextMeshProUGUI _collectedNotesUI;
    [SerializeField] private float _messageWaitTime;
    [SerializeField] private GameObject _doorUnlockMessage;
    [SerializeField] private GameObject _incorrectMessage;
    [SerializeField] private TextMeshProUGUI _sequenceUI;
    [SerializeField] private GameObject[] _noteImages;
    [SerializeField] private GameObject[] _ghostNoteImages;
    [SerializeField] private TextMeshProUGUI _timeSignatureUIy;
    [SerializeField] private TextMeshProUGUI _timeSignatureUIx;
    private TimeSignatureManager _timeSigManager;
    [SerializeField] private bool timeSignature;

    private List<int> _notes;

    private const string BaseCollectedText = "Collect the notes in numerical order:";

    /// <summary>
    /// Initializing values and registering to actions
    /// </summary>
    private void Start()
    {
        if (_messageWaitTime <= 0) { _messageWaitTime = 5f; }

        _collectedNotesUI.text = BaseCollectedText;

        if (WinChecker.Instance != null)
        {
            WinChecker winChecker = WinChecker.Instance;
            _notes = winChecker.TargetNoteSequence;
            foreach (int note in _notes)
            {
                _sequenceUI.text += " " + note;
            }

            for (int i = 0; i < winChecker.TargetNoteSequence.Count; i++)
            {
                _ghostNoteImages[i].SetActive(true);
            }
        }

        _timeSigManager = TimeSignatureManager.Instance;

        if (_timeSigManager != null)
        {
            _timeSigManager.RegisterTimeListener(this);
        }

        // WinChecker.CollectedNote += UpdateCollectedNotesText;
        WinChecker.CollectedNote += UpdateColectedNotesIcons;
        WinChecker.CollectedNote += UpdateGhostNotesIcons;
        WinChecker.GotCorrectSequence += DisplayDoorUnlockMessage;
        WinChecker.GotWrongSequence += DisplayIncorrectMessage;
    }

    public void UpdateTimingFromSignature(Vector2Int newTimeSignature)
    {
        if (_timeSignatureUIx == null || _timeSignatureUIy == null)
        {
            Debug.LogWarning("Missing hud elements");
            return;
        }
        _timeSignatureUIy.text = newTimeSignature.y.ToString();
        _timeSignatureUIx.text = newTimeSignature.x.ToString();
    }

    /// <summary>
    /// Unregistering from actions
    /// </summary>
    private void OnDisable()
    {
        // WinChecker.CollectedNote -= UpdateCollectedNotesText;
        WinChecker.CollectedNote -= UpdateColectedNotesIcons;
        WinChecker.CollectedNote -= UpdateGhostNotesIcons;
        WinChecker.GotCorrectSequence -= DisplayDoorUnlockMessage;
        WinChecker.GotWrongSequence -= DisplayIncorrectMessage;

        if (_timeSigManager != null)
        {
            _timeSigManager.UnregisterTimeListener(this);
        }
    }

    /// <summary>
    /// Display newly collected note in UI onces collected, image edition
    /// </summary>
    private void UpdateColectedNotesIcons(int collectedNote)
    {
        if (collectedNote < 0 || collectedNote > _noteImages.Length - 1) return;
        _noteImages[collectedNote].SetActive(true);
    }

    /// <summary>
    /// Display ghost notes that need to be collected in UI once a note is collected
    /// </summary>
    private void UpdateGhostNotesIcons(int collectedNote)
    {
        if (collectedNote < 0 || collectedNote > _noteImages.Length - 1) return;
        _ghostNoteImages[collectedNote].SetActive(false);
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
        if (_doorUnlockMessage.gameObject.activeSelf == false)
        {
            _incorrectMessage.SetActive(true);
        }
        else
        {
            _incorrectMessage.SetActive(false);
            StartCoroutine(IncorrectMessageResetTimer(_incorrectMessage));
        }
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
