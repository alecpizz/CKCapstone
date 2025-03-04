/******************************************************************
*    Author: Nick Grinstead
*    Contributors: 
*    Date Created: 10/10/24
*    Description: Handles the loading and reloading of scenes along with
*    performning a screen wipe transition.
*    Used https://www.youtube.com/watch?v=9d5Pz4SNmqo&t=300s as reference
*    for the shader graph set-up.
*******************************************************************/
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SaintsField;
using SaintsField.Playa;
using FMODUnity;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    // Remembers previous wipe color when loading a new scene
    private static Color _currentFadeColor = Color.black;

    [Required]
    [SerializeField] private Image _circleWipeImage;
    [Required]
    [SerializeField] private Camera _camera;
    [InfoBox("Needed for screen wipe to center on the player", EMessageType.Warning)]
    [SerializeField] private float _timeForScreenWipe;
    [SerializeField] private bool _shouldFadeInOnLoad;
    [SerializeField] private EventReference _endSound;
    [SerializeField] private bool _playSoundOnSceneChange = true;

    private readonly int _circleSizePropId = Shader.PropertyToID("_CircleSize");
    private readonly int _backgroundColorPropId = Shader.PropertyToID("_BackgroundColor");
    private readonly int _positionOffsetPropId = Shader.PropertyToID("_PositionOffset");

    private const float xUvOffset = 0.5f;
    private const float yUvOffset = 0.28f;

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

        if (_shouldFadeInOnLoad)
        {
            StartCoroutine(CircleWipeTransition(true, _currentFadeColor));
        }
        
        Transitioning = false;
    }

    /// <summary>
    /// Called to reload the current scene after a fade out transition
    /// </summary>
    public void ReloadCurrentScene()
    {
        StopAllCoroutines();
        _currentFadeColor = Color.black;
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        StartCoroutine(CircleWipeTransition(false, _currentFadeColor, sceneIndex));
    }

    /// <summary>
    /// Called to load a new scene after playing a fade out transition
    /// </summary>
    /// <param name="sceneBuildIndex">Build index of the scene to load</param>
    public void LoadNewScene(int sceneBuildIndex)
    {
        StopAllCoroutines();
        _currentFadeColor = Color.white;
        StartCoroutine(CircleWipeTransition(false, _currentFadeColor, sceneBuildIndex));
    }

    /// <summary>
    /// Called by the circle wipe coroutine to center the circle over the player
    /// </summary>
    private void RepositionCircleWipe()
    {
        // Do circle wipe in center of the screen if there's no player
        if (PlayerMovement.Instance == null)
        {
            _circleWipeImage.materialForRendering.SetVector(_positionOffsetPropId, new Vector2(0, 0));
            return;
        }

        // Find where player is in screen space
        Vector3 screenPos = _camera.WorldToScreenPoint(PlayerMovement.Instance.transform.position);

        // Turn pixel position into percent of screen width and height
        float xPercent = screenPos.x / _camera.pixelWidth;
        float yPercent = screenPos.y / _camera.pixelHeight;

        // Use percentages to calculate UV offsets for shader
        Vector2 newOffset = new Vector2();
        newOffset.x = (xPercent - xUvOffset) * -1f;
        newOffset.y = (yPercent * yUvOffset * 2 - yUvOffset) * -1f;
        _circleWipeImage.materialForRendering.SetVector(_positionOffsetPropId, newOffset);
    }

    /// <summary>
    /// Handles the smoothly animating the circle wipe over a period of time
    /// </summary>
    /// <param name="isFadingIn">True if circle should move out, false if it should close in</param>
    /// <param name="fadeColor">Determines color of the background during wipe</param>
    /// <param name="sceneIndexToLoad">Index of the scene to load, only needed if fading out</param>
    private IEnumerator CircleWipeTransition(bool isFadingIn, Color fadeColor, int sceneIndexToLoad = -1)
    {
        float elapsedTime = 0f;
        float lerpingTime;
        float newCircleSize;
        float startingCircleSize = isFadingIn ? 0f : 1f;
        float targetCircleSize = isFadingIn ? 1f : 0f;

        _circleWipeImage.materialForRendering.SetFloat(_circleSizePropId, startingCircleSize);
        _circleWipeImage.materialForRendering.SetColor(_backgroundColorPropId, fadeColor);

        Transitioning = true;
        
        RepositionCircleWipe();
        if (AudioManager.Instance != null && _playSoundOnSceneChange)
        {
            AudioManager.Instance.PlaySound(_endSound);
        }
        yield return new WaitForEndOfFrame();
        
        
        
        // Animates circle wipe until the end time is reached
        while (elapsedTime < _timeForScreenWipe)
        {
            lerpingTime = elapsedTime / _timeForScreenWipe;
            newCircleSize = Mathf.Lerp(startingCircleSize, targetCircleSize, lerpingTime);
            _circleWipeImage.materialForRendering.SetFloat(_circleSizePropId, newCircleSize);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        _circleWipeImage.materialForRendering.SetFloat(_circleSizePropId, targetCircleSize);

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
        ReloadCurrentScene();
    }
#endif
}
