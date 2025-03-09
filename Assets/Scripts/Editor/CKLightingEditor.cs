/******************************************************************
 *    Author: Alec Pizziferro
 *    Contributors:  nullptr
 *    Date Created: 3/5/2025
 *    Description: A quick and (very) dirty tool to replace scene lighting
 *    with game specific lighting assets. Pretty fragile.
 *******************************************************************/
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class CKLightingEditor
{
    private const string GridPrefab = "_gridPrefab";
    /// <summary>
    /// Applies ch1 lighting with pre-set settings.
    /// </summary>
    [MenuItem("Tools/Crowded Kitchen/Light Level/Chapter 1 Lighting")]
    public static void ApplyChapter1Lighting() =>
        ApplyChapter1Lighting(new LightingData { AmbientIntensity = 0.5f, GodRayAlpha = 1.0f });

    /// <summary>
    /// Apply ch1 lighting with custom settings.
    /// </summary>
    /// <param name="data"></param>
    public static void ApplyChapter1Lighting(LightingData data)
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

        RenderSettings.ambientIntensity = data.AmbientIntensity;
        TryDestroyEnvironmentArt();
        var fxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/VFX/Environment/EnvFX_Chapter1.prefab");
        var fx = Object.Instantiate(fxPrefab);
        var godRays = fx.transform.Find("GodRays");
        var godParticles = godRays.GetComponent<ParticleSystem>();
        AdjustGodRays(godParticles, data.GodRayAlpha);
        AdjustMotes(fx);
        string prefab = "Assets/Prefabs/LevelPrefabs/GridTileCH1.prefab";
        ReplaceGridPrefab(prefab);
        //need to adjust god ray position
    }

   

    /// <summary>
    /// Applies ch2 lighting with pre-set settings.
    /// </summary>
    [MenuItem("Tools/Crowded Kitchen/Light Level/Chapter 2 Lighting")]
    public static void ApplyChapter2Lighting() =>
        ApplyChapter2Lighting(new LightingData { AmbientIntensity = 0.4f, GodRayAlpha = 1.0f });

    /// <summary>
    /// Apply ch1 lighting with custom settings.
    /// </summary>
    /// <param name="data"></param>
    public static void ApplyChapter2Lighting(LightingData data)
    {
        //grab skybox from assets and apply it
        Material skybox = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Materials/Lighting/MAT_Skybox_2.mat");
        if (skybox == null)
        {
            Debug.LogError("Couldn't find skybox!");
            return;
        }

        RenderSettings.skybox = skybox;

        RenderSettings.ambientIntensity = data.AmbientIntensity;
        TryDestroyEnvironmentArt();
        //create fx
        var fxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/VFX/Environment/EnvFX_Chapter2.prefab");
        var fx = Object.Instantiate(fxPrefab);
        AdjustMotes(fx);
        var godRays = fx.transform.Find("GodRays");
        var godParticles = godRays.GetComponent<ParticleSystem>();
        AdjustGodRays(godParticles, data.GodRayAlpha);
        string prefab = "Assets/Prefabs/LevelPrefabs/GridTileCH2.prefab";
        ReplaceGridPrefab(prefab);
    }

    /// <summary>
    /// Applies ch3 lighting with pre-set settings.
    /// </summary>
    [MenuItem("Tools/Crowded Kitchen/Light Level/Chapter 3 Lighting")]
    public static void ApplyChapter3Lighting() =>
        ApplyChapter3Lighting(new LightingData { AmbientIntensity = 0.5f, GodRayAlpha = 1.0f });

    /// <summary>
    /// Apply ch1 lighting with custom settings.
    /// </summary>
    /// <param name="data"></param>
    public static void ApplyChapter3Lighting(LightingData data)
    {
        //grab skybox from assets and apply it
        Material skybox = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Materials/Lighting/MAT_Skybox_3.mat");
        if (skybox == null)
        {
            Debug.LogError("Couldn't find skybox!");
            return;
        }

        RenderSettings.skybox = skybox;
        RenderSettings.ambientIntensity = data.AmbientIntensity;
        TryDestroyEnvironmentArt();
        //setup fx
        var fxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/VFX/Environment/EnvFX_Chapter3.prefab");
        var fx = Object.Instantiate(fxPrefab);
        var godRays = fx.transform.Find("GodRays");
        var godParticles = godRays.GetComponent<ParticleSystem>();
        AdjustGodRays(godParticles, data.GodRayAlpha);
        string prefab = "Assets/Prefabs/LevelPrefabs/GridTileCH3.prefab";
        ReplaceGridPrefab(prefab);
    }

    /// <summary>
    /// Applies ch4 lighting with pre-set settings.
    /// </summary>
    [MenuItem("Tools/Crowded Kitchen/Light Level/Chapter 4 Lighting")]
    public static void ApplyChapter4Lighting() => ApplyChapter4Lighting(new LightingData()
        { AmbientIntensity = 1.3f, CloudSpacing = 2, GodRayAlpha = 1.0f, HueMultiplier = 1f, MinimumGridZ = 3 });

    /// <summary>
    /// Apply ch1 lighting with custom settings.
    /// </summary>
    /// <param name="data"></param>
    public static void ApplyChapter4Lighting(LightingData data)
    {
        //grab skybox from assets and apply it
        Material skybox = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Materials/Lighting/MAT_Skybox_4.mat");
        if (skybox == null)
        {
            Debug.LogError("Couldn't find skybox!");
            return;
        }

        RenderSettings.skybox = skybox;
        RenderSettings.ambientIntensity = data.AmbientIntensity;
        TryDestroyEnvironmentArt();
        //spawn in fx
        var fxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/VFX/Environment/EnvFX_Chapter4.prefab");
        var fx = Object.Instantiate(fxPrefab);

        Object.DestroyImmediate(fx.transform.Find("VolumetricClouds").gameObject);
        string prefab = "Assets/Prefabs/LevelPrefabs/GridTileCH4.prefab";;
        ReplaceGridPrefab(prefab);
        /**************************************************************
         * CLOUD PASS BELOW. commented out while i figure out a better
         * way to distribute them without it looking shitty.
         *************************************************************/
        // var grid = GridBase.Instance;
        // var cloudPrefab = fx.transform.Find("VolumetricClouds/VolClouds");
        // if (grid != null)
        // {
        //     HashSet<Vector3Int> colliderMap = new();
        //     var gridEntries = Object.FindObjectsOfType<GridPlacer>();
        //     foreach (var gridPlacer in gridEntries)
        //     {
        //         var cell = grid.WorldToCell(gridPlacer.transform.position);
        //         if (gridPlacer.gameObject.name.Contains("Disable"))
        //         {
        //             colliderMap.Add(
        //                 cell);
        //         }
        //     }
        //
        //     List<Vector3Int> cloudCells = new();
        //     for (int i = 0; i < grid.Size; i++)
        //     {
        //         for (int j = 0; j < grid.Size; j++)
        //         {
        //             var gridIdx = new Vector3Int(i, 0, j);
        //             if (!colliderMap.Contains(gridIdx) && j < data.MinimumGridZ)
        //             {
        //                 cloudCells.Add(gridIdx);
        //             }
        //         }
        //     }
        //
        //     List<ParticleSystem> cloudParticles = new();
        //     for (var index = 0; index < cloudCells.Count; index += data.CloudSpacing)
        //     {
        //         var cell = cloudCells[index];
        //         var worldPos = grid.CellToWorld(cell);
        //         var obj = Object.Instantiate(cloudPrefab.gameObject, worldPos - new Vector3(0.5f, 0f, 1f),
        //             Quaternion.Euler(-90f, 0f, 0f), fx.transform);
        //         cloudParticles.Add(obj.GetComponent<ParticleSystem>());
        //     }
        //
        //     foreach (var cloud in cloudParticles)
        //     {
        //         var main = cloud.main;
        //         var startColor = main.startColor;
        //         var colorMin = startColor.colorMin;
        //         Color.RGBToHSV(colorMin, out float h, out float s, out float v);
        //         h *= data.HueMultiplier;
        //         startColor.colorMin = Color.HSVToRGB(h, s, v);
        //         var colorMax = startColor.colorMax;
        //         Color.RGBToHSV(colorMax, out h, out s, out v);
        //         h *= data.HueMultiplier;
        //         startColor.colorMax = Color.HSVToRGB(h, s, v);
        //         main.startColor = startColor;
        //     }
        //
        //     Object.DestroyImmediate(fx.transform.Find("VolumetricClouds").gameObject);
        // }
    }

    /// <summary>
    /// Applies ch5 lighting with pre-set settings.
    /// </summary>
    [MenuItem("Tools/Crowded Kitchen/Light Level/Chapter 5 Lighting")]
    public static void ApplyChapter5Lighting() => ApplyChapter5Lighting(new LightingData()
    {
        AmbientIntensity = 1.3f, GodRayAlpha = 1.0f, RainAlpha = 0.682353f, RainEmissionRate = 500f,
        RainSimulationSpeed = 0.55f, RainParticleMaxSize = 0.02f, RainParticleMinSize = 0.01f
    });

    /// <summary>
    /// Apply ch1 lighting with custom settings.
    /// </summary>
    /// <param name="data"></param>
    public static void ApplyChapter5Lighting(LightingData data)
    {
        //grab skybox and apply it
        Material skybox = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Materials/Lighting/MAT_Skybox_5.mat");
        if (skybox == null)
        {
            Debug.LogError("Couldn't find skybox!");
            return;
        }

        RenderSettings.skybox = skybox;
        RenderSettings.ambientIntensity = data.AmbientIntensity;
        TryDestroyEnvironmentArt();
        //spawn fx
        var fxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/VFX/Environment/EnvFX_Chapter5.prefab");
        //adjust rain start size
        var fx = Object.Instantiate(fxPrefab);
        var rainParticles = fx.transform.Find("CFXR4 Rain Falling").GetComponent<ParticleSystem>();
        var main = rainParticles.main;
        var startSize = main.startSize;
        startSize.constantMin = data.RainParticleMinSize;
        startSize.constantMax = data.RainParticleMaxSize;
        main.startSize = startSize;
        //adjust rain speed
        main.simulationSpeed = data.RainSimulationSpeed;
        var emission = rainParticles.emission;
        var rateOverTime = emission.rateOverTime;
        rateOverTime.constant = data.RainEmissionRate;
        emission.rateOverTime = rateOverTime;
        //adjust rain opacity
        var startColor = main.startColor;
        var startColorColor = startColor.color;
        startColorColor.a = data.RainAlpha;
        startColor.color = startColorColor;
        main.startColor = startColor;
        var grid = GridBase.Instance;
        //spawn rain drops around the visible tiles
        var rainDrop = fx.transform.Find("Raindrops/CFXR4 Rain Falling (1)");
        if (grid != null)
        {
            //add all tiles that are not disabled, add them to a map
            HashSet<Vector3Int> colliderMap = new();
            var gridEntries = Object.FindObjectsOfType<GridPlacer>();
            foreach (var gridPlacer in gridEntries)
            {
                var cell = grid.WorldToCell(gridPlacer.transform.position);
                if (gridPlacer.gameObject.name.Contains("Disable"))
                {
                    colliderMap.Add(cell);
                }
            }

            //all tiles that are not disabled get some particles :)
            for (int i = 0; i < grid.Size; i++)
            {
                for (int j = 0; j < grid.Size; j++)
                {
                    var gridIdx = new Vector3Int(i, 0, j);
                    if (!colliderMap.Contains(gridIdx))
                    {
                        Object.Instantiate(rainDrop.gameObject, grid.CellToWorld(gridIdx) 
                                                                - new Vector3(0f, 0f, 1f),
                            Quaternion.Euler(90f, 0f, 0f), fx.transform);
                    }
                }
            }
        }

        //clean up prefab's particles
        Object.DestroyImmediate(fx.transform.Find("Raindrops").gameObject);
        string prefab = "Assets/Prefabs/LevelPrefabs/GridTileCH5.prefab";
        ReplaceGridPrefab(prefab);
    }

    /// <summary>
    /// Attempts to destroy any envFX objects in the scene.
    /// </summary>
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

    /// <summary>
    /// Adjusts god ray alpha.
    /// </summary>
    /// <param name="godParticles">The god ray particles in the scene.</param>
    /// <param name="newGodRayAlpha">The new alpha value</param>
    private static void AdjustGodRays(ParticleSystem godParticles, float newGodRayAlpha)
    {
        var main = godParticles.main;
        var startColor = main.startColor;
        var startColorColor = startColor.color;
        startColorColor.a = newGodRayAlpha;
        startColor.color = startColorColor;
        main.startColor = startColor;
    }

    /// <summary>
    /// Adjusts the motes particle system to surround the grid.
    /// </summary>
    /// <param name="fx">The object contianing the motes.</param>
    private static void AdjustMotes(GameObject fx)
    {
        var grid = GridBase.Instance;
        if (grid == null) return;
        
        //add all disable grid placers to a hashset 
        HashSet<Vector3Int> colliderMap = new();
        var gridEntries = Object.FindObjectsOfType<GridPlacer>();
        foreach (var gridPlacer in gridEntries)
        {
            var cell = grid.WorldToCell(gridPlacer.transform.position);
            if (gridPlacer.gameObject.name.Contains("Disable"))
            {
                colliderMap.Add(cell);
            }
        }

        //create a bounds that contains every grid cell that is not a disable grid placer
        var bounds = new Bounds();
        for (int i = 0; i < grid.Size; i++)
        {
            for (int j = 0; j < grid.Size; j++)
            {
                var gridIdx = new Vector3Int(i, 0, j);
                if (!colliderMap.Contains(gridIdx))
                {
                    //this is a grid cell
                    bounds.Encapsulate(grid.CellToWorld(gridIdx));
                }
            }
        }

        //adjust the motes position & scale to fit the new bounds
        var motes = fx.transform.Find("Motes");
        motes.transform.position = bounds.center;
        var moteParticles = motes.GetComponent<ParticleSystem>();
        //flip x & z since the motes are rotated in world space
        Vector3 moteSize = new Vector3(bounds.size.x + 0.5f, bounds.size.z + 0.5f, 4f);
        var shape = moteParticles.shape;
        shape.scale = moteSize;
    }
    
    /// <summary>
    /// Replaces the grid prefab for the scene.
    /// </summary>
    /// <param name="prefabPath">The path to the new prefab.</param>
    private static void ReplaceGridPrefab(string prefabPath)
    {
        var grid = GridBase.Instance;
        //delete any extra grids minus the prefab's one.
        for (int i = grid.transform.childCount - 1; i >= 0; i--)
        {
            if (PrefabUtility.IsPartOfAnyPrefab(grid.transform.GetChild(i).gameObject))
            {
                continue;
            }
            Object.DestroyImmediate(grid.transform.GetChild(i));
        }

        //set the field to be the new prefab.
        var cubePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        var field = grid.GetType().GetField(GridPrefab, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field != null)
        {
            field.SetValue(grid, cubePrefab);
            EditorUtility.SetDirty(grid);
        }
        else
        {
            Debug.LogError($"Could not find field {GridPrefab}");
        }
    }
}