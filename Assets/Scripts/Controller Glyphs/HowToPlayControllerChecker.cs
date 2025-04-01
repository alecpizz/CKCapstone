using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class HowToPlayControllerChecker : MonoBehaviour
{
    public static HowToPlayControllerChecker Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI _tutorialText;

    [SerializeField] private string _keyboardText;
    [SerializeField] private string _controllerText; //Text for Xbox/Generic Controllers
    [SerializeField] private string _playstationText; //Text for PS Controllers
    [SerializeField] private string _switchText; //Text for Switch Pro Controllers

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        HowToPlayTextChange();
    }

    /// <summary>
    /// Handles changing the tutorial text depending on if a controller is connected or not
    /// </summary>
    public void HowToPlayTextChange()
    {
        if (DebugMenuManager.Instance.KeyboardAndMouse)
        {
            _tutorialText.text = "Collect notes in ascending order\r\nMove: WASD or arrow keys\r\n" +
                "Restart Level: R\r\nPause: Esc\r\nEnemy Path Toggle: Q";
        }
        else if (DebugMenuManager.Instance.SwitchController)
        {
            _tutorialText.text = "Collect notes in ascending order\r\nMove: Left control stick or D-pad\r\n" +
                "Restart Level: Select\r\nPause: Start\r\nEnemy Path Toggle: X\r\nIndividual Enemy Path Cycle: L or R";
        }
        else if (DebugMenuManager.Instance.PlayStationController)
        {
            _tutorialText.text = "Collect notes in ascending order\r\nMove: Left control stick or D-pad\r\n" +
                "Restart Level: Share\r\nPause: Options\r\nEnemy Path Toggle: Triangle\r\nIndividual Enemy Path Cycle: L1 or R1";
        }
        else
        {
            _tutorialText.text = "Collect notes in ascending order\r\nMove: Left control stick or D-pad\r\n" +
                "Restart Level: Select\r\nPause: Start\r\nEnemy Path Toggle: Y\r\nIndividual Enemy Path Cycle: LB or RB";
        }
    }
}
