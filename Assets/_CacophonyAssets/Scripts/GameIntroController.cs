using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cacophony
{
    public class GameIntroController : MonoBehaviour
    {
        public void LoadGame()
        {
            SceneTransitions.Instance.LoadSceneWithTransition(SceneTransitions.TransitionType.Fade, 1);
        }

        public void MarkTutorialsComplete()
        {
            TutorialManager.Instance.SetAllTutorialsSeen();
        }
    }
}