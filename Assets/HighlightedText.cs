using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class HighlightedText : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject _nameOfCutscene;

    // Start is called before the first frame update
    void Start()
    {
        TextMeshProUGUI cutsceneNameText = _nameOfCutscene.GetComponent<TextMeshProUGUI>();

        //cutsceneNameText.text = ;

        if( _nameOfCutscene != null)
        {
            _nameOfCutscene.SetActive(false);
        }
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_nameOfCutscene != null)
        {
            _nameOfCutscene.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_nameOfCutscene != null)
        {
            _nameOfCutscene.SetActive(false);
        }
    }

}
