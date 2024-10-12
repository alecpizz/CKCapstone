using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NaughtyAttributes;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    [Required]
    [SerializeField] private Image _circleWipeImage;
    [SerializeField] private float _timeForScreenWipe;

    private readonly int _circleSizePropId = Shader.PropertyToID("_CircleSize");

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

        StartCoroutine(CircleWipeTransition(true));
    }

    /// <summary>
    /// Called to reload the current scene after a fade out transition
    /// </summary>
    public void ReloadCurrentScene()
    {
        StopAllCoroutines();
        string sceneName = SceneManager.GetActiveScene().name;
        StartCoroutine(CircleWipeTransition(false, sceneName));
    }

    /// <summary>
    /// Overload for loading a new scene with build index
    /// </summary>
    /// <param name="sceneBuildIndex">Build index of the scene to load</param>
    public void LoadNewScene(int sceneBuildIndex)
    {
        LoadNewScene(SceneManager.GetSceneByBuildIndex(sceneBuildIndex).name);
    }

    /// <summary>
    /// Loads a new scene after a fade out transition
    /// </summary>
    /// <param name="sceneName">Name of the scene to load</param>
    public void LoadNewScene(string sceneName)
    {
        StopAllCoroutines();
        StartCoroutine(CircleWipeTransition(false, sceneName));
    }

    /// <summary>
    /// Handles the smoothly animating the circle wipe over a period of time
    /// </summary>
    /// <param name="isFadingIn">True if circle should move out, false if it should close in</param>
    /// <param name="sceneToLoad">Name of the scene to load, only needed if fading out</param>
    private IEnumerator CircleWipeTransition(bool isFadingIn, string sceneToLoad = "")
    {
        float elapsedTime = 0f;
        float lerpingTime;
        float newCircleSize;
        float startingCircleSize = isFadingIn ? 0f : 1f;
        float targetCircleSize = isFadingIn ? 1f : 0f;

        _circleWipeImage.materialForRendering.SetFloat(_circleSizePropId, startingCircleSize);

        // Animates circle wipe until the end time is reached
        while (elapsedTime < _timeForScreenWipe)
        {
            lerpingTime = elapsedTime / _timeForScreenWipe;
            newCircleSize = Mathf.Lerp(startingCircleSize, targetCircleSize, lerpingTime);
            _circleWipeImage.materialForRendering.SetFloat(_circleSizePropId, newCircleSize);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _circleWipeImage.materialForRendering.SetFloat(_circleSizePropId, targetCircleSize);

        // Loads new scene if needed
        if (!isFadingIn && !sceneToLoad.Equals(""))
            SceneManager.LoadScene(sceneToLoad);
    }

#if UNITY_EDITOR
    /// <summary>
    /// Can be used for testing in editor
    /// </summary>
    [Button]
    private void ReloadScene()
    {
        ReloadCurrentScene();
    }
#endif
}
