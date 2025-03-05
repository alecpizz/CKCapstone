using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class CKLightingEditor
{
    [MenuItem("Tools/Crowded Kitchen/Light Level/Chapter 1 Lighting")]
    public static void ApplyChapter1Lighting() =>
        ApplyChapter1Lighting(new LightingData { AmbientIntensity = 0.5f, GodRayAlpha = 1.0f });

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
        //need to adjust god ray position
    }

    [MenuItem("Tools/Crowded Kitchen/Light Level/Chapter 2 Lighting")]
    public static void ApplyChapter2Lighting() =>
        ApplyChapter2Lighting(new LightingData { AmbientIntensity = 0.4f, GodRayAlpha = 1.0f });

    public static void ApplyChapter2Lighting(LightingData data)
    {
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
        var fxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/VFX/Environment/EnvFX_Chapter2.prefab");
        var fx = Object.Instantiate(fxPrefab);
        AdjustMotes(fx);
        var godRays = fx.transform.Find("GodRays");
        var godParticles = godRays.GetComponent<ParticleSystem>();
        AdjustGodRays(godParticles, data.GodRayAlpha);
    }

    [MenuItem("Tools/Crowded Kitchen/Light Level/Chapter 3 Lighting")]
    public static void ApplyChapter3Lighting() =>
        ApplyChapter3Lighting(new LightingData { AmbientIntensity = 0.5f, GodRayAlpha = 1.0f });

    public static void ApplyChapter3Lighting(LightingData data)
    {
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
        var fxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/VFX/Environment/EnvFX_Chapter3.prefab");
        var fx = Object.Instantiate(fxPrefab);
        var godRays = fx.transform.Find("GodRays");
        var godParticles = godRays.GetComponent<ParticleSystem>();
        AdjustGodRays(godParticles, data.GodRayAlpha);
    }

    [MenuItem("Tools/Crowded Kitchen/Light Level/Chapter 4 Lighting")]
    public static void ApplyChapter4Lighting() => ApplyChapter4Lighting(new LightingData()
        { AmbientIntensity = 1.3f, CloudSpacing = 2, GodRayAlpha = 1.0f, HueMultiplier = 1f, MinimumGridZ = 3 });

    public static void ApplyChapter4Lighting(LightingData data)
    {
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
        var fxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/VFX/Environment/EnvFX_Chapter4.prefab");
        var fx = Object.Instantiate(fxPrefab);

        Object.DestroyImmediate(fx.transform.Find("VolumetricClouds").gameObject);
        /**************************************************************
         * CLOUD PASS BELOW. commented out while i figure out a better
         * way to distribute them.
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

    [MenuItem("Tools/Crowded Kitchen/Light Level/Chapter 5 Lighting")]
    public static void ApplyChapter5Lighting() => ApplyChapter5Lighting(new LightingData()
    {
        AmbientIntensity = 1.3f, GodRayAlpha = 1.0f, RainAlpha = 0.682353f, RainEmissionRate = 500f,
        RainSimulationSpeed = 0.55f, RainParticleMaxSize = 0.02f, RainParticleMinSize = 0.01f
    });

    public static void ApplyChapter5Lighting(LightingData data)
    {
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
        var fxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/VFX/Environment/EnvFX_Chapter5.prefab");
        var fx = Object.Instantiate(fxPrefab);
        var rainParticles = fx.transform.Find("CFXR4 Rain Falling").GetComponent<ParticleSystem>();
        var main = rainParticles.main;
        var startSize = main.startSize;
        startSize.constantMin = data.RainParticleMinSize;
        startSize.constantMax = data.RainParticleMaxSize;
        main.startSize = startSize;
        main.simulationSpeed = data.RainSimulationSpeed;
        var emission = rainParticles.emission;
        var rateOverTime = emission.rateOverTime;
        rateOverTime.constant = data.RainEmissionRate;
        emission.rateOverTime = rateOverTime;
        var startColor = main.startColor;
        var startColorColor = startColor.color;
        startColorColor.a = data.RainAlpha;
        startColor.color = startColorColor;
        main.startColor = startColor;
        var grid = GridBase.Instance;
        var rainDrop = fx.transform.Find("Raindrops/CFXR4 Rain Falling (1)");
        if (grid != null)
        {
            HashSet<Vector3Int> colliderMap = new();
            var gridEntries = Object.FindObjectsOfType<GridPlacer>();
            foreach (var gridPlacer in gridEntries)
            {
                var cell = grid.WorldToCell(gridPlacer.transform.position);
                if (gridPlacer.gameObject.name.Contains("Disable"))
                {
                    colliderMap.Add(
                        cell);
                }
            }

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

        Object.DestroyImmediate(fx.transform.Find("Raindrops").gameObject);
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

    private static void AdjustGodRays(ParticleSystem godParticles, float newGodRayAlpha)
    {
        var main = godParticles.main;
        var startColor = main.startColor;
        var startColorColor = startColor.color;
        startColorColor.a = newGodRayAlpha;
        startColor.color = startColorColor;
        main.startColor = startColor;
    }

    private static void AdjustMotes(GameObject fx)
    {
        var grid = GridBase.Instance;
        if (grid != null)
        {
            Dictionary<Vector3Int, Collider> colliderMap = new();
            var gridEntries = Object.FindObjectsOfType<GridPlacer>();
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
}