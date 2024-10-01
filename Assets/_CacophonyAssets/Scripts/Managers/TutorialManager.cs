using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: Liz
/// Description: Singleton that manages the tutorials a player has scene. Finds tutorials in the SceneTutorials object and displays them on startup.
/// </summary>
namespace Cacophony
{
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance;

        private void Awake()
        {
            if (Instance == null && Instance != this)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            DontDestroyOnLoad(gameObject);
        }

        [SerializeField] private List<TutorialSO> _allTutorials = new List<TutorialSO>();
        private List<TutorialSO> _playerSeenTutorials = new List<TutorialSO>();

        public void CheckLevelTutorials()
        {
            TutorialsInScene tutorials = FindObjectOfType<TutorialsInScene>();

            if (tutorials == null)
            {
                return;
            }

            List<TutorialSO> newTutorials = new List<TutorialSO>();

            foreach (TutorialSO t in tutorials.GetSceneTutorials())
            {
                if (HasTutorial(t))
                {
                    continue;
                }

                _playerSeenTutorials.Add(t);
                newTutorials.Add(t);
            }

            if (newTutorials.Count > 0)
            {
                DisplayStartupTutorials(newTutorials);
            }
        }

        private void DisplayStartupTutorials(List<TutorialSO> newTutorials)
        {
            TutorialScreenUI ui = FindObjectOfType<TutorialScreenUI>();

            if (ui == null)
            {
                return;
            }

            ui.PrepareTutorials(newTutorials);
            ui.DisplayTutorials();
        }

        private bool HasTutorial(TutorialSO t)
        {
            if (_playerSeenTutorials.Contains(t))
            {
                return true;
            }

            return false;
        }

        public void SetAllTutorialsSeen()
        {
            _playerSeenTutorials.Clear();
            _playerSeenTutorials.AddRange(_allTutorials);
        }

        public void ResetTutorials()
        {
            _playerSeenTutorials.Clear();
        }

        public List<TutorialSO> SeenTutorials()
        {
            return _playerSeenTutorials;
        }
    }
}