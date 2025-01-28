using System;
using System.Collections.Generic;
using SaintsField;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelOrder", menuName = "ScriptableObjects/Level Order", order = 0)]
public class LevelOrder : ScriptableSingleton<LevelOrder>
{
    [Serializable]
    public class Chapter
    {
        [field: SerializeField] public string ChapterName { get; private set; } = "Chapter CHANGEME";
        [field: SerializeField, SepTitle("Intro Level", EColor.Green), SaintsRow(inline:true)] public LevelData Intro { get; private set; } = new();
        [field: SerializeField, SepTitle("Puzzles", EColor.Yellow)] public List<LevelData> Levels { get; private set; } = new();
        [field: SerializeField, SepTitle("Outro Level", EColor.Red), SaintsRow(inline:true)] public LevelData Outro { get; private set; } = new();
    }
    [Serializable]
    public class LevelData
    {
        [field: SerializeField] public string LevelName { get; private set; } = "CHANGEME";
        [field: SerializeField] public SceneAsset Scene { get; private set; }
        [field: SerializeField] public bool UseNextLevelInListAsExit { get; private set; } = true;

        [field: SerializeField, HideIf(nameof(UseNextLevelInListAsExit))]
        public SceneAsset ExitScene { get; private set; }

        [field: SerializeField] public bool HasChallengeExit { get; private set; } = false;

        [field: SerializeField, ShowIf(nameof(HasChallengeExit))]
        public SceneAsset ChallengeScene { get; private set; }
    }

    [field: SerializeField]
    public List<LevelData> Levels { get; private set; } = new();
    [field: SerializeField]
    public List<Chapter> Chapters { get; private set; } = new();

    [SepTitle("Level Input", EColor.White)] [SerializeField] 
    [BelowButton(nameof(AddLevel), groupBy: "Build")]
    [BelowButton(nameof(RunBuildPreProcess), groupBy: "Build")]
    [BelowButton(nameof(ClearLevels), groupBy: "Build")]
    [SaintsRow(inline:true)]
    private LevelData _inputLevelData;

    
    private void AddLevel()
    {
        Levels.Add(_inputLevelData);
        EditorUtility.SetDirty(this);
    }
    
    private void RunBuildPreProcess()
    {
        CKBuildPreProcessor.BuildSceneIndex();
    }

    private void ClearLevels()
    {
        Levels.Clear();
        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(EditorWindow.focusedWindow);
    }
}