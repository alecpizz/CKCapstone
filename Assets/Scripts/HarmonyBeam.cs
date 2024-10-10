using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarmonyBeam : MonoBehaviour
{
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private float _laserDistance = 50f;
    [SerializeField] private LineRenderer _lineRenderer;

    void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    void FixedUpdate()
    {
        ShootLaser();
    }

    void ShootLaser()
    {
        _lineRenderer.SetPosition(0, transform.position);

        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, _laserDistance, _layerMask);
        System.Array.Sort(hits, (hit1, hit2) => hit1.distance.CompareTo(hit2.distance));
        Vector3 laserEndPoint = transform.position + transform.forward * _laserDistance;

        foreach (RaycastHit hit in hits)
        {
            switch (hit.collider.tag)
            {
                case "Enemy":
                    Debug.Log("Hit an enemy: " + hit.collider.name);
                    break;
                case "Reflective":
                    hit.collider.GetComponent<ReflectiveObject>().Reflect();
                    break;
            }

            laserEndPoint = hit.point;
        }

        _lineRenderer.SetPosition(1, laserEndPoint);
    }
}
