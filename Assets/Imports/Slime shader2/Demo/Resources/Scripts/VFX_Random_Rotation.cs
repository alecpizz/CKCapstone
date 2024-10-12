using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFX_Random_Rotation : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var euler = transform.eulerAngles;
        euler.y = Random.Range(0, 360);
        transform.eulerAngles = euler;
    }
}
