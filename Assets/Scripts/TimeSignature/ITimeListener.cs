using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITimeListener
{
    public void UpdateTimingFromSignature(Vector2Int newTimeSignature);
}
