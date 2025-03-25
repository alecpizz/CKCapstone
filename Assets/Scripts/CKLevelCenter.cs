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