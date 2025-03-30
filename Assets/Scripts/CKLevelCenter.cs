/******************************************************************
 *    Author: Alec Pizziferro
 *    Contributors:  nullptr
 *    Date Created: 3/30/2025
 *    Description: Various level object centering methods to.
 *******************************************************************/
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class CKLevelCenter
{
    /// <summary>
    /// Centerst the mother in the scene.
    /// </summary>
    [MenuItem("Tools/Crowded Kitchen/Center/Center Mother")]
    public static void CenterMother()
    {
        var player = Object.FindObjectOfType<PlayerMovement>();
        if (player == null) return;
        CenterGridEntry(player);
    }

    /// <summary>
    /// Centered the selected gameobject if it has
    /// a grid entry component. Will do nothing otherwise.
    /// </summary>
    [MenuItem("Tools/Crowded Kitchen/Center/Center Selected")]
    public static void CenterSelected()
    {
        var selected = Selection.activeGameObject;
        if (selected == null) return;
        var gridEntry = selected.GetComponentInParent<IGridEntry>();
        CenterGridEntry(gridEntry);
    }

    /// <summary>
    /// Center enemies in the scene.
    /// </summary>
    [MenuItem("Tools/Crowded Kitchen/Center/Center Enemies")]
    public static void CenterEnemies()
    {
        var enemies = Object.FindObjectsOfType<EnemyBehavior>();
        foreach (var enemyBehavior in enemies)
        {
            CenterGridEntry(enemyBehavior);
        }

        var mirrorEnemies = Object.FindObjectsOfType<MirrorAndCopyBehavior>();
        foreach (var enemyBehavior in mirrorEnemies)
        {
            CenterGridEntry(enemyBehavior);
        }
    }

    /// <summary>
    /// Center switches in the scene.
    /// </summary>
    [MenuItem("Tools/Crowded Kitchen/Center/Center Switches")]
    public static void CenterSwitches()
    {
        var switches = Object.FindObjectsOfType<SwitchTrigger>();
        foreach (var switchTrigger in switches)
        {
            CenterGridEntry(switchTrigger);
        }
    }

    /// <summary>
    /// Center moving walls in the scene.
    /// </summary>
    [MenuItem("Tools/Crowded Kitchen/Center/Center Moving Walls")]
    public static void CenterMovingWalls()
    {
        var movingWalls = Object.FindObjectsOfType<MovingWall>();
        foreach (var wall in movingWalls)
        {
            //center the wall
            CenterGridEntry(wall);
            var placer = wall.GetComponent<GridPlacer>();
            //disable snapping to grid because it messes up the in-game.
            var placeField = placer.GetType().GetField("_snapToGrid");
            if (placeField != null)
            {
                placeField.SetValue(placer, false);
            }

            //center the wall ghost. use reflection since its not exposed.
            var field = wall.GetType().GetField("_wallGhost", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null || field.GetValue(wall) == null) continue;
            GameObject ghost = field.GetValue(wall) as GameObject;
            if (ghost == null) continue;
            var entry = ghost.GetComponent<IGridEntry>();
            CenterGridEntry(entry);
        }
    }

    /// <summary>
    /// Center harmony beams and reflectors in the scene.
    /// </summary>
    [MenuItem("Tools/Crowded Kitchen/Center/Center Harmony Beams & Reflectors")]
    public static void CenterHarmonyBeams()
    {
        var harmonyBeams = Object.FindObjectsOfType<HarmonyBeam>();
        foreach (var harmonyBeam in harmonyBeams)
        {
            //harmony beams do not have grid entries, so we'll use a dedicated method
            Vector3Int cellPos = GridBase.Instance.WorldToCell(harmonyBeam.transform.position);
            harmonyBeam.transform.position = GridBase.Instance.CellToWorld(cellPos) +
                                             CKOffsetsReference.HarmonyBeamOffset(
                                                 harmonyBeam.GetComponent<ReflectionSwitch>() != null);
        }
    }

    /// <summary>
    /// Center the metronomes in the scene.
    /// </summary>
    [MenuItem("Tools/Crowded Kitchen/Center/Center Metronomes")]
    public static void CenterMetronomes()
    {
        var metronomes = Object.FindObjectsOfType<MetronomeBehavior>();
        foreach (var metronomeBehavior in metronomes)
        {
            //Have to grab the animator at the root because whoever setup the metronome
            //didn't add the component to the root for some reason. can't use root because if a designer grouped
            //everything that would break too. TODO: not do this.
            var targetTransform = metronomeBehavior.GetComponentInParent<Animator>().transform;
            Vector3Int cellPos = GridBase.Instance.WorldToCell(targetTransform.position);
            targetTransform.position = GridBase.Instance.CellToWorld(cellPos) +
                                       CKOffsetsReference.MetronomeOffset;
            var predictor = targetTransform.Find("MetronomePredictor");
            if (predictor == null) continue;
            cellPos = GridBase.Instance.WorldToCell(predictor.position);
            predictor.position = GridBase.Instance.CellToWorld(cellPos) + CKOffsetsReference.MetronomePredictorOffset;
        }
    }

    /// <summary>
    /// Center the doors in the scene
    /// </summary>
    [MenuItem("Tools/Crowded Kitchen/Center/Center Doors")]
    public static void CenterDoors()
    {
        var doors = Object.FindObjectsOfType<EndLevelDoor>();
        foreach (var door in doors)
        {
            //check the dot product of the door's right axis (it's forward on the model for some reason).
            var right = door.transform.right;
            var offset = Vector3.zero;
            if (Vector3.Dot(right, Vector3.back) >= 0.9f)
            {
                //door facing front of scene
                offset = CKOffsetsReference.DoorOffsetDown;
            }
            else if (Vector3.Dot(right, Vector3.forward) >= 0.9f)
            {
                //door facing back of scene
                offset = CKOffsetsReference.DoorOffsetUp;
            }
            else if (Vector3.Dot(right, Vector3.right) >= 0.9f)
            {
                //door is facing right of scene
                offset = CKOffsetsReference.DoorOffsetLeft;
            }
            else if (Vector3.Dot(right, Vector3.left) >= 0.9f)
            {
                //door is facing left of scene
                offset = CKOffsetsReference.DoorOffsetRight;
            }

            //center the door with the offset.
            var cell = GridBase.Instance.WorldToCell(door.transform.position);
            door.transform.position = GridBase.Instance.CellToWorld(cell) + offset;
        }
    }

    /// <summary>
    /// Rotate the selected door by 90 degrees.
    /// </summary>
    [MenuItem("Tools/Crowded Kitchen/Pivot/Rotate Selected Door 90")]
    public static void RotateSelectedDoor()
    {
        if (Selection.activeGameObject == null) return;
        var door = Selection.activeGameObject.GetComponentInParent<EndLevelDoor>();
        if (door == null) return;
        door.transform.Rotate(Vector3.up, 90f);
        CenterDoors();
    }

    /// <summary>
    /// Center notes in the scene.
    /// </summary>
    [MenuItem("Tools/Crowded Kitchen/Center/Center Notes")]
    public static void CenterNotes()
    {
        var notes = Object.FindObjectsOfType<Collectibles>();
        foreach (var note in notes)
        {
            CenterGridEntry(note);
        }
    }

    /// <summary>
    /// Invokes all center methods.
    /// </summary>
    [MenuItem("Tools/Crowded Kitchen/Center/Center ALL")]
    public static void CenterAll()
    {
        CenterEnemies();
        CenterMother();
        CenterSwitches();
        CenterMovingWalls();
        CenterHarmonyBeams();
        CenterMetronomes();
        CenterDoors();
        CenterNotes();
    }

    /// <summary>
    /// Rotates the mother by 90 degrees
    /// </summary>
    [MenuItem("Tools/Crowded Kitchen/Pivot/Rotate Mother 90")]
    public static void RotateMother()
    {
        var player = Object.FindObjectOfType<PlayerMovement>();
        if (player == null) return;
        RotateGridEntry(player);
    }


    /// <summary>
    /// Rotates the selected entry by 90 degrees.
    /// </summary>
    [MenuItem("Tools/Crowded Kitchen/Pivot/Rotate Selected 90")]
    public static void RotateSelected90()
    {
        var selected = Selection.activeGameObject;
        if (selected == null) return;
        var gridEntry = selected.GetComponentInParent<IGridEntry>();
        RotateGridEntry(gridEntry);
    }

    /// <summary>
    /// Centers the grid entry on the tile.
    /// Records an undo as well.
    /// </summary>
    /// <param name="entry">The grid entry to center.</param>
    private static void CenterGridEntry(IGridEntry entry)
    {
        if (entry == null) return;
        var gameObj = entry.EntryObject;
        Undo.RecordObject(gameObj.transform, "Center Entry");
        entry.SnapToGridSpace();
    }

    /// <summary>
    /// Rotate the grid entry by 90 degrees on the vertical axis.
    /// </summary>
    /// <param name="entry">The entry to rotate.</param>
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