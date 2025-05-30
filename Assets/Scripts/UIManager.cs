/******************************************************************
 *    Author: Nick Grinstead
 *    Contributors:  Rider Hagen, Alec Pizziferro, Josephine Qualls, Trinity Hutson
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
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.UI;
using PrimeTween;

public class UIManager : MonoBehaviour, ITimeListener
{
    [FormerlySerializedAs("_collectedNotesUI")] [Required] [SerializeField]
    private TextMeshProUGUI _collectedNotesUi;

    [SerializeField] private float _messageWaitTime;
    [SerializeField] private GameObject _doorUnlockMessage;
    [SerializeField] private GameObject _incorrectMessage;

    [FormerlySerializedAs("_sequenceUI")] [SerializeField]
    private TextMeshProUGUI _sequenceUi;

    [SerializeField] private bool _isIntermission;
    [SerializeField] private bool _isChallenge;

    [Space]
    [SerializeField] private NotesUI _notesUI;
    [SerializeField] private GameObject _timeSigNotesUIPrefab;
    [Space]

    private TimeSignatureManager _timeSigManager;

    private List<int> _notes;

    private const string BaseCollectedText = "Collect the notes in numerical order:";
    [SerializeField] private string _levelText = "Level";
    [SerializeField] private string _challengeText = "Challenge";
    [SerializeField] private string _intermissionText = "Intermission";

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
            WinChecker.CollectedNote += UpdateGhostNotesIcon;
            foreach (int note in _notes)
            {
                _sequenceUi.text += " " + note;
                if (!_isIntermission)
                {
                    UpdateGhostNotesIcon(note);
                }
            }
        }

        _timeSigManager = TimeSignatureManager.Instance;

        if (_timeSigManager != null)
        {
            _timeSigManager.RegisterTimeListener(this);

            InitializeTimeSigHud();
        }

        // WinChecker.CollectedNote += UpdateCollectedNotesText;
        WinChecker.CollectedNote += UpdateColectedNotesIcons;
        WinChecker.GotCorrectSequence += DisplayDoorUnlockMessage;
        WinChecker.GotWrongSequence += DisplayIncorrectMessage;


        //Currently saving old if statement to potentially use/repurpose for Trinity's revisions

        /*if (LevelOrderSelection.Instance.SelectedLevelData.PrettySceneNames.Count > 0)
        {
            int index = SceneManager.GetActiveScene().buildIndex;
            var prettyName = LevelOrderSelection.Instance.SelectedLevelData.PrettySceneNames[index].PrettyName;
            _levelNumber.text = prettyName; //Change to reflect what Trinity wants
        }
        else
        {
            //legacy level ordering
            if (_isChallenge)
            {
                _levelNumber.text = "Challenge";
                Debug.Log("it's challenging");
            }
            else
            {
                _levelNumber.text = $"{LevelText} {SceneManager.GetActiveScene().buildIndex}";
                Debug.Log("it's a level");
            }
                
        }*/
    }

    public void SetLevelText(string text)
    {
        _notesUI.SetLevelText(text);
    }

    /// <summary>
    /// Assigns the proper level name to the time signature
    /// </summary>
    private void LvlDictUpdate()
    {
        // int index = SceneManager.GetActiveScene().buildIndex;
        //
        // //Gets the path to a scene
        // string path = SceneUtility.GetScenePathByBuildIndex(index);
        // //Uses the path to get the full name of the scene in the build
        // string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
        //
        // //Assigns the name of scenes to the time signature in a level
        // if (_levelNumber == null) return;
        // if (_isChallenge)
        // {
        //     //Challenges are currently Challenge + their level number
        //     _levelNumber.text = _challengeText;
        // }
        // else
        // {
        //     //Levels are Level + lvl number
        //     if (sceneName[0] == 'I')
        //     {
        //         _levelNumber.text = _intermissionText;
        //     }
        //     else
        //     {
        //         _levelNumber.text = $"{_levelText} {_levelButtons.GetLvlCounter(index)}";
        //     }
        // }
    }

    public void UpdateTimingFromSignature(Vector2Int newTimeSignature)
    {
        _notesUI.UpdateTimingFromSignature(newTimeSignature, true);
    }

    /// <summary>
    /// Unregistering from actions
    /// </summary>
    private void OnDisable()
    {
        // WinChecker.CollectedNote -= UpdateCollectedNotesText;
        WinChecker.CollectedNote -= UpdateColectedNotesIcons;
        WinChecker.CollectedNote -= UpdateGhostNotesIcon;
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
        _notesUI.UpdateCollectedNoteIcon(collectedNote);
    }

    /// <summary>
    /// Display ghost notes that need to be collected in UI once a note is collected
    /// </summary>
    private void UpdateGhostNotesIcon(int collectedNote)
    {
        _notesUI.UpdateGhostNoteIcon(collectedNote);
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

    /// <summary>
    /// Updates HUD to reflect whether or not the Time Sig is in use for the level. Should be called once per level, in Start
    /// </summary>
    private void InitializeTimeSigHud()
    {
        if (_timeSigManager == null || !_timeSigManager.TimeSigInUse)
            return;

        GameObject uiObj = Instantiate(_timeSigNotesUIPrefab, transform);
        uiObj.transform.SetAsFirstSibling();

        if(!uiObj.TryGetComponent(out NotesUI notesUI))
        {
            Debug.LogError("TimeSigNotesUI Prefab is missing NotesUI script!");
            return;
        }

        string levelNumber = _notesUI.LevelNumber.text;

        notesUI.LevelNumber.text = levelNumber;

        if (!_timeSigManager.MetronomeInUse)
            notesUI.ToggleArrow(false);

        Destroy(_notesUI.gameObject);

        _notesUI = notesUI;

        foreach (int note in _notes)
        {
            if (!_isIntermission)
                UpdateGhostNotesIcon(note);
        }

        _notesUI.UpdateTimingFromSignature(_timeSigManager.GetCurrentTimeSignature(), false);
    }
}