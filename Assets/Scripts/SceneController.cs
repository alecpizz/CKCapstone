using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    [SerializeField] private float _timeForSceneTransition;
    [SerializeField] private Transform _circularWipeMaskTrans;
    [SerializeField] private Transform _wipeBackgroundTrans;

    private const float MaxWipeScale = 12.0f;
    private const float MinWipeScale = 0.0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }

        Instance = this;
    }

    public void ReloadCurrentScene()
    {
        
    }

    public void LoadNewScene(int sceneBuildIndex)
    {
        LoadNewScene(SceneManager.GetSceneByBuildIndex(sceneBuildIndex).name);
    }

    public void LoadNewScene(string sceneName)
    {
        StartCoroutine(nameof(DelayedSceneTransition));

        //SceneManager.LoadScene(sceneName);
    }

    private IEnumerator DelayedSceneTransition()
    {
        float elapsedTime = 0f;
        float lerpingTime;
        Vector3 newScale = new Vector3();

        while (elapsedTime < _timeForSceneTransition)
        {
            lerpingTime = elapsedTime / _timeForSceneTransition;
            newScale.x = newScale.y = newScale.z = Mathf.Lerp(MaxWipeScale, MinWipeScale, lerpingTime);
            _circularWipeMaskTrans.localScale = newScale;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
