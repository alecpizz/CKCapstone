/******************************************************************
 *    Author: Alec Pizziferro
 *    Contributors:  nullptr
 *    Date Created: 1/30/2025
 *    Description: A build pre-processor for linking scenes together
 *    automatically.
 *******************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Player;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class CKBuildPreProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    /// <summary>
    /// Called when the build starts. Will prompt the user for the scene linking,
    /// and if so will begin linking all of the scnes.
    /// </summary>
    /// <param name="report"></param>
    public void OnPreprocessBuild(BuildReport report)
    {
        Debug.Log($"<color=green>Starting Build for platform {report.summary.platform}</color>");
        if (EditorUtility.DisplayDialog("Build Pre-Process Warning",
                "This may take a moment, are you sure you wish to run the scene linking?", "Yes",
                "No, skip the scene linking."))
        {
            //apply build scenes
            BuildSceneIndex();
        }

        ToggleUnlockedDefines();
    }

    /// <summary>
    /// Toggles the unlocked level define.
    /// </summary>
    [MenuItem("Tools/Crowded Kitchen/Toggle Unlocked Levels")]
    public static void ToggleUnlockedDefines()
    {
        var buildTarget = NamedBuildTarget.FromBuildTargetGroup(
            BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
        List<string> symbols = PlayerSettings.GetScriptingDefineSymbols(buildTarget).Split(';').ToList();
        bool hasOverride = symbols.Any(symbol => symbol.ToUpper() == "OVERRIDE_LEVEL");
        if (EditorUtility.DisplayDialog("Build Pre-Process Question",
                "Do you wish to build with all levels in level select unlocked?", "yes", "no"))
        {
            if (!hasOverride)
            {
                symbols.Add("OVERRIDE_LEVEL");
                Debug.Log("<color=green>Added</color> define");
            }
        }
        else
        {
            if (hasOverride)
            {
                symbols.Remove("OVERRIDE_LEVEL");
                Debug.Log("<color=red>Removed</color> define");
            }
        }

        PlayerSettings.SetScriptingDefineSymbols(buildTarget, symbols.ToArray());
    }

    /// <summary>
    /// Goes through the entire project and links
    /// all scenes together, as well as add them to the
    /// build index.
    /// </summary>
    [MenuItem("Tools/Crowded Kitchen/Run Scene Linking")]
    public static void BuildSceneIndex()
    {
        // var nowOpenScene = EditorSceneManager.GetActiveScene();
        // // Find valid Scene paths and make a list of EditorBuildSettingsScene
        AddScenesToBuild();
        var levelData = LevelOrderSelection.Instance.SelectedLevelData;

        //set the next level from the main menu to load to the first level of the first chapter.
        var menuScene = EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(levelData.MainMenuScene));
        var menuManager = Object.FindObjectOfType<MenuManager>();
        if (menuManager != null)
        {
            //use reflection to set the menu manager's load value
            var field = menuManager.GetType()
                .GetField("_firstLevelIndex", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null)
            {
                int index = SceneUtility.GetBuildIndexByScenePath(
                    AssetDatabase.GetAssetPath(levelData.Chapters[0].GetStartingLevel.Scene));
                field.SetValue(menuManager, index);
            }
            else
            {
                Debug.LogError("Missing field _firstLevelIndex!");
            }
        }

        EditorSceneManager.SaveScene(menuScene);

        //loop thru each scene, apply puzzle exits
        for (var chapterIndex = 0; chapterIndex < levelData.Chapters.Count; chapterIndex++)
        {
            var chapter = levelData.Chapters[chapterIndex];
            //very gross switch statement, none & custom are unused right now
            Action<LightingData> lightDataMethod = null;
            switch (chapter.Lighting)
            {
                case LevelOrder.LightingMode.None:
                    break;
                case LevelOrder.LightingMode.Chapter1:
                    lightDataMethod = CKLightingEditor.ApplyChapter1Lighting;
                    break;
                case LevelOrder.LightingMode.Chapter2:
                    lightDataMethod = CKLightingEditor.ApplyChapter2Lighting;
                    break;
                case LevelOrder.LightingMode.Chapter3:
                    lightDataMethod = CKLightingEditor.ApplyChapter3Lighting;
                    break;
                case LevelOrder.LightingMode.Chapter4:
                    lightDataMethod = CKLightingEditor.ApplyChapter4Lighting;
                    break;
                case LevelOrder.LightingMode.Chapter5:
                    lightDataMethod = CKLightingEditor.ApplyChapter5Lighting;
                    break;
                case LevelOrder.LightingMode.Custom:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            var intro = chapter.Intro;
            SetOpenerCloserSceneExit(intro, chapter.Puzzles[0].Scene, lightDataMethod);
            UpdatePuzzles(chapterIndex, lightDataMethod);

            var outro = chapter.Outro;
            var outroExit = chapterIndex != levelData.Chapters.Count - 1
                ? levelData.Chapters[chapterIndex + 1].GetStartingLevel.Scene
                : levelData.CreditsScene;
            SetOpenerCloserSceneExit(outro, outroExit, lightDataMethod);
        }

        // EditorSceneManager.OpenScene(nowOpenScene.path, OpenSceneMode.Single);
    }

    /// <summary>
    /// Sets the scene links for scenes that are a opener/closer.
    /// Typically these will have a cutscene framework object in the scene
    /// or something similar. 
    /// </summary>
    /// <param name="entrance">The scene to modify.</param>
    /// <param name="scene">The destination scene to link towards.</param>
    private static void SetOpenerCloserSceneExit(LevelOrder.LevelData entrance, SceneAsset scene, Action<LightingData> lightingDataAction)
    {
        if (entrance.Scene == null) return;
        //apply transitions for intro/outro
        //currently handled data types: CutsceneFramework, EndLevelDoor.
        //TODO: wrap this in an interface
        var introScene = EditorSceneManager.OpenScene(
            AssetDatabase.GetAssetPath(entrance.Scene));
        var cutsceneFrameWork = Object.FindObjectOfType<CutsceneFramework>();
        if (cutsceneFrameWork != null)
        {
            var field = cutsceneFrameWork.GetType().GetField("_loadingLevelIndex",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(cutsceneFrameWork,
                    SceneUtility.GetBuildIndexByScenePath(
                        AssetDatabase.GetAssetPath(scene)));
            }
            else
            {
                Debug.LogError("Field _loadingLevelIndex not found on cutsceneFramework!");
            }
        }

        //apply transitions to any doors in the scene as well. I dunno how every scene will work :)
        var doors = Object.FindObjectsOfType<EndLevelDoor>();
        foreach (var endLevelDoor in doors)
        {
            SetDoorExitScene(endLevelDoor, SceneUtility.GetBuildIndexByScenePath(
                AssetDatabase.GetAssetPath(scene)));
        }
        lightingDataAction?.Invoke(entrance.LightingData);

        EditorSceneManager.SaveScene(introScene);
    }

    /// <summary>
    /// Goes through a chapter's puzzles and updates each of their exit locations.
    /// </summary>
    /// <param name="chapterIndex">The index of the chapter that is being modified.</param>
    private static void UpdatePuzzles(int chapterIndex, Action<LightingData> lightingDataApply)
    {
        var levelData = LevelOrderSelection.Instance.SelectedLevelData;
        var chapter = levelData.Chapters[chapterIndex];
       
        
        for (int puzzleIndex = 0; puzzleIndex < chapter.Puzzles.Count; puzzleIndex++)
        {
            var currentLevel = chapter.Puzzles[puzzleIndex];

            //determine exit scene
            SceneAsset nextScene;
            SceneAsset bonusScene = null;
            if (currentLevel.UseNextLevelInListAsExit)
            {
                //still in the list, grab the next one
                if (puzzleIndex != chapter.Puzzles.Count - 1)
                {
                    nextScene = chapter.Puzzles[puzzleIndex + 1].Scene;
                }
                else
                {
                    //last level, go to outro
                    if (chapter.Outro.Scene != null)
                    {
                        nextScene = chapter.Outro.Scene;
                    }
                    //use next chapter intro
                    else if (chapterIndex != levelData.Chapters.Count - 1)
                    {
                        nextScene = levelData.Chapters[chapterIndex + 1].GetStartingLevel.Scene;
                    }
                    //loop to end of the game
                    else
                    {
                        nextScene = levelData.CreditsScene;
                    }
                }
            }
            else //user provided a scene to exit to.
            {
                nextScene = currentLevel.ExitScene;
            }

            if (currentLevel.HasChallengeExit)
            {
                bonusScene = currentLevel.ChallengeScene;
            }

            //load in the scene here
            if (currentLevel.Scene == null)
            {
                Debug.LogError($"Missing scene! {currentLevel.LevelName}");
                continue;
            }

            var currScene = EditorSceneManager.OpenScene(
                AssetDatabase.GetAssetPath(currentLevel.Scene));
            var doors = Object.FindObjectsOfType<EndLevelDoor>();

            //apply door exits
            if (doors.Length > 2)
            {
                Debug.LogWarning("There are more than 2 doors in this scene. " +
                                 "There may be duplicate exits...");
            }

            foreach (var endLevelDoor in doors)
            {
                if (!endLevelDoor.gameObject.activeSelf) continue;
                //challenge door TODO: do something besides checking gameobject name for this.
                bool isBonusDoor = endLevelDoor.name.ToLower().Contains("challenge");
                //use reflection to set the next level index to the next scene. 
                int index;
                if (bonusScene != null && isBonusDoor)
                {
                    index = SceneUtility.GetBuildIndexByScenePath(
                        AssetDatabase.GetAssetPath(bonusScene));
                }
                else
                {
                    index = SceneUtility.GetBuildIndexByScenePath(
                        AssetDatabase.GetAssetPath(nextScene));
                }

                SetDoorExitScene(endLevelDoor, index);
                
                

                EditorUtility.SetDirty(endLevelDoor);
            }
            
            var cutsceneFrameWork = Object.FindObjectOfType<CutsceneFramework>();
            if (cutsceneFrameWork != null)
            {
                var field = cutsceneFrameWork.GetType().GetField("_loadingLevelIndex",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null)
                {
                    field.SetValue(cutsceneFrameWork,
                        SceneUtility.GetBuildIndexByScenePath(
                            AssetDatabase.GetAssetPath(nextScene)));
                }
                else
                {
                    Debug.LogError("Field _loadingLevelIndex not found on cutsceneFramework!");
                }
            }
            else
            {
                Debug.Log($"NO CUTSCENE FRAMEWORK IN SCENE {currScene.name}");
            }
            
            lightingDataApply?.Invoke(currentLevel.LightingData);

            EditorSceneManager.MarkSceneDirty(currScene);
            //save the changes
            EditorSceneManager.SaveScene(currScene);
        }
    }

    /// <summary>
    /// Use reflection to set a door's exit scene.
    /// </summary>
    /// <param name="door">The door to modify.</param>
    /// <param name="index">Destination scene index.</param>
    private static void SetDoorExitScene(EndLevelDoor door, int index)
    {
        if (!door.gameObject.activeSelf) return;
        var field = door.GetType().GetField("_levelIndexToLoad", BindingFlags.Instance | BindingFlags.NonPublic);
        if (field != null)
        {
            field.SetValue(door, index);
        }
        else
        {
            Debug.LogError($"Missing field _levelIndexToLoad");
        }

        EditorUtility.SetDirty(door);
    }

    /// <summary>
    /// Adds every scene in the level data to the build index.
    /// </summary>
    private static void AddScenesToBuild()
    {
        //TODO: double check levels aren't being included twice lol
        List<EditorBuildSettingsScene> editorBuildSettingsScenes =
            new List<EditorBuildSettingsScene>();
        var levelData = LevelOrderSelection.Instance.SelectedLevelData;
        //add the main menu scene
        editorBuildSettingsScenes.Add(
            new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(levelData.MainMenuScene),
                true));
        //add each chapter's data
        int chapterIndex = 0;
        foreach (var chapter in levelData.Chapters)
        {
            //add intro scene
            if (chapter.Intro.Scene != null)
            {
                editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(
                    AssetDatabase.GetAssetPath(chapter.Intro.Scene),
                    true));
                chapter.Intro.ScenePath = editorBuildSettingsScenes[^1].path;
            }

            //add all puzzles
            foreach (var level in chapter.Puzzles)
            {
                if (level.Scene == null) continue;
                editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(
                    AssetDatabase.GetAssetPath(level.Scene),
                    true));
                level.ScenePath = editorBuildSettingsScenes[^1].path;
            }

            //add outro scene
            if (chapter.Outro.Scene != null)
            {
                editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(
                    AssetDatabase.GetAssetPath(chapter.Outro.Scene),
                    true));
                chapter.Outro.ScenePath = editorBuildSettingsScenes[^1].path;
            }
        }

        //add the final credits scene.
        if (levelData.CreditsScene == null)
        {
            Debug.LogError("No credits scene assigned!");
        }
        else
        {
            editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(
                AssetDatabase.GetAssetPath(levelData.CreditsScene),
                true));
        }

        EditorUtility.SetDirty(levelData);
        Debug.Log($"Added {editorBuildSettingsScenes.Count} Scenes");
        EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
    }
}