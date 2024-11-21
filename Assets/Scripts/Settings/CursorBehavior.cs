/******************************************************************
*    Author: Claire Noto
*    Contributors: Claire Noto
*    Date Created: 11/14/2024
*    Description: Centralized cursor manager that changes the cursor
*                 when hovering over various UI elements, including
*                 Buttons, Toggles, Sliders, and Dropdowns.
*******************************************************************/
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using FMODUnity;

public class CursorManager : MonoBehaviour
{
    [SerializeField] private Texture2D _cursor;
    [SerializeField] private Texture2D _cursorHover;
    [SerializeField] private EventReference _clickSound = default;
    [SerializeField] private Canvas _canvas;

    private void Start()
    {
        // Find the Canvas or root UI GameObject
        if (_canvas != null)
        {
            AddEventTriggersToChildren(_canvas.gameObject);
        }
    }

    /// <summary>
    /// Adds EventTrigger components to each specified UI element type
    /// within the given parent object (e.g., the Canvas).
    /// </summary>
    /// <param name="parent">The parent GameObject containing UI elements</param>
    private void AddEventTriggersToChildren(GameObject parent)
    {
        // Find all relevant UI components within the parent (Canvas)
        Button[] buttons = parent.GetComponentsInChildren<Button>(true);
        Toggle[] toggles = parent.GetComponentsInChildren<Toggle>(true);
        Slider[] sliders = parent.GetComponentsInChildren<Slider>(true);
        TMP_Dropdown[] dropdowns = parent.GetComponentsInChildren<TMP_Dropdown>(true);

        // Add EventTriggers to each UI component type
        foreach (Button button in buttons)
        {
            AddEventTrigger(button.gameObject);
        }

        foreach (Toggle toggle in toggles)
        {
            AddEventTrigger(toggle.gameObject);
        }

        foreach (Slider slider in sliders)
        {
            AddEventTrigger(slider.gameObject);
        }

        foreach (TMP_Dropdown dropdown in dropdowns)
        {
            AddEventTrigger(dropdown.gameObject);
        }
    }

    /// <summary>
    /// Adds an EventTrigger component to a UI element to handle
    /// PointerEnter and PointerExit events, if not already present.
    /// </summary>
    /// <param name="uiElement">The UI GameObject to modify</param>
    private void AddEventTrigger(GameObject uiElement)
    {
        EventTrigger trigger = uiElement.GetComponent<EventTrigger>() ?? uiElement.AddComponent<EventTrigger>();

        // Add Pointer Enter event
        EventTrigger.Entry entryEnter = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        entryEnter.callback.AddListener((eventData) => { OnPointerEnter(); });
        trigger.triggers.Add(entryEnter);

        // Add Pointer Exit event
        EventTrigger.Entry entryExit = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerExit
        };
        entryExit.callback.AddListener((eventData) => { OnPointerExit(); });
        trigger.triggers.Add(entryExit);

        EventTrigger.Entry entrySelect = new EventTrigger.Entry
        {
            eventID = EventTriggerType.Select
        };
        entrySelect.callback.AddListener((eventData) => { Select(); });
        trigger.triggers.Add(entrySelect);
    }

    /// <summary>
    /// Sets the custom cursor when the pointer enters a UI element.
    /// </summary>
    private void OnPointerEnter()
    {
        Cursor.SetCursor(_cursorHover, Vector2.zero, CursorMode.Auto);
    }

    /// <summary>
    /// Reverts to the default cursor when the pointer exits a UI element.
    /// </summary>
    private void OnPointerExit()
    {
        Cursor.SetCursor(_cursor, Vector2.zero, CursorMode.Auto);
    }

    private void Select()
    {
        AudioManager.Instance.PlaySound(_clickSound);
    }
}