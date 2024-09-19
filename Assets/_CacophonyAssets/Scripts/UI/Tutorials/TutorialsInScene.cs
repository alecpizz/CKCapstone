using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: Liz
/// Description: Holds scene-specific Tutorial data. Separated from "TutorialUI" for convenience.
/// </summary>
public class TutorialsInScene : MonoBehaviour
{
    [SerializeField] private List<TutorialSO> _sceneTutorials = new List<TutorialSO>();

    public List<TutorialSO> GetSceneTutorials()
    {
        return _sceneTutorials;
    }
}
