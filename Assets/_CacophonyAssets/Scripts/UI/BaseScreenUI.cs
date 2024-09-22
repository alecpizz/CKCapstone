using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: Liz
/// Description: Base class for full-screen UI elements
/// </summary>
namespace Cacophony
{
    public class BaseScreenUI : MonoBehaviour
    {
        private const string ENTER_ANIM = "TutorialBoxFadeIn";
        private const string EXIT_ANIM = "TutorialBoxFadeOut";

        public static bool UIActive = false;

        [SerializeField] private Animator _animator;

        public void SetUIActive(bool active)
        {
            UIActive = active;

            if (active)
            {
                _animator.Play(ENTER_ANIM);
            }
            else
            {
                _animator.Play(EXIT_ANIM);
            }
        }
    }
}