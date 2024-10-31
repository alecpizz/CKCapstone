using UnityEngine;

public interface IHarmonyBeamEntity
{
    bool AllowLaserPassThrough { get; }
    void OnLaserHit(RaycastHit hit);
    void OnLaserExit();
    bool HitWrapAround { get; }
    Vector3 Position { get; }
}