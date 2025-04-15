/******************************************************************
*    Author: Mitchell Young
*    Contributors: David Henvick, Claire Noto, Alec Pizziferro, Jamison Parks
*    Date Created: 4/10/25
*    Description: Handles on click behavior for collectable buttons
*    in the pause menu. Reuses functionality from NpcDialogueController
*    script.
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SaintsField;
using UnityEngine.Serialization;
using System;

public class CollectableButtonBehavior : MonoBehaviour
{
    [SerializeField] private TMP_Text _dialogueBox;
    [SerializeField] private GameObject _collectableImage;
    [SerializeField] private GameObject _collectableObject;
    [SerializeField] private GameObject _collectablePanel;

    [Serializable]
    public struct DialogueEntry
    {
        [TextArea] public string text;
    }

    private bool _isTalking;
    [InfoBox("This adjusts the base typing speed. 2 is the slowest, 10 is the fastest", EMessageType.Info)]
    [Range(2f, 10f)] [SerializeField] private float _typingSpeed = 5f;
    [SerializeField] private List<DialogueEntry> _dialogueEntries;
    [SerializeField] private float _dialogueFadeDuration = 0.25f;
    //dialogue options
    private int _currentDialogue = 0;
    private float _currentTypingSpeed;
    private Coroutine _typingCoroutine;
    private bool _isTyping = false;
    private string _currentFullText;

    /// <summary>
    /// Function called on button click that loads the collectable text.
    /// </summary>
    public void loadText()
    {
        if (_collectablePanel == null || _collectableObject == null || _collectableImage == null)
        {
            return;
        }
        _collectablePanel.SetActive(true);
        _collectableObject.GetComponent<Image>().sprite = _collectableImage.GetComponent<Image>().sprite;
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }
        AdvanceDialogue();
    }

    /// <summary>
    /// Is used to advance the current dialogue or show the dialogue if it is not already
    /// Is called by player when the interact key is used
    /// </summary>
    private void AdvanceDialogue()
    {
        if (!_isTalking)
        {
            if (CheckForEntries())
            {
                if (_typingCoroutine != null)
                {
                    StopCoroutine(_typingCoroutine);
                }
                _currentDialogue = 0;
                _typingCoroutine = StartCoroutine(TypeDialogue(_dialogueEntries[_currentDialogue].text));
            }
            _isTalking = true;
        }
        else
        {
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

            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
            }
            
            _typingCoroutine = StartCoroutine(TypeDialogue(_dialogueEntries[_currentDialogue].text));
        }
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
                        yield return new WaitForSeconds(_currentTypingSpeed * 3f);
                        break;
                    case ',':
                        yield return new WaitForSeconds(_currentTypingSpeed * 1.5f);
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
    /// Hides the dialogue for the collectable
    /// </summary>
    public void HideDialogue()
    {
        _currentTypingSpeed = 0;

        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }
        _isTyping = false;
        _isTalking = false;
        if (_collectablePanel == null)
        {
            return;
        }
        _collectablePanel.SetActive(false);
    }

    /// <summary>
    /// Makes sure the collectable object has dialogue entries
    /// </summary>
    /// <returns>true if it does, false if it does not</returns>
    private bool CheckForEntries()
    {
        if (_dialogueEntries.Count == 0)
        {
            HideDialogue();
        }
        return true;
    }

}
