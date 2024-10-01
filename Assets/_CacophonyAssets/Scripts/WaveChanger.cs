using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: Ryan
/// Description: The object that is hit to change the type of wave
/// </summary>
namespace Cacophony
{
    public class WaveChanger : MonoBehaviour
    {
        [SerializeField] HarmonizationType _changeToType;

        public HarmonizationType GetChangeType()
        {
            return _changeToType;
        }
    }
}