/******************************************************************
*    Author: Zayden Joyner
*    Contributors: 
*    Date Created: 4/1/25
*    Description: Creates a lightning visual effect using a directional light.
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class LightningEffect : MonoBehaviour
{
    [Tooltip("Use this bool to test the lightning at any time in editor.")]

    [SerializeField] private bool _testLightning = false;

    [Tooltip("Set the light intensity value (brightness) for when the lightning strikes.")]
    [SerializeField] private float _onIntensity = 2.5f;

    [Tooltip("Set how long the lightning takes to fade out.")]
    [SerializeField] private float _animationDuration = .5f;

    private void Update()
    {
        if (_testLightning)
        {
            Lightning();
            _testLightning = false;
        }
    }

    /// <summary>
    /// Performs all functionality to making lightning happen
    /// </summary>
    public void Lightning()
    {
        // Lerp light intensity
        StartCoroutine(LerpLightning(_onIntensity, 0f, _animationDuration));
    }

    /// <summary>
    /// This coroutine lerps the intensity value of the lightning light
    /// </summary>
    /// <param name="startValue"> The value the intensity should start at </param>
    /// <param name="endValue"> The value the intensity should end at </param>
    /// <param name="duration"> How long the change should take (in seconds) </param>
    /// <returns></returns>
    private IEnumerator LerpLightning(float startValue, float endValue, float duration)
    {
        // This float will be updated over time to set the interpolation percentage
        // according the the specified lerp duration
        float time = 0;

        while (time < duration)
        {
            // Set the light intensity to a percentage between the start and end values
            // that is correct according to the specified duration
            GetComponent<Light>().intensity = Mathf.Lerp(startValue, endValue, time / duration);

            // Add the seconds passed to time
            time += Time.deltaTime;

            // Return a null value
            yield return null;
        }

        // Just in case, set the intensity value to end value at the end
        GetComponent<Light>().intensity = endValue;
    }
}
