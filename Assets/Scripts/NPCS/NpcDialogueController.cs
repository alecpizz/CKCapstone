/******************************************************************
*    Author: David Henvick
*    Contributors: Claire Noto, Alec Pizziferro, Mitchell Young, Jamison Parks, Alex Laubenstein
*    Date Created: 09/30/2024
*    Description: this is the script that is used control an npc 
*    and their dialogue
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System;
using SaintsField;
using System.Diagnostics;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SaintsField.Playa;
using Debug = UnityEngine.Debug;
using UnityEngine.Serialization;
using EventInstance = FMOD.Studio.EventInstance;

public class NpcDialogueController : MonoBehaviour, IInteractable
{
    [SerializeField] private TMP_Text _dialogueBox;
    [SerializeField] private Image _background;
    [SerializeField] private EndLevelDoor[] _doors;
    [SerializeField] private bool isCollectible;
    [SerializeField] public bool isNpc = false;
    [SerializeField] public Image _eKey;
    [SerializeField] public Image _nameBox;
    [SerializeField] public TMP_Text _nameText;
    [SerializeField] public Sprite _keyboardPrompt;
    [SerializeField] public Sprite _playstationButtonPrompt;
    [SerializeField] public Sprite _letterButtonPrompt; //Switch and Xbox sprite
    //Sound for collecting interactable items 
    [SerializeField] private EventReference _onCollected;

    [Serializable]
    public struct DialogueEntry
    {
        [FormerlySerializedAs("_sound")]
        public EventReference sound;
        [FormerlySerializedAs("_text")]
        [TextArea] public string text;
        [InfoBox("This chooses the emotion animation " +
            "There is Neutral, Happy, Sad, and Angry", EMessageType.Info)]
        public EmotionType emotion;
    }

    private bool _isTalking;
    [Header("Typing Speeds")]
    [InfoBox("This adjusts the base typing speed. 2 is the slowest, 10 is the fastest", EMessageType.Info)]
    [Range(2f, 10f)][SerializeField] private float _typingSpeed = 5f;
    [SerializeField] private float _periodTypeDelayMult = 2f;
    [SerializeField] private float _commaTypeDelayMult = 1.33f;
    [SerializeField] private List<DialogueEntry> _dialogueEntries;
    [SerializeField] [TextArea] private string _tutorialHint = "Press E to Talk";
    [SerializeField] private float _dialogueFadeDuration = 0.25f;
    //dialogue options
    private int _currentDialogue = 0;
    private float _currentTypingSpeed;
    //used to tell if player is in adjacent square
    private bool _occupied;
    private Coroutine _typingCoroutine;
    private bool _isTyping = false;
    private string _currentFullText;
    private bool _playerWithinBounds = false;

    private EventInstance _currentInstance;

    private int _totalNpcs = 1;
    private bool _loopedOnce;

    private static readonly int Talk = Animator.StringToHash("Talk");
    private static readonly int Neutral = Animator.StringToHash("Neutral");
    private static readonly int Happy = Animator.StringToHash("Happy");
    private static readonly int Sad = Animator.StringToHash("Sad");
    private static readonly int Angry = Animator.StringToHash("Angry");
    private static readonly int Scared = Animator.StringToHash("Scared");
    [SerializeField] private Animator _animator;

    /// <summary>
    /// Field to retrieve attached GameObject: from IInteractable
    /// </summary>
    public GameObject GetGameObject 
    { 
        get; 
    }

    /// <summary>
    /// This function will be implemented to contain the specific functionality
    /// for an interactable object: from IInteractable
    /// This one is used to call the advance dialogue function
    /// </summary>
    public void OnInteract()
    {
        AdvanceDialogue();
    }

    /// <summary>
    /// This function will be implemented for when the player is no longer interacting with the interactable
    /// this one is used to call the hide dialogue function
    /// from IInteractable
    /// </summary>
    public void OnLeave()
    {
        if (!_playerWithinBounds)
        {
            HideDialogue();
        }
        else
        {
            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
            }
            _dialogueBox.SetText(_tutorialHint);
        }
    }
    
    /// <summary>
    /// Resets the memory, causing the doors to be locked when the scene is loaded.
    /// </summary>
    [Button] 
    public void ResetMemory()
    {
        SaveDataManager.SetNpcProgressionCurrentScene(0);
    }

    /// <summary>
    /// Start is called before the first frame update
    /// used here to grabe the dialogue ui item and to set the occupied variable
    /// </summary>
    private void Start()
    {
        _totalNpcs = FindObjectsOfType<NpcDialogueController>().Length;
        if (CheckForEntries())
        {
            _dialogueBox.SetText(_dialogueEntries[_currentDialogue].text);
        }

        var canvas = _dialogueBox.transform.parent;
        var cam = Camera.main;
        if (cam != null)
        {
            canvas.transform.rotation = cam.transform.rotation;
        }
        _dialogueBox.CrossFadeAlpha(0f, 0f, true);
        _background.CrossFadeAlpha(0f, 0f, true);
        _eKey.CrossFadeAlpha(0f, 0f, true);
        _nameBox.CrossFadeAlpha(0f, 0f, true);
        _nameText.CrossFadeAlpha(0f, 0f, true);
        _occupied = false;
        _isTalking = false;
        _currentTypingSpeed = Mathf.Clamp(
            _typingSpeed, 1f, 10f) / 100f;

        if (SaveDataManager.GetNpcProgressionCurrentScene() >=  _totalNpcs)
        {
            UnlockDoors();
        }
    }

    /// <summary>
    /// is used to advance the current dialogue or show the dialogue if it is not already
    /// is called by player when the interact key is used
    /// </summary>
    public void AdvanceDialogue()
    {
        if (isCollectible && !SaveDataManager.GetCollectableFound(gameObject.name))
        {
            AudioManager.Instance.PlaySound(_onCollected);
            CollectableManager.Instance.Collection(gameObject);
        }

        if (!_isTalking)
        {
            if (_animator != null)
            {
                _animator.SetBool(Talk, true);
                switch (_dialogueEntries[_currentDialogue].emotion)
                {
                    case EmotionType.NEUTRAL:
                        _animator.SetTrigger(Neutral);
                        break;
                    case EmotionType.HAPPY:
                        _animator.SetTrigger(Happy);
                        break;
                    case EmotionType.SAD:
                        _animator.SetTrigger(Sad);
                        break;
                    case EmotionType.ANGRY:
                        _animator.SetTrigger(Angry);
                        break;
                    case EmotionType.SCARED:
                        _animator.SetTrigger(Scared);
                        break;
                }
            }

            if (CheckForEntries())
            {
                if (_typingCoroutine != null)
                {
                    StopCoroutine(_typingCoroutine);
                }
                _typingCoroutine = StartCoroutine(TypeDialogue(_dialogueEntries[_currentDialogue].text));
            }

            VignetteController.InteractionTriggered.Invoke(true);
            _dialogueBox.CrossFadeAlpha(1f, _dialogueFadeDuration, false);
            _background.CrossFadeAlpha(1f, _dialogueFadeDuration, false);
            _eKey.CrossFadeAlpha(1f, _dialogueFadeDuration, false);
            _nameBox.CrossFadeAlpha(1f, _dialogueFadeDuration, false);
            _nameText.CrossFadeAlpha(1f, _dialogueFadeDuration, false);
            _occupied = true;
            _isTalking = true;
        }
        else
        {
            if (!_occupied)
            {
                return;
            }

            if (!CheckForEntries())
            {
                return;
            }

            // skips the typing to show complete text
            if (_isTyping)
            {
                FinishTyping();
                return;
            }

            if (_currentDialogue < _dialogueEntries.Count - 1)
            {
                _currentDialogue++;

                if (!_loopedOnce && _currentDialogue == (_dialogueEntries.Count - 1))
                {
                    int progress = SaveDataManager.GetNpcProgressionCurrentScene() + 1;
                    SaveDataManager.SetNpcProgressionCurrentScene(progress);
                    if (progress >= _totalNpcs)
                    {
                        UnlockDoors();
                    }
                    _loopedOnce = true;
                }
            }
            else
            {
                _currentDialogue = 0;
                HideDialogue();

                return;
            }

            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
            }

            // adjusts typing speed on a per-entry basis
            _currentTypingSpeed = Mathf.Clamp(_typingSpeed, 1f, 10f) / 100f;
            //adjusts emotion on a per-entry basis
            if (_animator != null)
            {
                _animator.ResetTrigger(Neutral);
                _animator.ResetTrigger(Happy);
                _animator.ResetTrigger(Sad);
                _animator.ResetTrigger(Angry);
                _animator.ResetTrigger(Scared);
                switch (_dialogueEntries[_currentDialogue].emotion)
                {
                    case EmotionType.NEUTRAL:
                        _animator.SetTrigger(Neutral);
                        break;
                    case EmotionType.HAPPY:
                        _animator.SetTrigger(Happy);
                        break;
                    case EmotionType.SAD:
                        _animator.SetTrigger(Sad);
                        break;
                    case EmotionType.ANGRY:
                        _animator.SetTrigger(Angry);
                        break;
                    case EmotionType.SCARED:
                        _animator.SetTrigger(Scared);
                        break;
                }
            }
            _typingCoroutine = StartCoroutine(TypeDialogue(_dialogueEntries[_currentDialogue].text));
        }
    }

    /// <summary>
    /// Unlocks the door after NPC dialogue is completed.
    /// </summary>
    private void UnlockDoors()
    {
        if (_doors.Length < 1)
        {
            return;
        }

        foreach (EndLevelDoor door in _doors)
        {
            door.UnlockDoor();
        }
    }

    /// <summary>
    /// use this to hide the dialogue of the npc
    /// </summary>
    public void HideDialogue()
    {
        VignetteController.InteractionTriggered.Invoke(false);
        _dialogueBox.CrossFadeAlpha(0f, _dialogueFadeDuration, false);
        _background.CrossFadeAlpha(0f, _dialogueFadeDuration, false);
        _eKey.CrossFadeAlpha(0f, _dialogueFadeDuration, false);
        _nameBox.CrossFadeAlpha(0f, _dialogueFadeDuration, false);
        _nameText.CrossFadeAlpha(0f, _dialogueFadeDuration, false);
        _occupied = false;

        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }
        _isTalking = false;
        if (_animator != null)
        {
            _animator.SetBool(Talk, false);
        }
    }

    /// <summary>
    /// Invoked when the player enters, displays the dialogue box
    /// for tutorial text.
    /// </summary>
    /// <param name="other">The collider of the player.</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _currentDialogue = 0;
            _playerWithinBounds = true;
            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
            }
            //set to tutorial text and fade in over time.
            _dialogueBox.SetText(_tutorialHint);
            _dialogueBox.alignment = TextAlignmentOptions.Center;

            if (!isNpc)
            {
                Vector4 newMargins = _dialogueBox.margin;
                newMargins.y = 0.05f;
                _dialogueBox.margin = newMargins;
            }

            _dialogueBox.CrossFadeAlpha(1f, _dialogueFadeDuration, false);
            _background.CrossFadeAlpha(1f, _dialogueFadeDuration, false);
            _eKey.CrossFadeAlpha(1f, _dialogueFadeDuration, false);
            _nameBox.CrossFadeAlpha(1f, _dialogueFadeDuration, false);
            _nameText.CrossFadeAlpha(1f, _dialogueFadeDuration, false);
        }
    }

    /// <summary>
    /// Invoked when the player exits,
    /// hides the dialogue box.
    /// </summary>
    /// <param name="other">The collider of the player.</param>
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _playerWithinBounds = false;
            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
            }
            OnLeave();
        }
    }

    /// <summary>
    /// Immediately show the entire text if it's currently being typed
    /// </summary>
    private void FinishTyping()
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }

        _dialogueBox.SetText(_currentFullText);
        _isTyping = false;
    }

    /// <summary>
    /// Coroutine to display the dialogue one letter at a time
    /// </summary>
    /// <param name="dialogue"></param>
    private IEnumerator TypeDialogue(string dialogue)
    {
        _isTyping = true;
        _currentFullText = dialogue;
        _dialogueBox.SetText(""); // Clear the dialogue box
        _dialogueBox.alignment = TextAlignmentOptions.Top;

        bool style = false;
        string currentTag = "";

        foreach (char letter in dialogue.ToCharArray())
        {
            if (letter == '<')
            {
                // Start capturing the tag
                style = true;
                currentTag += letter;
            }
            else if (style)
            {
                currentTag += letter;
                if (letter == '>')
                {
                    style = false;

                    _dialogueBox.text += currentTag;

                    currentTag = "";
                }
            }
            else
            {
                _dialogueBox.text += letter;

                // Apply delays based on punctuation
                switch (letter)
                {
                    case '?':
                    case '!':
                    case '.':
                        yield return new WaitForSeconds(_currentTypingSpeed * _periodTypeDelayMult);
                        break;
                    case ',':
                        yield return new WaitForSeconds(_currentTypingSpeed * _commaTypeDelayMult);
                        break;
                    default:
                        yield return new WaitForSeconds(_currentTypingSpeed);
                        break;
                }
            }
        }

        _isTyping = false;
    }

    /// <summary>
    /// Takes the reference of the current controller from the debug manager to make sure
    /// text and input prompts lines up with your current input device
    /// </summary>
    public void ControllerText()
    {
        _tutorialHint = ControllerGlyphManager.Instance.GetGlyph();
    }

    

    /// <summary>
    /// Makes sure the NPC has dialogue entries
    /// </summary>
    /// <returns>true if it does, false if it does not</returns>
    private bool CheckForEntries()
    {
        if (_dialogueEntries.Count == 0)
        {
            UnityEngine.Debug.LogWarning("No entries in " + gameObject.name + ", please add some.");
            _dialogueBox.SetText("No Entries in NPC.");
            return false;
        }
        return true;
    }
}
public enum EmotionType
{
    NEUTRAL,
    HAPPY,
    SAD,
    ANGRY,
    SCARED
}
