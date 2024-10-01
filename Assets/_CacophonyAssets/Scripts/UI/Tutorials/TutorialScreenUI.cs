using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Video;

// using UnityEngine.InputSystem;

/// <summary>
/// Author: Liz
/// Description: Manages the UI aspects of the Tutorial system.
/// </summary>
namespace Cacophony
{
    public class TutorialScreenUI : BaseScreenUI
    {
        [SerializeField] private Image _iconArea;
        [SerializeField] private TextMeshProUGUI _nameText, _descriptionText;
        [SerializeField] private Transform _pageIconArea;
        [Space] [SerializeField] private GameObject _tutorialPageIcon;
        [SerializeField] private Color _pageIconActiveColor, _pageIconInactiveColor;

        private List<TutorialSO> _givenTutorials = new List<TutorialSO>();
        private List<GameObject> _pageIcons = new List<GameObject>();
        private int _currentPage;

        public void PrepareTutorials(List<TutorialSO> tutorials)
        {
            foreach (GameObject i in _pageIcons)
            {
                Destroy(i);
            }

            _pageIcons.Clear();
            _givenTutorials.Clear();
            _givenTutorials.AddRange(tutorials);

            for (int i = 0; i < _givenTutorials.Count; i++)
            {
                GameObject icon = Instantiate(_tutorialPageIcon, _pageIconArea);
                _pageIcons.Add(icon);
            }
        }

        public void GiveSeenTutorials()
        {
            PrepareTutorials(TutorialManager.Instance.SeenTutorials());
        }

        public void DisplayTutorials()
        {
            _currentPage = 0;
            SetTutorialState(_currentPage);
            SetUIActive(true);
        }

        public void ChangePage(int amount)
        {
            if (!UIActive)
            {
                Debug.LogWarning("Tutorial box is not showing, the page cannot change.");
                return;
            }

            _currentPage += amount;

            if (_currentPage >= _givenTutorials.Count)
            {
                _currentPage = 0;
            }

            if (_currentPage < 0)
            {
                _currentPage = _givenTutorials.Count - 1;
            }

            SetTutorialState(_currentPage);
        }

        private void SetTutorialState(int page)
        {
            if (_givenTutorials.Count == 0)
            {
                _nameText.text = "No Tutorials Seen!";
                _descriptionText.text = "";
            }
            else
            {
                TutorialSO t = _givenTutorials[page];

                _iconArea.sprite = t.TutorialImage;
                _nameText.text = t.TutorialName;
                _descriptionText.text = t.TutorialDescription;

                foreach (GameObject icon in _pageIcons)
                {
                    icon.GetComponent<Image>().color = _pageIconInactiveColor;
                }

                _pageIcons[page].GetComponent<Image>().color = _pageIconActiveColor;
            }
        }
    }
}