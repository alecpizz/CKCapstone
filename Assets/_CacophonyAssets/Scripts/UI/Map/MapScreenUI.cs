using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cacophony
{
    public class MapScreenUI : BaseScreenUI
    {
        [SerializeField] private Transform _keyContainer;
        [SerializeField] private GameObject _keyElementPrefab;

        public void SpawnKeyElement(KeyElement keyElement)
        {
            KeyItemUI ki = Instantiate(_keyElementPrefab, _keyContainer).GetComponent<KeyItemUI>();
            ki.PrepareElement(keyElement);
        }
    }
}