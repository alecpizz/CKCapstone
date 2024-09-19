using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// Author: Liz
/// Description: Allows for easy scene loading with transitions. Nice and pretty!
/// </summary>
public class SceneTransitions : MonoBehaviour
{
    public static SceneTransitions Instance;

    private const string WIPE_UP = "WipeUp";
    private const string WIPE_DOWN = "WipeDown";
    private const string WIPE_LEFT = "WipeLeft";
    private const string WIPE_RIGHT = "WipeRight";
    private const string FADE = "Fade";

    private const string ANIM_START = "_Enter";
    private const string ANIM_END = "_Exit";

    public enum TransitionType
    {
        WipeUp,
        WipeDown,
        WipeLeft,
        WipeRight,
        Fade,
    }

    public static bool TransitionActive;

    [SerializeField] private Animator _animator;

    /// <summary>
    /// Singleton pattern.
    /// </summary>
    private void Awake()
    {
        if (Instance == null && Instance != this)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    //This is for testing going between the starting level and the 2 unlock levels - Ryan
    /*private void Update()
    {
        if(Input.GetKeyDown(KeyCode.U))
        {
            LoadSceneWithTransition(TransitionType.WipeDown, 1);
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            LoadSceneWithTransition(TransitionType.WipeDown, 17);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            LoadSceneWithTransition(TransitionType.WipeDown, 20);
        }
    }*/

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    /// <summary>
    /// Runs whenever the scene is loaded.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        MusicController.Instance?.EstablishMusic();
        TutorialManager.Instance?.CheckLevelTutorials();
    }

    /// <summary>
    /// Function to be called externally in order to load a scene.
    /// </summary>
    /// <param name="typeOfTransition">A TransitionType enum for the type of transition you want to play between scenes.</param>
    /// <param name="sceneToLoad">The scene build index to load.</param>
    public void LoadSceneWithTransition(TransitionType typeOfTransition, int sceneToLoad)
    {
        StartCoroutine(SceneTransition(TransitionNameFromTransitionType(typeOfTransition), sceneToLoad));
    }

    /// <summary>
    /// Gets the animation name from a TransitionType enum. Exists as typo prevention.
    /// </summary>
    /// <param name="typeOfTransition">The type of transition you are trying to find.</param>
    /// <returns>The string name of that transition.</returns>
    public string TransitionNameFromTransitionType(TransitionType typeOfTransition)
    {
        string parameter = "";

        switch (typeOfTransition)
        {
            case TransitionType.WipeUp:
                parameter = WIPE_UP;
                break;
            case TransitionType.WipeDown:
                parameter = WIPE_DOWN;
                break;
            case TransitionType.WipeLeft:
                parameter = WIPE_LEFT;
                break;
            case TransitionType.WipeRight:
                parameter = WIPE_RIGHT;
                break;
            case TransitionType.Fade:
                parameter = FADE;
                break;
        }

        return parameter;
    }

    /// <summary>
    /// Loads a new scene with a transition. Sets "TransitionActive" to true while running.
    /// </summary>
    /// <param name="transitionName">The name of the transition to use.</param>
    /// <param name="sceneToLoad">The buildindex of the scene to load.</param>
    private IEnumerator SceneTransition(string transitionName, int sceneToLoad)
    {
        TransitionActive = true;

        string enterAnimName = transitionName + ANIM_START;
        string exitAnimName = transitionName + ANIM_END;
        float enterTime = UIVisuals.GetAnimationTime(_animator, enterAnimName);
        float exitTime = UIVisuals.GetAnimationTime(_animator, exitAnimName);

        _animator.Play(enterAnimName);

        yield return new WaitForSecondsRealtime(enterTime);

        _animator.Play(exitAnimName);
        SceneManager.LoadScene(sceneToLoad);

        yield return new WaitForSecondsRealtime(exitTime);

        //MusicController.Instance.EstablishMusic();

        TransitionActive = false;
    }

    /// <summary>
    /// Gets the build index of the active scene.
    /// </summary>
    /// <returns>The active scene's build index.</returns>
    public int GetBuildIndex()
    {
        return SceneManager.GetActiveScene().buildIndex;
    }
}
