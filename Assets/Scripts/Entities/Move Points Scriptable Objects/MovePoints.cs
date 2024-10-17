using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Move Point", menuName = "Move Point")]
public class MovePoints : ScriptableObject
{
    public string direction;
    public int tilesToMove;
}
