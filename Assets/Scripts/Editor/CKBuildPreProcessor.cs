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
        // // Find valid Scene paths and make a list of EditorBuildSettingsScene
        AddScenesToBuild();
        var levelData = LevelOrder.instance;
        var loadedScene = EditorSceneManager.GetActiveScene();
        for (var chapterIndex = 0; chapterIndex < levelData.Chapters.Count; chapterIndex++)
        {
            var chapter = levelData.Chapters[chapterIndex];
            var intro = chapter.Intro;
            if (intro.Scene != null)
            {
                //apply transitions for intro
            }

            UpdatePuzzleExits(chapter, chapterIndex, levelData);

            var outro = chapter.Outro;
            if (outro.Scene != null)
            {
                //apply transitions for outro
            }
        }
    }

    private static void UpdatePuzzleExits(LevelOrder.Chapter chapter, int chapterIndex, LevelOrder levelData)
    {
        for (int puzzleIndex = 0; puzzleIndex < chapter.Puzzles.Count; puzzleIndex++)
        {
            var currentLevel = chapter.Puzzles[puzzleIndex];

            //determine exit scene
            SceneAsset nextScene = null;
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

            Debug.Log(nextScene.name);
            if (bonusScene != null) Debug.Log(bonusScene.name);
            //load in the scene here
            var currScene = EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(currentLevel.Scene));
            var doors = Object.FindObjectsOfType<EndLevelDoor>();
            foreach (var endLevelDoor in doors)
            {
                Debug.Log(endLevelDoor.gameObject.name);
                if (endLevelDoor.isActiveAndEnabled)
                {
                    //challenge door TODO: do something besides checking gameobject name for this.
                    bool isBonusDoor = endLevelDoor.name.ToLower().Contains("challenge");
                    //use reflection to set the next level index to the next scene. 
                    endLevelDoor.GetType().GetField("_levelIndexToLoad",
                            BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(endLevelDoor,
                        SceneUtility.GetBuildIndexByScenePath(
                            AssetDatabase.GetAssetPath(isBonusDoor ? bonusScene : nextScene)));
                }
            }

            //save the changes
            EditorSceneManager.SaveScene(currScene);

            //close the scene
            EditorSceneManager.CloseScene(currScene, true);
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

        EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
    }
}