/******************************************************************
 *    Author: Alec Pizziferro
 *    Contributors: nullptr
 *    Date Created: 1/23/25
 *    Description: Editor utiltiy to snap an object to the grid.
 *    To use, right click on the gameObject you wish to move, and choose the option.
 *******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SnapToGridUtil : MonoBehaviour
{
    /// <summary>
    /// Snaps the item to the grid.
    /// </summary>
   [MenuItem("GameObject/Snap To Grid", false, 30)]
   private static void SnapToGrid()
   {
      var pos = Selection.activeTransform.position;
      var grid = FindObjectOfType<GridBase>();
      Selection.activeTransform.position = grid.CellToWorld(grid.WorldToCell(pos));
   }
}
