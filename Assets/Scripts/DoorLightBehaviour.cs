/******************************************************************
*    Author: Zayden Joyner
*    Contributors: David Galmines 
*    Date Created: 11/7/24
*    Description: Handles the funcionality of turning the door light on/off.
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class DoorLightBehaviour : MonoBehaviour
{
    [Tooltip("For testing purposes only - this bool allows the user to turn " +
        "the light on and off in the inspector.  This can be removed once " +
        "the script is integrated into proper code.")]

    [FormerlySerializedAs("testLightSwitch")]
    [SerializeField] private bool _testLightSwitch = false;

    [Tooltip("Set the emission value (lamp brightness) for when the light is on.")]
    [SerializeField] private float _onEmission = 1f;

    [Tooltip("Set the light intensity value (actual brightness) for when the light is on.")]
    [SerializeField] private float _onIntensity = .07f;

    [Tooltip("Set how long the lamp should take to turn on/off.")]
    [SerializeField] private float _animationDuration = .5f;

    // Sets the light to be on or off
    private bool _lightOn = false;

    // Access the lamp's point light object
    private GameObject _pointLight;

    // Access the material the door is using
    private Material _doorMaterial;

    // Access the name of the shader property that handles emissive intensity
    private string _emissionPropertyName;

    [SerializeField] private GameObject _pointLightChild;
    [SerializeField] private GameObject _doorMaterialChild;

    /// <summary>
    /// Assign initial references
    /// </summary>
    void Start()
    {
        // Get the light object from the gameobject hierarchy
        _pointLight = _pointLightChild.gameObject;

        // Get the material of the lamp mesh
        _doorMaterial = _doorMaterialChild.GetComponent<MeshRenderer>().material;
        // Get the property name of the door material's emissive intensity
        _emissionPropertyName = _doorMaterial.shader.GetPropertyName(0);
    }

    /// <summary>
    /// Performs all functionality to turn the light on, including
    /// calling the coroutine to lerp emission and turning on the actual point light.
    /// </summary>
    public void TurnLightOn()
    {
        // Lerp emission
        StartCoroutine(LerpEmission(0f, _onEmission, _animationDuration));
        // Lerp light intensity
        StartCoroutine(LerpLight(0f, _onIntensity, _animationDuration));
    }

    /// <summary>
    /// Performs all functionality to turn the light off, including
    /// calling the coroutine to lerp emission and turning off the actual point light.
    /// </summary>
    public void TurnLightOff()
    {
        // Lerp emission
        StartCoroutine(LerpEmission(_onEmission, 0f, _animationDuration));
        // Lerp light intensity
        StartCoroutine(LerpLight(_onIntensity, 0f, _animationDuration));
    }

    /// <summary>
    /// This coroutine lerps the emission value of the lamp's material from and to
    /// specified values, over a specified span of time.
    /// </summary>
    /// <param name="startValue"> The value the emission should start at </param>
    /// <param name="endValue"> The value the emission should end at </param>
    /// <param name="duration"> How long the change should take (in seconds) </param>
    /// <returns></returns>
    private IEnumerator LerpEmission(float startValue, float endValue, float duration)
    {
        // This float will be updated over time to set the interpolation percentage
        // according the the specified lerp duration
        float time = 0;

        while (time < duration)
        {
            // Set the material emission to a percentage between the start and end values
            // that is correct according to the specified duration
            _doorMaterial.SetFloat(_emissionPropertyName, Mathf.Lerp(startValue, endValue, time / duration));

            // Add the seconds passed to time
            time += Time.deltaTime;

            // Return a null value
            yield return null;
        }

        // Just in case, set the emissive property to end value at the end
        _doorMaterial.SetFloat(_emissionPropertyName, endValue);
    }

    /// <summary>
    /// This coroutine lerps the intensity value of the lamp's associated point light from and to
    /// specified values, over a specified span of time.
    /// </summary>
    /// <param name="startValue"> The value the intensity should start at </param>
    /// <param name="endValue"> The value the intensity should end at </param>
    /// <param name="duration"> How long the change should take (in seconds) </param>
    /// <returns></returns>
    private IEnumerator LerpLight(float startValue, float endValue, float duration)
    {
        // This float will be updated over time to set the interpolation percentage
        // according the the specified lerp duration
        float time = 0;

        while (time < duration)
        {
            // Set the light intensity to a percentage between the start and end values
            // that is correct according to the specified duration
            _pointLight.GetComponent<Light>().intensity = Mathf.Lerp(startValue, endValue, time / duration);

            // Add the seconds passed to time
            time += Time.deltaTime;

            // Return a null value
            yield return null;
        }

        // Just in case, set the intensity value to end value at the end
        _pointLight.GetComponent<Light>().intensity = endValue;
    }
}
