/******************************************************************
 *    Author: Alec Pizziferro
 *    Contributors:  nullptr
 *    Date Created: 3/30/2025
 *    Description: Scriptable object singleton for referencing offsets
 *    within the game.
 *******************************************************************/
using SaintsField;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "ScriptableObjects/Offsets Singleton", fileName = "CKOffsetsReference")]
public class CKOffsetsReference : ScriptableObjectSingleton<CKOffsetsReference>
{
    [SepTitle("Entities", EColor.Magenta)] [SerializeField] [OnValueChanged(nameof(MotherOffsetChanged))]
    private Vector3 _motherOffset = new(0f, 0.15f, 0f);

    [SerializeField] [OnValueChanged(nameof(EnemyOffsetChanged))]
    private Vector3 _sonOffset = new Vector3(0.0f, 0.0f, 0.0f);

    [SerializeField] [OnValueChanged(nameof(EnemyOffsetChanged))]
    private Vector3 _enemyOffset = new Vector3(0.0f, 0.0f);
    
    [SerializeField] [OnValueChanged((nameof(EnemyOffsetChanged)))]
    private Vector3 _mirrorEnemyOffset = new Vector3(0.0f, 0.0f, 0.0f);
    [SerializeField] [OnValueChanged((nameof(EnemyOffsetChanged)))]
    private Vector3 _copyEnemyOffset = new Vector3(0.0f, 0.0f, 0.0f);

    [SepTitle("Level Objects", EColor.Green)] [SerializeField] [OnValueChanged(nameof(SwitchOffsetChanged))]
    private Vector3 _switchOffset = new Vector3(0.0f, 0.0f);

    [SerializeField] [OnValueChanged(nameof(MovingWallOffsetChanged))]
    private Vector3 _movingWallOffset = new Vector3(0.0f, 0.0f);

    [SerializeField] [OnValueChanged(nameof(HarmonyBeamOffsetChanged))]
    private Vector3 _harmonyBeamOffset = new Vector3(0.0f, 0.0f);

    [SerializeField] [OnValueChanged(nameof(HarmonyBeamOffsetChanged))]
    private Vector3 _reflectorOffset = new Vector3(0.0f, 0.0f);

    [SerializeField] [OnValueChanged(nameof(MetronomeOffsetChanged))]
    private Vector3 _metronomeOffset = new Vector3(0.0f, 0.0f);
    [SerializeField] [OnValueChanged(nameof(MetronomeOffsetChanged))]
    private Vector3 _metronomePredictorOffset = new Vector3(0.0f, 0.0f);
    
    [SepTitle("Doors", EColor.DarkBlue)]
    [SerializeField] [OnValueChanged(nameof(DoorOffsetChanged))]
    private Vector3 _doorOffsetLeft = new Vector3(0.0f, 0.0f);
    [SerializeField] [OnValueChanged(nameof(DoorOffsetChanged))]
    private Vector3 _doorOffsetRight = new Vector3(0.0f, 0.0f);
    [SerializeField] [OnValueChanged(nameof(DoorOffsetChanged))]
    private Vector3 _doorOffsetDown = new Vector3(0.0f, 0.0f);
 [SerializeField] [OnValueChanged(nameof(DoorOffsetChanged))]
    private Vector3 _doorOffsetUp = new Vector3(0.0f, 0.0f);

    [SerializeField] [OnValueChanged(nameof(NoteOffsetChanged))]
    private Vector3 _noteOffset = new Vector3(0f, 0f, 0f);

    public static Vector3 MotherOffset => Instance._motherOffset;
    public static Vector3 NoteOffset => Instance._noteOffset;

    public static Vector3 EnemyOffset(bool isSon) => isSon ? Instance._sonOffset : Instance._enemyOffset;

    public static Vector3 DoorOffsetLeft => Instance._doorOffsetLeft;
    public static Vector3 DoorOffsetRight => Instance._doorOffsetRight;
    public static Vector3 DoorOffsetDown => Instance._doorOffsetDown;
    public static Vector3 DoorOffsetUp => Instance._doorOffsetUp;

    public static Vector3 SwitchOffset => Instance._switchOffset;
    public static Vector3 MovingWallOffset => Instance._movingWallOffset;

    public static Vector3 HarmonyBeamOffset(bool isReflector) =>
        isReflector ? Instance._reflectorOffset : Instance._harmonyBeamOffset;

    public static Vector3 MetronomeOffset => Instance._metronomeOffset;
    public static Vector3 MetronomePredictorOffset => Instance._metronomePredictorOffset;
    public static Vector3 MirrorCopyEnemyOffset(bool isCopy) => isCopy ? Instance._copyEnemyOffset : Instance._mirrorEnemyOffset;

    /// <summary>
    /// Calls the center door method when changed.
    /// </summary>
    private void DoorOffsetChanged()
    {
#if UNITY_EDITOR
        CKLevelCenter.CenterDoors();
#endif
    }

    /// <summary>
    /// Calls the center metronome method when changed.
    /// </summary>
    private void MetronomeOffsetChanged()
    {
#if UNITY_EDITOR
        CKLevelCenter.CenterMetronomes();
#endif
    }
    
    /// <summary>
    /// Calls the center harmony beams method when changed.
    /// </summary>
    private void HarmonyBeamOffsetChanged()
    {
#if UNITY_EDITOR
        CKLevelCenter.CenterHarmonyBeams();
#endif
    }

    /// <summary>
    /// Calls the center moving walls method when changed.
    /// </summary>
    private void MovingWallOffsetChanged()
    {
#if UNITY_EDITOR
        CKLevelCenter.CenterMovingWalls();
#endif
    }

    /// <summary>
    /// Calls the center switches method when changed.
    /// </summary>
    private void SwitchOffsetChanged()
    {
#if UNITY_EDITOR
        CKLevelCenter.CenterSwitches();
#endif
    }

    /// <summary>
    /// Calls the center enemy offset method when changed.
    /// </summary>
    private void EnemyOffsetChanged()
    {
#if UNITY_EDITOR
        CKLevelCenter.CenterEnemies();
#endif
    }

    /// <summary>
    /// Calls the center mother offset method when changed.
    /// </summary>
    private void MotherOffsetChanged()
    {
#if UNITY_EDITOR
        CKLevelCenter.CenterMother();
#endif
    }

    /// <summary>
    /// Calls the center note offset method when changed.
    /// </summary>
    private void NoteOffsetChanged()
    {
        #if UNITY_EDITOR
        CKLevelCenter.CenterNotes();
        #endif
    }
}