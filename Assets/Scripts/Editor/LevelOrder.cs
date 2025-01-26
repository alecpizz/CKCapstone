using System.Collections.Generic;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "LevelOrder", menuName = "ScriptableObjects/Level Order", order = 0)]
public class LevelOrder : ScriptableSingleton<LevelOrder>
{
    [field: SerializeField] public List<SceneAsset> Scenes { get; private set; } = new List<SceneAsset>();


    [Button]
    private void PopulateBuildOrder()
    {
        // Find valid Scene paths and make a list of EditorBuildSettingsScene
        List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>();
        foreach (var sceneAsset in Scenes)
        {
            string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
            if (!string.IsNullOrEmpty(scenePath))
                editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(scenePath, true));
        }

        // Set the active platform or build profile scene list
        EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
    }
}