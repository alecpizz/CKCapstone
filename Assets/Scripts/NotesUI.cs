/******************************************************************
*    Author: Trinity Hutson
*    Contributors: 
*    Date Created: 4/13/25
*    Description: Stores information regarding a note HUD prefab
*******************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NotesUI : MonoBehaviour
{
    public TMP_Text LevelNumber { get { return _levelNumber; } }
    public TMP_Text TimeSigX { get { return _timeSignatureUiX; } }
    public TMP_Text TimeSigY { get { return _timeSignatureUiY; } }
    public Image Ribbon { get { return _ribbon; } }
    public Image[] NoteImages { get { return _noteImages; } }
    public Image[] GhostNoteImages { get { return _ghostNoteImages; } }

    [Header("Required Components")]
    [SerializeField]
    TMP_Text _levelNumber;
    [Space]
    [SerializeField]
    private TMP_Text _timeSignatureUiX;
    [SerializeField]
    private TMP_Text _timeSignatureUiY;
    [Space]
    [SerializeField] 
    private Image[] _noteImages;
    [SerializeField] 
    private Image[] _ghostNoteImages;

    [Header("Optional Components")]
    [SerializeField]
    private Image _ribbon;
    [Space]
    [SerializeField]
    private Image _arrow;
    [SerializeField]
    private TMP_Text _secondaryUiX;
    [SerializeField]
    private TMP_Text _secondaryUiY;
}
