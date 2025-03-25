using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Offsets Singleton", fileName = "CKOffsetsReference")]
public class CKOffsetsReference : ScriptableObjectSingleton<CKOffsetsReference>
{
    [field: SerializeField] private Vector3 _motherOffset = new(0f, 0.15f, 0f);
    public static Vector3 MotherOffset => Instance._motherOffset;
}