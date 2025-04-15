/******************************************************************
*    Author: Trinity Hutson
*    Contributors: 
*    Date Created: 4/13/25
*    Description: Stores information regarding a note HUD prefab, and methods to modify it
*******************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using PrimeTween;

public class NotesUI : MonoBehaviour
{
    public TMP_Text LevelNumber { get { return _levelNumber; } }

    [Header("Required Components")]
    [SerializeField]
    private TMP_Text _levelNumber;
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

    [Header("Animation")]
    [SerializeField]
    private float _animDuration = 1f;
    [SerializeField]
    private float _animArrowHeight = 221f;
    [SerializeField]
    private float _animArrowScale = 1.3f;
    [SerializeField]
    private float _animMinAlpha = 0.1f;
    [SerializeField]
    private Ease _animEaseType = Ease.OutSine;
    [SerializeField]
    private float[] animKeyFrameDurations = { 0.33f, 0.45f, 0.5f, 2f };

    private bool _timeSigAnimEnabled = false;
    
    private float _arrowStartY;
    private Vector3 _arrowStartScale;
    private List<Sequence> _activeTweens = new();

    /// <summary>
    /// Prevent animation from playing on level start / restart
    /// </summary>
    private void Start()
    {
        Tween.Delay(_animDuration)
            .OnComplete(() => {
                _timeSigAnimEnabled = true;
            });

        _arrowStartY = _arrow.transform.localPosition.y;
        _arrowStartScale = _arrow.transform.localScale;
    }

    /// <summary>
    /// Set the level title text
    /// </summary>
    /// <param name="text"></param>
    public void SetLevelText(string text)
    {
        _levelNumber.text = text;
    }

    /// <summary>
    /// Toggles the arrow child object
    /// </summary>
    /// <param name="active"></param>
    public void ToggleArrow(bool active)
    {
        _arrow.gameObject.SetActive(active);
    }

    /// <summary>
    /// Enables Ghost note icon at specified note index
    /// </summary>
    /// <param name="collectedNote">note index</param>
    public void UpdateGhostNoteIcon(int collectedNote)
    {
        if (collectedNote >= 0 && collectedNote < _noteImages.Length)
        {
            _ghostNoteImages[collectedNote].gameObject.SetActive(true);
            _ghostNoteImages[collectedNote].enabled = true;
        }
    }

    /// <summary>
    /// Enables note icon at specified index, and disables the ghost note at that index
    /// </summary>
    /// <param name="collectedNote">note index</param>
    public void UpdateCollectedNoteIcon(int collectedNote)
    {
        if (collectedNote < 0 || collectedNote > _noteImages.Length - 1)
            return;

        _noteImages[collectedNote].gameObject.SetActive(true);
        _noteImages[collectedNote].enabled = true;
        _ghostNoteImages[collectedNote].enabled = false;
    }

    /// <summary>
    /// Updates the HUD to match the newest time signature
    /// </summary>
    /// <param name="newTimeSignature">new time sig</param>
    /// <param name="playAnimation">determines if the animation should play or not</param>
    public void UpdateTimingFromSignature(Vector2Int newTimeSignature, bool playAnimation)
    {
        if (_timeSignatureUiX == null || _timeSignatureUiY == null)
        {
            Debug.LogWarning("Missing hud elements");
            return;
        }

        bool hasOptionalUI = _arrow != null;

        if (hasOptionalUI && playAnimation && _timeSigAnimEnabled)
        {
            AnimatedTimeSigUpdate(newTimeSignature);
            return;
        }

        _timeSignatureUiX.text = newTimeSignature.x.ToString();
        _timeSignatureUiY.text = newTimeSignature.y.ToString();

        // Return early if no more updating is needed
        if (!hasOptionalUI)
            return;

        Vector2Int nextSecondaryTS = TimeSignatureManager.Instance.GetNextTimeSignature();
        _secondaryUiX.text = nextSecondaryTS.x.ToString();
        _secondaryUiY.text = nextSecondaryTS.y.ToString();
    }

    /// <summary>
    /// Animates the time signature change through tweening
    /// </summary>
    /// <param name="newTimeSignature">new time sig</param>
    private void AnimatedTimeSigUpdate(Vector2Int newTimeSignature)
    {
        if (_arrow == null)
            return;

        ResetAnimation();

        Vector2Int nextSecondaryTS = TimeSignatureManager.Instance.GetNextTimeSignature();

        float _arrowStopY = _arrowStartY + _animArrowHeight;
        Vector3 _arrowStopScale = _arrowStartScale * _animArrowScale;

        for (int i = 0; i < animKeyFrameDurations.Length; i++)
            animKeyFrameDurations[i] *= _animDuration;

        // Moves _arrow Up
        Sequence temp = Tween.LocalPositionY(_arrow.transform, _arrowStopY, animKeyFrameDurations[0], _animEaseType)

            // Reduce alpha of current time sig, updates it, then returns to alpha 1
            .Group(Tween.Alpha(_timeSignatureUiX, _animMinAlpha, animKeyFrameDurations[0], _animEaseType))
            .Group(Tween.Alpha(_timeSignatureUiY, _animMinAlpha, animKeyFrameDurations[0], _animEaseType))

            // Scales up _arrow (and its child text) to fit current time sig. Vanishes after matching
            .Chain(Tween.Scale(_arrow.transform, _arrowStopScale, animKeyFrameDurations[1])
                .Group(Tween.Alpha(_arrow, 0, animKeyFrameDurations[1]))
                .Group(Tween.Alpha(_timeSignatureUiX, 1, animKeyFrameDurations[1]))
                .Group(Tween.Alpha(_timeSignatureUiY, 1, animKeyFrameDurations[1]))
                .Group(Tween.Alpha(_secondaryUiX, 0, animKeyFrameDurations[1]))
                .Group(Tween.Alpha(_secondaryUiY, 0, animKeyFrameDurations[1]))
                .InsertCallback(animKeyFrameDurations[1], () => {
                    _timeSignatureUiX.text = newTimeSignature.x.ToString();
                    _timeSignatureUiY.text = newTimeSignature.y.ToString();
                }))

            .OnComplete(() => {
                _timeSignatureUiX.text = newTimeSignature.x.ToString();
                _timeSignatureUiY.text = newTimeSignature.y.ToString();
            })
            .Chain(Tween.Alpha(_timeSignatureUiX, 1, 0)
                .Group(Tween.Alpha(_timeSignatureUiY, 1, 0)))

            // Resets _arrow
            .Chain(Tween.LocalPositionY(_arrow.transform, _arrowStartY, 0f))
            .ChainCallback(() => {
                _secondaryUiX.text = nextSecondaryTS.x.ToString();
                _secondaryUiY.text = nextSecondaryTS.y.ToString();

                _secondaryUiX.alpha = 0;
                _secondaryUiY.alpha = 0;
            })
            .ChainDelay(animKeyFrameDurations[^2])

            // Fade _arrow text back in
            .Chain(Tween.Scale(_arrow.transform, _arrowStartScale, 0f))
            .Chain(Tween.Alpha(_arrow, 1, animKeyFrameDurations[^1])
                .Group(Tween.Alpha(_secondaryUiX, 1, animKeyFrameDurations[^1]))
                .Group(Tween.Alpha(_secondaryUiY, 1, animKeyFrameDurations[^1])));

        _activeTweens.Add(temp);

        temp = Tween.Alpha(_timeSignatureUiX, _animMinAlpha, _animDuration / 2)
            .OnComplete(() => {
                _timeSignatureUiX.text = newTimeSignature.x.ToString();
            })
            .Chain(Tween.Alpha(_timeSignatureUiX, 1, _animDuration / 2));

        _activeTweens.Add(temp);

        temp = Tween.Alpha(_timeSignatureUiY, _animMinAlpha, _animDuration / 2)
            .OnComplete(() => {
                _timeSignatureUiY.text = newTimeSignature.y.ToString();
            })
            .Chain(Tween.Alpha(_timeSignatureUiY, 1, _animDuration / 2));

        _activeTweens.Add(temp);
    }

    /// <summary>
    /// Resets arrow's position and scale, and prevents it from being manipulated in unintended ways. Stops all active tweens.
    /// </summary>
    private void ResetAnimation()
    {
        foreach(Sequence t in _activeTweens)
        {
            if (t.isAlive)
                t.Stop();
        }

        _activeTweens.Clear();

        _arrow.transform.localPosition = new Vector3(_arrow.transform.localPosition.x, _arrowStartY, _arrow.transform.localPosition.z);

        _arrow.transform.localScale = _arrowStartScale;

        _secondaryUiX.alpha = 1;
        _secondaryUiY.alpha = 1;
        _arrow.color = new Color(_arrow.color.r, _arrow.color.g, _arrow.color.b, 1f);
    }
}
