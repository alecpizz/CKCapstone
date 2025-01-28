using System;
using System.Collections.Generic;
using SaintsField;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelOrder", menuName = "ScriptableObjects/Level Order", order = 0)]
public class LevelOrder : ScriptableSingleton<LevelOrder>
{
    [field: SerializeField] public SceneAsset MainMenuScene { get; private set; }

    [Serializable]
    public class Chapter
    {
        [field: SerializeField] public string ChapterName { get; internal set; } = "Chapter CHANGEME";

        [field: SerializeField, SepTitle("Intro Level", EColor.Green), SaintsRow(inline: true)]
        public LevelData Intro { get; internal set; } = new();

        [field: SerializeField, SepTitle("Puzzles", EColor.Yellow)]
        public List<LevelData> Puzzles { get; private set; } = new();

        [field: SerializeField, SepTitle("Outro Level", EColor.Red), SaintsRow(inline: true)]
        public LevelData Outro { get; internal set; } = new();
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

    [field: SerializeField, ListDrawerSettings(searchable: true, 2)]
    public List<Chapter> Chapters { get; private set; } = new();
    [field: SerializeField] public SceneAsset CreditsScene { get; private set; }

    [PlayaInfoBox("Chapter Creation", EMessageType.Info)] [SerializeField]
    private string _chapterName = "Chapter CHANGEME";


    [SepTitle("Level Input", EColor.White)]
    [SerializeField]
    [BelowSeparator("Adding to Chapter", EColor.White)]
    [SaintsRow(inline: true)]
    [BelowButton(nameof(AddLevel), groupBy: "Chapter")]
    [BelowButton(nameof(SetIntro), groupBy: "Chapter")]
    [BelowButton(nameof(SetOutro), groupBy: "Chapter")]
    [BelowButton(nameof(ClearLevels), groupBy: "Chapter2")]
    [BelowButton(nameof(ClearIntro), groupBy: "Chapter2")]
    [BelowButton(nameof(ClearOutro), groupBy: "Chapter2")]
    [BelowButton(nameof(RunBuildPreProcess), groupBy: "Build")]
    private LevelData _inputLevelData;


    private void AddLevel()
    {
        var chapter = TryGetOrAddChapter(_chapterName);
        Debug.Log("add level");
        chapter.Puzzles.Add(_inputLevelData);
        EditorUtility.SetDirty(this);
        Undo.RecordObject(this, "Add Level");
    }

    private void SetIntro()
    {
        var chapter = TryGetOrAddChapter(_chapterName);
        chapter.Intro = _inputLevelData;
        EditorUtility.SetDirty(this);
        Undo.RecordObject(this, "Set Intro");
    }

    private void ClearIntro()
    {
        var chapter = TryGetOrAddChapter(_chapterName);
        chapter.Intro = null;
        EditorUtility.SetDirty(this);
        Undo.RecordObject(this, "Clear Intro");
    }

    private void SetOutro()
    {
        var chapter = TryGetOrAddChapter(_chapterName);
        chapter.Outro = _inputLevelData;
        EditorUtility.SetDirty(this);
        Undo.RecordObject(this, "Set Outro");
    }

    private void ClearOutro()
    {
        var chapter = TryGetOrAddChapter(_chapterName);
        chapter.Outro = null;
        EditorUtility.SetDirty(this);
        Undo.RecordObject(this, "Clear Intro");
    }

    private void RunBuildPreProcess()
    {
        if (EditorUtility.DisplayDialog("Build Pre-Process Warning",
                "This may take a moment, are you sure you wish to run the build pre-process?", "Yes",
                "No, take me back!"))
        {
            CKBuildPreProcessor.BuildSceneIndex();
        }
    }

    private void ClearLevels()
    {
        var chapter = TryGetOrAddChapter(_chapterName);
        chapter.Puzzles.Clear();
        EditorUtility.SetDirty(this);
        Undo.RecordObject(this, "Clear Levels");
    }

    private Chapter TryGetOrAddChapter(string chapterName)
    {
        var chapter = Chapters.Find(p =>
            string.Equals(p.ChapterName, chapterName, StringComparison.CurrentCultureIgnoreCase));
        if (chapter != null)
        {
            return chapter;
        }

        chapter = new Chapter()
        {
            ChapterName = chapterName
        };
        Chapters.Add(chapter);

        return chapter;
    }
}