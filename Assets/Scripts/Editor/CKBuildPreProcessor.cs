using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CKBuildPreProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        Debug.Log($"Starting Build for platform {report.summary.platform}");

        //apply build scenes
        BuildSceneIndex();
    }


    [MenuItem("Tools/Crowded Kitchen/Preview Build Order")]
    public static void BuildSceneIndex()
    {
        var nowOpenScene = EditorSceneManager.GetActiveScene();
        // // Find valid Scene paths and make a list of EditorBuildSettingsScene
        AddScenesToBuild();
        var levelData = LevelOrder.instance;

        //set the next level from the main menu to load to the first level of the first chapter.
        var menuScene = EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(levelData.MainMenuScene));
        var menuManager = Object.FindObjectOfType<MenuManager>();
        if (menuManager != null)
        {
            //use reflection to set the menu manager's load value
            var field = menuManager.GetType().GetField("_firstLevelIndex");
            if (field != null)
            {
                int index = SceneUtility.GetBuildIndexByScenePath(
                    AssetDatabase.GetAssetPath(levelData.Chapters[0].GetStartingLevel.Scene));
                field.SetValue(menuManager, index);
            }
        }

        EditorSceneManager.SaveScene(menuScene);

        //loop thru each scene, apply puzzle exits
        for (var chapterIndex = 0; chapterIndex < levelData.Chapters.Count; chapterIndex++)
        {
            var chapter = levelData.Chapters[chapterIndex];
            var intro = chapter.Intro;
            if (intro.Scene != null)
            {
                //apply transitions for intro
                var introScene = EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(intro.Scene));
                var cutsceneFrameWork = Object.FindObjectOfType<CutsceneFramework>();
                if (cutsceneFrameWork != null)
                {
                    var field = cutsceneFrameWork.GetType().GetField("_loadingLevelIndex");
                    if (field != null)
                    {
                        field.SetValue(cutsceneFrameWork,
                            SceneUtility.GetBuildIndexByScenePath(
                                AssetDatabase.GetAssetPath(chapter.Puzzles[0].Scene)));
                    }
                    else
                    {
                        Debug.LogError("Field _loadingLevelIndex not found on cutsceneFramework!");
                    }
                }

                EditorSceneManager.SaveScene(introScene);
            }

            UpdatePuzzleExits(chapter, chapterIndex, levelData);

            var outro = chapter.Outro;
            if (outro.Scene != null)
            {
                //apply transitions for outro
                //apply transitions for intro
                var outroScene = EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(outro.Scene));
                var cutsceneFrameWork = Object.FindObjectOfType<CutsceneFramework>();
                if (cutsceneFrameWork != null)
                {
                    var field = cutsceneFrameWork.GetType().GetField("_loadingLevelIndex");
                    if (field != null)
                    {
                        //set to last chapter
                        field.SetValue(cutsceneFrameWork,
                            SceneUtility.GetBuildIndexByScenePath(
                                AssetDatabase.GetAssetPath(chapterIndex != levelData.Chapters.Count - 1
                                    ? levelData.Chapters[chapterIndex + 1].GetStartingLevel.Scene
                                    : levelData.CreditsScene)));
                    }
                    else
                    {
                        Debug.LogError("Field _loadingLevelIndex not found on cutsceneFramework!");
                    }
                }

                EditorSceneManager.SaveScene(outroScene);
            }
        }
    }

    private static void UpdatePuzzleExits(LevelOrder.Chapter chapter, int chapterIndex, LevelOrder levelData)
    {
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
                    else if (chapterIndex != levelData.Chapters.Count - 1)
                    {
                        //use next chapter intro
                        nextScene = levelData.Chapters[chapterIndex + 1].GetStartingLevel.Scene;
                    }
                    else //loop to end of the game
                    {
                        nextScene = levelData.CreditsScene;
                    }
                }
            }
            else
            {
                nextScene = currentLevel.ExitScene;
            }

            if (currentLevel.HasChallengeExit)
            {
                bonusScene = currentLevel.ChallengeScene;
            }

            //load in the scene here
            var currScene = EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(currentLevel.Scene));
            var doors = Object.FindObjectsOfType<EndLevelDoor>();

            foreach (var endLevelDoor in doors)
            {
                if (endLevelDoor.gameObject.activeSelf)
                {
                    //challenge door TODO: do something besides checking gameobject name for this.
                    bool isBonusDoor = endLevelDoor.name.ToLower().Contains("challenge");
                    //use reflection to set the next level index to the next scene. 
                    int index;
                    if (bonusScene != null && isBonusDoor)
                    {
                        index = SceneUtility.GetBuildIndexByScenePath(AssetDatabase.GetAssetPath(bonusScene));
                    }
                    else
                    {
                        index = SceneUtility.GetBuildIndexByScenePath(AssetDatabase.GetAssetPath(nextScene));
                    }

                    var field = endLevelDoor.GetType().GetField("_levelIndexToLoad");
                    if (field != null)
                    {
                        field.SetValue(endLevelDoor, index);
                    }
                    else
                    {
                        Debug.Log($"Missing field _levelIndexToLoad");
                    }

                    EditorUtility.SetDirty(endLevelDoor);
                }
            }


            var levelText = GameObject.Find("Level Number");
            if (levelText != null)
            {
                levelText.GetComponent<TMPro.TMP_Text>().text = currentLevel.LevelName;
            }

            EditorSceneManager.MarkSceneDirty(currScene);
            //save the changes
            EditorSceneManager.SaveScene(currScene);
        }
    }

    private static void AddScenesToBuild()
    {
        List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>();
        var levelData = LevelOrder.instance;
        //add the main menu scene
        editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(levelData.MainMenuScene),
            true));

        foreach (var chapter in levelData.Chapters)
        {
            //add intro scene
            if (chapter.Intro.Scene != null)
            {
                editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(
                    AssetDatabase.GetAssetPath(chapter.Intro.Scene),
                    true));
            }

            //add all puzzles
            foreach (var level in chapter.Puzzles)
            {
                if (level.Scene == null) continue;
                editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(
                    AssetDatabase.GetAssetPath(level.Scene),
                    true));
            }

            //add outro scene
            if (chapter.Outro.Scene != null)
            {
                editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(
                    AssetDatabase.GetAssetPath(chapter.Outro.Scene),
                    true));
            }
        }

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

        Debug.Log($"Added {editorBuildSettingsScenes.Count} Scenes");
        EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
    }
}