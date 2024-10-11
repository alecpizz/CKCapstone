using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moving_wall : MonoBehaviour
{
    private Vector3 originTransform;

    [SerializeField] private Vector3 wallTransform;
    [SerializeField] private Material wallColor;

    // Start is called before the first frame update
    void Start()
    {
        originTransform = transform.position;
    }

    public void Wall_Is_Moved()
    {
        transform.position = wallTransform;
        
    }

    public void Wall_Move_Back()
    {
        transform.position = originTransform;
    }
}
