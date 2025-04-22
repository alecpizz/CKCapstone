using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SaintsField;
using SaintsField.Playa;

public class TestScreenTransitions : MonoBehaviour
{
    [SerializeField] private Image _circleWipeImage;
    [SerializeField] private float _timeForScreenWipe = 1f;
    [SerializeField] private float _riseTo = 1f;

    private readonly int _rise = Shader.PropertyToID("_Rise");

#if UNITY_EDITOR
    /// <summary>
    /// Can be used for testing in editor
    /// </summary>
    [Button]
    private void PlayTransition()
    {
        StartCoroutine(ScreenTransition());
    }
#endif

    private IEnumerator ScreenTransition()
    {
        float time = 0f;
        _circleWipeImage.materialForRendering.SetFloat(_rise, 0f);

        while (time < _timeForScreenWipe)
        {
            _circleWipeImage.materialForRendering.SetFloat(_rise, Mathf.Lerp(0f, _riseTo, time / _timeForScreenWipe));

            time += Time.unscaledDeltaTime;
            yield return null;
        }

        _circleWipeImage.materialForRendering.SetFloat(_rise, _riseTo);

        yield return new WaitForSeconds(.3f);

        time = 0f;

        while (time < _timeForScreenWipe)
        {
            _circleWipeImage.materialForRendering.SetFloat(_rise, Mathf.Lerp(_riseTo, 0f, time / _timeForScreenWipe));

            time += Time.unscaledDeltaTime;
            yield return null;
        }

        _circleWipeImage.materialForRendering.SetFloat(_rise, 0f);
    }
}
