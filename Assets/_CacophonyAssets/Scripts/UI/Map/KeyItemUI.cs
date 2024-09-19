using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class KeyItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private Image _colorIcon;

    public void PrepareElement(KeyElement keyElement)
    {
        _text.text = keyElement.KeyElementName;
        _colorIcon.color = keyElement.KeyElementColor;
    }
}
