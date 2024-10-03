using UnityEngine;

public interface IGridEntry
{
    bool IsTransparent { get; }
    Vector3 Position { get; }
}
