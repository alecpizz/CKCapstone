/******************************************************************
 *    Author: Alec Pizziferro
 *    Contributors:  Josephine Qualls
 *    Date Created: 1/30/2025
 *    Description: A scriptable object for handling level ordering.
 *    contains tools to add scenes quicker. This is a EDITOR ONLY
 *    class. It is also a singleton.
 *******************************************************************/

using System;
using System.Collections.Generic;
using SaintsField;
using SaintsField.Playa;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;

[CreateAssetMenu(fileName = "LevelOrder", menuName = "ScriptableObjects/Level Order", order = 0)]
public class LevelOrder : ScriptableObject
{
#if UNITY_EDITOR
    [field: SerializeField, SepTitle("Main Menu", EColor.Aqua), BelowSeparator(EColor.Aqua)]
    public SceneAsset MainMenuScene { get; private set; }
    [field: SerializeField]
    public SceneAsset IntroCutScene { get; private set; }
#endif
    public enum LightingMode
    {
        None = -1,
        Chapter1,
        Chapter2,
        Chapter3,
        Chapter4,
        Chapter5,
        Custom
    }
    
    /// <summary>
    /// A class representing a chapter.
    /// A chapter contains:
    ///     - The chapter name
    ///     - The Intro Level or Scene
    ///     - The puzzles in the chapter
    ///     - The Outro Level or Scene
    /// </summary>
    [Serializable]
    public class Chapter
    {
        [field: SerializeField]
        public LightingMode Lighting { get; internal set; } = LightingMode.None;
        [field: SerializeField] public string ChapterName { get; internal set; } = "Chapter CHANGEME";

        [field: SerializeField, SepTitle("Intro Level", EColor.Green), SaintsRow(inline: true)]
        public LevelData Intro { get; internal set; } = new();

        [field: SerializeField, SepTitle("Puzzles", EColor.Yellow)]
        public List<LevelData> Puzzles { get; private set; } = new();

        [field: SerializeField, SepTitle("Outro Level", EColor.Red), SaintsRow(inline: true)]
        public LevelData Outro { get; internal set; } = new();
        #if UNITY_EDITOR
        public LevelData GetStartingLevel => Intro.Scene ? Intro : Puzzles[0];
        #endif
    }

    /// <summary>
    /// A representation of level data.
    /// A level data contains:
    ///     - The name of the level
    ///     - The scene asset of the level itself.
    ///     - Some determination of whether or not the scene should
    ///       exit to the next level or a custom target.
    ///     - A potential challenge scene.
    /// </summary>
    [Serializable]
    public class LevelData
    {
        [field: SerializeField] public string LevelName { get; private set; } = "CHANGEME";
        #if UNITY_EDITOR
        [field: SerializeField] public SceneAsset Scene { get; internal set; }
        #endif
        [field: SerializeField] public string ScenePath { get;  set; }
        [field: SerializeField] public bool UseNextLevelInListAsExit { get; private set; } = true;

        #if UNITY_EDITOR
        [field: SerializeField, HideIf(nameof(UseNextLevelInListAsExit))]
        public SceneAsset ExitScene { get; private set; }
        #endif
        [field: SerializeField] public bool HasChallengeExit { get; private set; } = false;

        #if UNITY_EDITOR
        [field: SerializeField, ShowIf(nameof(HasChallengeExit))]
        public SceneAsset ChallengeScene { get; private set; }
#endif
        [field: SerializeField] public LightingData LightingData { get; private set; } = new();

        /// <summary>
        /// Copy constructor for the level data.
        /// </summary>
        /// <param name="other">The level data to copy from.</param>
        public LevelData(LevelData other)
        {
            UseNextLevelInListAsExit = other.UseNextLevelInListAsExit;
            LevelName = other.LevelName;
            HasChallengeExit = other.HasChallengeExit;
            #if UNITY_EDITOR
            Scene = other.Scene;
            ExitScene = other.ExitScene;
            ChallengeScene = other.ChallengeScene;
            #endif
            ScenePath = other.ScenePath;
            LightingData = other.LightingData;
        }

