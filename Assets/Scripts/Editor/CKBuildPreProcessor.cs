/******************************************************************
 *    Author: Alec Pizziferro
 *    Contributors:  nullptr
 *    Date Created: 1/30/2025
 *    Description: A build pre-processor for linking scenes together
 *    automatically.
 *******************************************************************/

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
    }

    /// <summary>
    /// Goes through the entire project and links
    /// all scenes together, as well as add them to the
    /// build index.
    /// </summary>
    [MenuItem("Tools/Crowded Kitchen/Run Scene Linking")]
    public static void BuildSceneIndex()
    {
        var nowOpenScene = EditorSceneManager.GetActiveScene();
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
            var intro = chapter.Intro;
            SetOpenerCloserSceneExit(intro, chapter.Puzzles[0].Scene);
            UpdatePuzzleExits(chapterIndex);

            var outro = chapter.Outro;
            var outroExit = chapterIndex != levelData.Chapters.Count - 1
                ? levelData.Chapters[chapterIndex + 1].GetStartingLevel.Scene
                : levelData.CreditsScene;
            SetOpenerCloserSceneExit(outro, outroExit);
        }
    }

    [MenuItem("Tools/Crowded Kitchen/Light Level/Chapter 1 Lighting")]
    public static void ApplyChapter1Lighting()
    {
        //testing chapter 1 here
        Material skybox = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Materials/Lighting/MAT_Skybox_1.mat");
        if (skybox == null)
        {
            Debug.LogError("Couldn't find skybox!");
            return;
        }

        RenderSettings.skybox = skybox;

        RenderSettings.ambientIntensity = 0.5f;
        TryDestroyEnvironmentArt();
        var fxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/VFX/Environment/EnvFX_Chapter1.prefab");
        var fx = Object.Instantiate(fxPrefab);
        var godRays = fx.transform.Find("GodRays");
        var godParticles = godRays.GetComponent<ParticleSystem>();
        //set the alpha of the particle
        const float newGodRayAlpha = 1.0f;
        AdjustGodRays(godParticles, newGodRayAlpha);
        //set mote size
        
        AdjustMotes(fx);
        //need to adjust god ray position
    }

    private static void AdjustGodRays(ParticleSystem godParticles, float newGodRayAlpha)
    {
        var main = godParticles.main;
        var startColor = main.startColor;
        var startColorColor = startColor.color;
        startColorColor.a = newGodRayAlpha;
        startColor.color = startColorColor;
        main.startColor = startColor;
    }


    [MenuItem("Tools/Crowded Kitchen/Light Level/Chapter 2 Lighting")]
    public static void ApplyChapter2Lighting()
    {
        Material skybox = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Materials/Lighting/MAT_Skybox_2.mat");
        if (skybox == null)
        {
            Debug.LogError("Couldn't find skybox!");
            return;
        }

        RenderSettings.skybox = skybox;
        
        RenderSettings.ambientIntensity = 0.4f;
        TryDestroyEnvironmentArt();
        var fxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/VFX/Environment/EnvFX_Chapter2.prefab");
        var fx = Object.Instantiate(fxPrefab);
        AdjustMotes(fx);
        var godRays = fx.transform.Find("GodRays");
        var godParticles = godRays.GetComponent<ParticleSystem>();
        AdjustGodRays(godParticles, 1.0f);
    }

    [MenuItem("Tools/Crowded Kitchen/Light Level/Chapter 3 Lighting")]
    public static void ApplyChapter3Lighting()
    {
        Material skybox = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Materials/Lighting/MAT_Skybox_3.mat");
        if (skybox == null)
        {
            Debug.LogError("Couldn't find skybox!");
            return;
        }

        RenderSettings.skybox = skybox;
        RenderSettings.ambientIntensity = 0.5f;
        TryDestroyEnvironmentArt();
        var fxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/VFX/Environment/EnvFX_Chapter3.prefab");
        var fx = Object.Instantiate(fxPrefab);
        var godRays = fx.transform.Find("GodRays");
        var godParticles = godRays.GetComponent<ParticleSystem>();
        AdjustGodRays(godParticles, 1f);
    }

    [MenuItem("Tools/Crowded Kitchen/Light Level/Chapter 4 Lighting")]
    public static void ApplyChapter4Lighting()
    {
        Material skybox = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Materials/Lighting/MAT_Skybox_4.mat");
        if (skybox == null)
        {
            Debug.LogError("Couldn't find skybox!");
            return;
        }
        RenderSettings.skybox = skybox;
        RenderSettings.ambientIntensity = 1.3f;
        TryDestroyEnvironmentArt();
        var fxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/VFX/Environment/EnvFX_Chapter4.prefab");
        var fx = Object.Instantiate(fxPrefab);
        
        //TODO: place clouds around bottom faces of level
        //TODO: cloud color intensity, make darker or lighter based on float value
    }

    [MenuItem("Tools/Crowded Kitchen/Light Level/Chapter 5 Lighting")]
    public static void ApplyChapter5Lighting()
    {
        Material skybox = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Materials/Lighting/MAT_Skybox_5.mat");
        if (skybox == null)
        {
            Debug.LogError("Couldn't find skybox!");
            return;
        }
        RenderSettings.skybox = skybox;
        RenderSettings.ambientIntensity = 1.3f;
        TryDestroyEnvironmentArt();
        var fxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/VFX/Environment/EnvFX_Chapter5.prefab");
        var fx = Object.Instantiate(fxPrefab);
        var rainParticles = fx.transform.Find("CFXR4 Rain Falling").GetComponent<ParticleSystem>();
        var (startMin, startMax) = (0.01f, 0.02f);
        var main = rainParticles.main;
        var startSize = main.startSize;
        startSize.constantMin = startMin;
        startSize.constantMax = startMax;
        main.startSize = startSize;
        main.simulationSpeed = 0.55f;
        var emission = rainParticles.emission;
        var rateOverTime = emission.rateOverTime;
        rateOverTime.constant = 500;
        emission.rateOverTime = rateOverTime;
        var startColor = main.startColor;
        var startColorColor = startColor.color;
        startColorColor.a = 0.682353f;
        startColor.color = startColorColor;
        main.startColor = startColor;
        //TODO: rain drops at front edge of floor tiles
    }

    private static void TryDestroyEnvironmentArt()
    {
        var ch1 = GameObject.Find("EnvFX_Chapter1(Clone)");
        var ch2 = GameObject.Find("EnvFX_Chapter2(Clone)");
        var ch3 = GameObject.Find("EnvFX_Chapter3(Clone)");
        var ch4 = GameObject.Find("EnvFX_Chapter4(Clone)");
        var ch5 = GameObject.Find("EnvFX_Chapter5(Clone)");
        if (ch1) Object.DestroyImmediate(ch1);
        if (ch2) Object.DestroyImmediate(ch2);
        if (ch3) Object.DestroyImmediate(ch3);
        if (ch4) Object.DestroyImmediate(ch4);
        if (ch5) Object.DestroyImmediate(ch5);
    }

    private static void AdjustMotes(GameObject fx)
    {
        var grid = GridBase.Instance;
        if (grid != null)
        {
            Dictionary<Vector3Int, Collider> colliderMap = new();
            var gridEntries = GameObject.FindObjectsOfType<GridPlacer>();
            foreach (var gridPlacer in gridEntries)
            {
                var cell = grid.WorldToCell(gridPlacer.transform.position);
                if (gridPlacer.gameObject.name.Contains("Disable") && !colliderMap.ContainsKey(cell))
                {
                    colliderMap.Add(
                        cell, gridPlacer.GetComponent<Collider>());
                }
            }

            var bounds = new Bounds();
            for (int i = 0; i < grid.Size; i++)
            {
                for (int j = 0; j < grid.Size; j++)
                {
                    var gridIdx = new Vector3Int(i, 0, j);
                    if (!colliderMap.ContainsKey(gridIdx))
                    {
                        //this is a grid cell
                        bounds.Encapsulate(grid.CellToWorld(gridIdx));
                    }
                }
            }

            var motes = fx.transform.Find("Motes");
            motes.transform.position = bounds.center;
            var moteParticles = motes.GetComponent<ParticleSystem>();
            Vector3 moteSize = new Vector3(bounds.size.x + 0.5f, bounds.size.z + 0.5f, 4f);
            var shape = moteParticles.shape;
            shape.scale = moteSize;
        }
    }
    
    /// <summary>
    /// Sets the scene links for scenes that are a opener/closer.
    /// Typically these will have a cutscene framework object in the scene
    /// or something similar. 
    /// </summary>
    /// <param name="entrance">The scene to modify.</param>
    /// <param name="scene">The destination scene to link towards.</param>
    private static void SetOpenerCloserSceneExit(LevelOrder.LevelData entrance, SceneAsset scene)
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

        EditorSceneManager.SaveScene(introScene);
    }

    /// <summary>
    /// Goes through a chapter's puzzles and updates each of their exit locations.
    /// </summary>
    /// <param name="chapterIndex">The index of the chapter that is being modified.</param>
    private static void UpdatePuzzleExits(int chapterIndex)
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
        levelData.PrettyChapterNames.Clear();
        levelData.PrettySceneNames.Clear();
        //add the main menu scene
        editorBuildSettingsScenes.Add(
            new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(levelData.MainMenuScene),
                true));
        levelData.PrettySceneNames.Add(new LevelOrder.PrettyData {PrettyName = "Main Menu", showUp = false});
        //add each chapter's data
        int chapterIndex = 0;
        foreach (var chapter in levelData.Chapters)
        {
            levelData.PrettyChapterNames.Add(chapter.ChapterName);
            //add intro scene
            if (chapter.Intro.Scene != null)
            {
                editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(
                    AssetDatabase.GetAssetPath(chapter.Intro.Scene),
                    true));
                levelData.PrettySceneNames.Add(new LevelOrder.PrettyData
                    {PrettyName = chapter.Intro.LevelName, showUp = false});
            }

            //add all puzzles
            foreach (var level in chapter.Puzzles)
            {
                if (level.Scene == null) continue;
                editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(
                    AssetDatabase.GetAssetPath(level.Scene),
                    true));
                levelData.PrettySceneNames.Add(new LevelOrder.PrettyData {PrettyName = level.LevelName, showUp = true});
            }

            //add outro scene
            if (chapter.Outro.Scene != null)
            {
                editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(
                    AssetDatabase.GetAssetPath(chapter.Outro.Scene),
                    true));
                levelData.PrettySceneNames.Add(new LevelOrder.PrettyData
                    {PrettyName = chapter.Outro.LevelName, showUp = true});
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
            levelData.PrettySceneNames.Add(new LevelOrder.PrettyData {PrettyName = "Credits Scene", showUp = false});
        }

        EditorUtility.SetDirty(levelData);
        Debug.Log($"Added {editorBuildSettingsScenes.Count} Scenes");
        EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
    }
}