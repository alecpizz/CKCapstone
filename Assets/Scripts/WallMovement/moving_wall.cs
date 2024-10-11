using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class moving_wall : MonoBehaviour, IGridEntry
{
    private Vector3 originTransform;

    [SerializeField] private Vector3 wallTransform;
    [SerializeField] private Material wallColor;

    [SerializeField] private GridPlacer wallGrid;
    [SerializeField] private GameObject wallGhost;

    public bool IsTransparent => throw new System.NotImplementedException();


    public GameObject GetGameObject => throw new System.NotImplementedException();

    public Vector3 Position => throw new System.NotImplementedException();



    // Start is called before the first frame update
    void Start()
    {
        originTransform = transform.position;

        wallGhost.transform.position = wallTransform;
    }

    public void Wall_Is_Moved()
    {
        transform.position = wallTransform;
        wallGhost.transform.position = originTransform;
        wallGrid.UpdatePosition();
    }

    public void Wall_Move_Back()
    {
        transform.position = originTransform;
        wallGhost.transform.position = wallTransform;
        wallGrid.UpdatePosition();
    }
}
