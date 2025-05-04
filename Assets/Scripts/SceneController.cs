/******************************************************************
*    Author: Nick Grinstead
*    Contributors: 
*    Date Created: 10/10/24
*    Description: Handles the loading and reloading of scenes along with
*    performning a screen wipe transition.
*    Used https://www.youtube.com/watch?v=9d5Pz4SNmqo&t=300s as reference
*    for the shader graph set-up.
*******************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SaintsField;
using SaintsField.Playa;
using FMODUnity;

public enum SceneTransitionType
{
    Illness,
    Son,
    Portal,
    Challenge,
    Black
}

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    // Remembers previous wipe color when loading a new scene
    private static SceneTransitionType currentTransitionType = SceneTransitionType.Black;

    [SerializeField] private bool _shouldFadeInOnLoad;
    [SerializeField] private EventReference _endSound;
    [SerializeField] private EventReference _deathSound;
    [SerializeField] private bool _playSoundOnSceneChange = true;
    [SerializeField] private SceneTransitionBase _illnessTransition;
    [SerializeField] private SceneTransitionBase _sonTransition;
    [SerializeField] private SceneTransitionBase _portalTransition;
    [SerializeField] private SceneTransitionBase _challengeTransition;
    [SerializeField] private SceneTransitionBase _blackTransition;
    private Dictionary<SceneTransitionType, SceneTransitionBase> _transitions = new();

    public bool Transitioning {get; private set;}
    
    /// <summary>
    /// Creates instance and starts fade in transition
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }

        Instance = this;
        

        Time.timeScale = 1.0f;
        _transitions[SceneTransitionType.Black] = _blackTransition;
        _transitions[SceneTransitionType.Challenge] = _challengeTransition;
        _transitions[SceneTransitionType.Illness] = _illnessTransition;
        _transitions[SceneTransitionType.Portal] = _portalTransition;
        _transitions[SceneTransitionType.Son] = _sonTransition;
        foreach (var st in _transitions)
        {
            st.Value?.Init();
        }

        if (_shouldFadeInOnLoad)
        {
            StartCoroutine(CircleWipeTransition(true, currentTransitionType));
        }
        
        Transitioning = false;
    }
    /// <summary>
    /// Called to reload the current scene after a fade out transition
    /// </summary>
    public void ReloadCurrentScene(SceneTransitionType type)
    {
        if (Transitioning) { return; }

        StopAllCoroutines();
        currentTransitionType = type;
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        StartCoroutine(CircleWipeTransition(false, currentTransitionType, sceneIndex));
    }

    /// <summary>
    /// Called to load a new scene after playing a fade out transition
    /// </summary>
    /// <param name="sceneBuildIndex">Build index of the scene to load</param>
    public void LoadNewScene(int sceneBuildIndex, SceneTransitionType type)
    {
        if (Transitioning) { return; }

        StopAllCoroutines();
        currentTransitionType = type;
        StartCoroutine(CircleWipeTransition(false, currentTransitionType, sceneBuildIndex));
    }
    
    /// <summary>
    /// Handles the smoothly animating the circle wipe over a period of time
    /// </summary>
    /// <param name="isFadingIn">True if circle should move out, false if it should close in</param>
    /// <param name="fadeColor">Determines color of the background during wipe</param>
    /// <param name="sceneIndexToLoad">Index of the scene to load, only needed if fading out</param>
    private IEnumerator CircleWipeTransition(bool isFadingIn, SceneTransitionType type, int sceneIndexToLoad = -1)
    {
        Transitioning = true;
        
        if (AudioManager.Instance != null) {

            if (_playSoundOnSceneChange && PlayerMovement.Instance != null)
            {
                if (PlayerMovement.Instance.PlayerDied)
                    AudioManager.Instance.PlaySound(_deathSound);
                else
                    AudioManager.Instance.PlaySound(_endSound);
            }
            else
                AudioManager.Instance.PlaySound(_endSound);
        }


        if (isFadingIn)
        {
            _transitions[type].FadeOut();
        }
        else
        {
            _transitions[type].FadeIn();
        }
        yield return new WaitForEndOfFrame();

        yield return new WaitUntil(() => !_transitions[type].InProgress());
        
        // Animates circle wipe until the end time is reached

        yield return new WaitForSecondsRealtime(0.1f);
        
        Transitioning = false;

        // Loads new scene if needed
        if (!isFadingIn && sceneIndexToLoad != -1)
        {
            SceneManager.LoadScene(sceneIndexToLoad);
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Can be used for testing in editor
    /// </summary>
    [Button]
    private void ReloadScene()
    {
        ReloadCurrentScene(currentTransitionType);
    }
#endif
}
