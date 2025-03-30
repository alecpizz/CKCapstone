/******************************************************************
 *    Author: Alec Pizziferro
 *    Contributors:  nullptr
 *    Date Created: 3/30/2025
 *    Description: Custom editor to draw a tool bar for centering levels.
 *******************************************************************/
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor.EditorTools;
using UnityEditor.Toolbars;
using UnityEditor.Overlays;
using UnityEngine.UIElements;
using UnityEditor;

[Overlay(typeof(SceneView), "Level Centering")]
public class CKLevelCenterToolbar : Overlay, ICreateVerticalToolbar
{
    /// <summary>
    /// Implementation for creating the panel content.
    /// Just makes a label.
    /// </summary>
    /// <returns>The created visual element.</returns>
    public override VisualElement CreatePanelContent()
    {
        return new Label("Centering Tool");
    }

    /// <summary>
    /// Implementation of the vertical toolbar.
    /// Creates buttons for all fields.
    /// </summary>
    /// <returns>The created toolbar.</returns>
    public OverlayToolbar CreateVerticalToolbarContent()
    {
        var toolbar = new OverlayToolbar();
        toolbar.style.alignItems = Align.FlexStart;
        toolbar.Add(new Label("Centering Tool"));
        //add all the buttons!!!
        toolbar.Add(new Button(CKLevelCenter.CenterAll)
        {
            text = "Center ALL",
        });
        toolbar.Add(new Button(CKLevelCenter.CenterEnemies)
        {
            text = "Enemies",
        });
        toolbar.Add(new Button(CKLevelCenter.CenterSelected)
        {
            text = "Selected Object",
        });
        toolbar.Add(new Button(CKLevelCenter.CenterMother)
        {
            text = "Mother",
        });
        toolbar.Add(new Button(CKLevelCenter.CenterSwitches)
        {
            text = "Switches",
        });
        toolbar.Add(new Button(CKLevelCenter.CenterMovingWalls)
        {
            text = "Moving Walls",
        });
        toolbar.Add(new Button(CKLevelCenter.CenterHarmonyBeams)
        {
            text = "Harmony Beams/Reflectors",
        });
        toolbar.Add(new Button(CKLevelCenter.CenterMetronomes)
        {
            text = "Metronomes",
        });
        toolbar.Add(new Button(CKLevelCenter.CenterDoors)
        {
            text = "Doors",
        });
        toolbar.Add(new Button(CKLevelCenter.CenterNotes)
        {
            text = "Notes",
        });
        toolbar.Add(new Button(CKLevelCenter.RotateSelectedDoor)
        {
            text = "Rotate Selected Door"
        });
        toolbar.Add(new Button(CKLevelCenter.RotateMother)
        {
            text = "Rotate Mother"
        });
        toolbar.Add(new Button(CKLevelCenter.RotateSelected90)
        {
            text = "Rotate Selected"
        });
        return toolbar;
    }
}