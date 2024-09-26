using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Cacophony
{
    public class PauseScreenUI : BaseScreenUI
    {
        [SerializeField] private GameObject _pauseDefault;
        [SerializeField] private GameObject _pauseControls;
        [SerializeField] private GameObject _pauseCredits;

        private void Start()
        {
            EnablePauseText(_pauseDefault);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (UIActive)
                {
                    return;
                }

                PauseGame();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetLevel();
            }
        }

        public void EnablePauseText(GameObject enabledObject)
        {
            _pauseControls.SetActive(false);
            _pauseDefault.SetActive(false);
            _pauseCredits.SetActive(false);

            enabledObject.SetActive(true);
        }

        public void PauseGame()
        {
            Time.timeScale = 0f;

            EnablePauseText(_pauseDefault);
            SetUIActive(true);
        }

        public void ResumeGame()
        {
            Time.timeScale = 1f;

            SetUIActive(false);
        }

        public void LoadScene(int sceneIndex)
        {
            Time.timeScale = 1f;

            SceneTransitions.Instance.LoadSceneWithTransition(SceneTransitions.TransitionType.Fade, sceneIndex);
        }

        public void ResetLevel()
        {
            LoadScene(SceneTransitions.Instance.GetBuildIndex());
        }

        public void CloseGame()
        {
            Application.Quit();
        }
    }
}