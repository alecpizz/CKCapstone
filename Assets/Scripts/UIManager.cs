/******************************************************************
 *    Author: Nick Grinstead
 *    Contributors:  Rider Hagen, Alec Pizziferro
 *    Date Created: 9/28/24
 *    Description: Script designed to handle all realtime
 *    UI related functionality.
 *******************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using SaintsField;
using Unity.VisualScripting;
using UnityEngine.Serialization;

public class UIManager : MonoBehaviour, ITimeListener
{
    [FormerlySerializedAs("_collectedNotesUI")] [Required] [SerializeField]
    private TextMeshProUGUI _collectedNotesUi;

    [SerializeField] private float _messageWaitTime;
    [SerializeField] private GameObject _doorUnlockMessage;
    [SerializeField] private GameObject _incorrectMessage;

    [FormerlySerializedAs("_sequenceUI")] [SerializeField]
    private TextMeshProUGUI _sequenceUi;

    [SerializeField] private GameObject[] _noteImages;
    [SerializeField] private GameObject[] _ghostNoteImages;
    [SerializeField] private bool _isIntermission;
    [SerializeField] private bool _isChallenge;

    [FormerlySerializedAs("_timeSignatureUIy")] [SerializeField]
    private TextMeshProUGUI _timeSignatureUiY;

    [FormerlySerializedAs("_timeSignatureUIx")] [SerializeField]
    private TextMeshProUGUI _timeSignatureUiX;

    [SerializeField] private TMP_Text _levelNumber;
    private TimeSignatureManager _timeSigManager;

    [FormerlySerializedAs("timeSignature")] [SerializeField]
    private bool _timeSignature;

    private List<int> _notes;

    private const string BaseCollectedText = "Collect the notes in numerical order:";
    private const string LevelText = "Level";

    /// <summary>
    /// Initializing values and registering to actions
    /// </summary>
    private void Start()
    {
        if (_messageWaitTime <= 0)
        {
            _messageWaitTime = 5f;
        }

        _collectedNotesUi.text = BaseCollectedText;

        if (WinChecker.Instance != null)
        {
            WinChecker winChecker = WinChecker.Instance;
            _notes = winChecker.TargetNoteSequence;
            WinChecker.CollectedNote += UpdateGhostNotesIcons;
            foreach (int note in _notes)
            {
                _sequenceUi.text += " " + note;
                if (!_isIntermission)
                {
                    UpdateGhostNotesIcons(note);
                }
            }
        }

        _timeSigManager = TimeSignatureManager.Instance;

        if (_timeSigManager != null)
        {
            _timeSigManager.RegisterTimeListener(this);
        }

        // WinChecker.CollectedNote += UpdateCollectedNotesText;
        WinChecker.CollectedNote += UpdateColectedNotesIcons;
        WinChecker.GotCorrectSequence += DisplayDoorUnlockMessage;
        WinChecker.GotWrongSequence += DisplayIncorrectMessage;

        if (_levelNumber == null) return;
        if (LevelOrderSelection.Instance.SelectedLevelData.PrettySceneNames.Count > 0)
        {
            int index = SceneManager.GetActiveScene().buildIndex;
            var prettyName = LevelOrderSelection.Instance.SelectedLevelData.PrettySceneNames[index].PrettyName;
            _levelNumber.text = "Level " + index; //Change to reflect what Trinity wants
        }
        else
        {
            //legacy level ordering
            if (_isChallenge)
                _levelNumber.text = "Challenge";
            else
                _levelNumber.text = $"{LevelText} {SceneManager.GetActiveScene().buildIndex}";
        }
    }

    public void UpdateTimingFromSignature(Vector2Int newTimeSignature)
    {
        if (_timeSignatureUiX == null || _timeSignatureUiY == null)
        {
            Debug.LogWarning("Missing hud elements");
            return;
        }

        _timeSignatureUiY.text = newTimeSignature.y.ToString();
        _timeSignatureUiX.text = newTimeSignature.x.ToString();
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
        if (collectedNote < 0 || collectedNote > _noteImages.Length - 1)
        {
            return;
        }

        _noteImages[collectedNote].SetActive(true);
        _ghostNoteImages[collectedNote].SetActive(false);
    }

    /// <summary>
    /// Display ghost notes that need to be collected in UI once a note is collected
    /// </summary>
    private void UpdateGhostNotesIcons(int collectedNote)
    {
        if (collectedNote >= 0 && collectedNote < _noteImages.Length)
        {
            _ghostNoteImages[collectedNote].SetActive(true);
        }
    }

    /// <summary>
    /// Returns collected notes UI to its default text
    /// </summary>
    private void ResetCollectedNotesText()
    {
        _collectedNotesUi.text = BaseCollectedText;
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