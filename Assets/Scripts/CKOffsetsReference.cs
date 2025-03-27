using SaintsField;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Offsets Singleton", fileName = "CKOffsetsReference")]
public class CKOffsetsReference : ScriptableObjectSingleton<CKOffsetsReference>
{
    [SepTitle("Entities", EColor.Magenta)] [SerializeField] [OnValueChanged(nameof(MotherOffsetChanged))]
    private Vector3 _motherOffset = new(0f, 0.15f, 0f);

    [SerializeField] [OnValueChanged(nameof(SonOffsetChanged))]
    private Vector3 _sonOffset = new Vector3(0.0f, 0.0f, 0.0f);

    [SerializeField] [OnValueChanged(nameof(SonOffsetChanged))]
    private Vector3 _enemyOffset = new Vector3(0.0f, 0.0f);

    [SepTitle("Level Objects", EColor.Green)] [SerializeField] [OnValueChanged(nameof(SwitchOffsetChanged))]
    private Vector3 _switchOffset = new Vector3(0.0f, 0.0f);

    [SerializeField] [OnValueChanged(nameof(MovingWallOffsetChanged))]
    private Vector3 _movingWallOffset = new Vector3(0.0f, 0.0f);

    [SerializeField] [OnValueChanged(nameof(HarmonyBeamOffsetChanged))]
    private Vector3 _harmonyBeamOffset = new Vector3(0.0f, 0.0f);

    [SerializeField] [OnValueChanged(nameof(HarmonyBeamOffsetChanged))]
    private Vector3 _reflectorOffset = new Vector3(0.0f, 0.0f);

    public static Vector3 MotherOffset => Instance._motherOffset;
    public static Vector3 EnemyOffset(bool isSon) => isSon ? Instance._sonOffset : Instance._enemyOffset;
    public static Vector3 SwitchOffset => Instance._switchOffset;
    public static Vector3 MovingWallOffset => Instance._movingWallOffset;

    public static Vector3 HarmonyBeamOffset(bool isReflector) =>
        isReflector ? Instance._reflectorOffset : Instance._harmonyBeamOffset;

    private void HarmonyBeamOffsetChanged()
    {
#if UNITY_EDITOR
        CKLevelCenter.CenterHarmonyBeams();
#endif
    }

    private void MovingWallOffsetChanged()
    {
#if UNITY_EDITOR
        CKLevelCenter.CenterMovingWalls();
#endif
    }

    private void SwitchOffsetChanged()
    {
#if UNITY_EDITOR
        CKLevelCenter.CenterSwitches();
#endif
    }

    private void SonOffsetChanged()
    {
#if UNITY_EDITOR
        CKLevelCenter.CenterEnemies();
#endif
    }

    private void MotherOffsetChanged()
    {
#if UNITY_EDITOR
        CKLevelCenter.CenterMother();
#endif
    }
}