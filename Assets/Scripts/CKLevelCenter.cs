#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class CKLevelCenter
{
    [MenuItem("Tools/Crowded Kitchen/Center/Center Selected")]
    public static void CenterMother()
    {
        var player = Object.FindObjectOfType<PlayerMovement>();
        if (player == null) return;
        CenterGridEntry(player);
    }
    
    [MenuItem("Tools/Crowded Kitchen/Center/Center Selected")]
    public static void CenterSelected()
    {
        var selected = Selection.activeGameObject;
        if (selected == null) return;
        var gridEntry = selected.GetComponentInParent<IGridEntry>();
        CenterGridEntry(gridEntry);
    }

    [MenuItem("Tools/Crowded Kitchen/Center/Center Enemies")]
    public static void CenterEnemies()
    {
        var enemies = Object.FindObjectsOfType<EnemyBehavior>();
        foreach (var enemyBehavior in enemies)
        {
            CenterGridEntry(enemyBehavior);
        }
    }

    [MenuItem("Tools/Crowded Kitchen/Center/Center Switches")]
    public static void CenterSwitches()
    {
        var switches = Object.FindObjectsOfType<SwitchTrigger>();
        foreach (var switchTrigger in switches)
        {
            CenterGridEntry(switchTrigger);
        }
    }

    [MenuItem("Tools/Crowded Kitchen/Center/Center Moving Walls")]
    public static void CenterMovingWalls()
    {
        var movingWalls = Object.FindObjectsOfType<MovingWall>();
        foreach (var wall in movingWalls)
        {
            CenterGridEntry(wall);
            var placer = wall.GetComponent<GridPlacer>();
            var placeField = placer.GetType().GetField("_snapToGrid");
            if (placeField != null)
            {
                placeField.SetValue(placer, false);
            }
            var field = wall.GetType().GetField("_wallGhost", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null || field.GetValue(wall) == null) continue;
            GameObject ghost = field.GetValue(wall) as GameObject;
            if (ghost == null) continue;
            var entry = ghost.GetComponent<IGridEntry>();
            CenterGridEntry(entry);
        }
    }

    [MenuItem("Tools/Crowded Kitchen/Pivot/Rotate Mother 90")]
    public static void RotateMother()
    {
        var player = Object.FindObjectOfType<PlayerMovement>();
        if (player == null) return;
        RotateGridEntry(player);
    }

   

    [MenuItem("Tools/Crowded Kitchen/Pivot/Rotate Selected 90")]
    public static void RotateSelected90()
    {
        var selected = Selection.activeGameObject;
        if (selected == null) return;
        var gridEntry = selected.GetComponentInParent<IGridEntry>();
        RotateGridEntry(gridEntry);
    }

    private static void CenterGridEntry(IGridEntry entry)
    {
        if (entry == null) return;
        var gameObj = entry.EntryObject;
        Undo.RecordObject(gameObj.transform, "Center Entry");
        entry.SnapToGridSpace();
    }

    private static void RotateGridEntry(IGridEntry entry)
    {
        if (entry == null) return;
        var gameObj = entry.EntryObject;
        Undo.RecordObject(gameObj.transform, "Rotate Entry 90");
        gameObj.transform.Rotate(Vector3.up, 90.0f);
        entry.SnapToGridSpace();
    }
}

#endif