        /// <summary>
        /// Empty constructor for level data. 
        /// </summary>
        public LevelData()
        {
            LevelName = "";
            #if UNITY_EDITOR
            Scene = null;
            ExitScene = null;
            ChallengeScene = null;
            #endif
        }
    }

    [field: SerializeField, ListDrawerSettings(searchable: true, 5)]
    public List<Chapter> Chapters { get; private set; } = new();
#if UNITY_EDITOR
    [field: SerializeField, SepTitle("Credits Scene", EColor.Magenta), BelowSeparator(EColor.Magenta)]
    public SceneAsset CreditsScene { get; private set; }
#endif
    [PlayaInfoBox("Chapter Creation", EMessageType.Info)] [SerializeField]
    private string _chapterName = "Chapter CHANGEME";


    //holy attributes batman! This just handles the buttons below the field.
    #if UNITY_EDITOR
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
    #endif
    // [BelowButton(nameof(RunBuildPreProcess), groupBy: "Build")]
    private LevelData _inputLevelData;


#if UNITY_EDITOR
    /// <summary>
    /// Add a level to the current chapter.
    /// Editor only
    /// </summary>
    private void AddLevel()
    {
        var chapter = TryGetOrAddChapter(_chapterName);
        chapter.Puzzles.Add(new LevelData(_inputLevelData));
        EditorUtility.SetDirty(this);
        Undo.RecordObject(this, "Add Level");
    }

    /// <summary>
    /// Clears the levels in a chapter.
    /// </summary>
    private void ClearLevels()
    {
        var chapter = TryGetOrAddChapter(_chapterName);
        chapter.Puzzles.Clear();
        EditorUtility.SetDirty(this);
        Undo.RecordObject(this, "Clear Levels");
    }

    /// <summary>
    /// Set the intro level for a chapter.
    /// </summary>
    private void SetIntro()
    {
        var chapter = TryGetOrAddChapter(_chapterName);
        chapter.Intro = new LevelData(_inputLevelData);
        EditorUtility.SetDirty(this);
        Undo.RecordObject(this, "Set Intro");
    }

    /// <summary>
    /// Clears the intro for a chapter
    /// </summary>
    private void ClearIntro()
    {
        var chapter = TryGetOrAddChapter(_chapterName);
        chapter.Intro = null;
        EditorUtility.SetDirty(this);
        Undo.RecordObject(this, "Clear Intro");
    }

    /// <summary>
    /// Sets the outro for a chapter.
    /// </summary>
    private void SetOutro()
    {
        var chapter = TryGetOrAddChapter(_chapterName);
        chapter.Outro = new LevelData(_inputLevelData);
        EditorUtility.SetDirty(this);
        Undo.RecordObject(this, "Set Outro");
    }

    /// <summary>
    /// Clears the outro for a chapter.
    /// </summary>
    private void ClearOutro()
    {
        var chapter = TryGetOrAddChapter(_chapterName);
        chapter.Outro = null;
        EditorUtility.SetDirty(this);
        Undo.RecordObject(this, "Clear Intro");
    }

    // /// <summary>
    // /// Runs the build linking step manually.
    // /// </summary>
    // private void RunBuildPreProcess()
    // {
    //     EditorUtility.RequestScriptReload();
    //     if (EditorUtility.DisplayDialog("Build Pre-Process Warning",
    //             "This may take a moment, are you sure you wish to run the build pre-process?", "Yes",
    //             "No, take me back!"))
    //     {
    //         CKBuildPreProcessor.BuildSceneIndex();
    //     }
    // }

    /// <summary>
    /// Will grab or add the chapter by name.
    /// </summary>
    /// <param name="chapterName">The name of the chapter to search for, case insensitive.</param>
    /// <returns>The newly created or found chapter.</returns>
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
#endif
}