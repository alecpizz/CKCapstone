using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

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
        List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>();
        
        // foreach (var data in LevelOrder.instance.Levels)
        // {
        //     var sceneAsset = data.Scene;
        //     editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(sceneAsset), true));
        // }
        
        // Set the active platform or build profile scene list
        EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
    }
}