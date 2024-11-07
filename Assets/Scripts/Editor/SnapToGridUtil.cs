using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SnapToGridUtil : MonoBehaviour
{
   [MenuItem("GameObject/Snap To Grid", false, 30)]
   private static void SnapToGrid()
   {
      var pos = Selection.activeTransform.position;
      var grid = FindObjectOfType<GridBase>();
      Selection.activeTransform.position = grid.CellToWorld(grid.WorldToCell(pos));
   }
}
