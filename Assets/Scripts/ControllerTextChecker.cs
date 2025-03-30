
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ControllerTextChecker : MonoBehaviour
{
    private Scene currentScene;
    [SerializeField] private TextMeshProUGUI _tutorialText;

    // Start is called before the first frame update
    void Start()
    {
        currentScene = SceneManager.GetActiveScene();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentScene.name == "T_FirstDay")
        {
            if (DebugMenuManager.Instance.KeyboardAndMouse)
            {
                _tutorialText.text = "Use WASD or the arrow keys to move.";
            }
            else
            {
                _tutorialText.text = "Use the left control stick or the D-pad to move.";
            }
        }
        else
        {
            return;
        }
    }
}
