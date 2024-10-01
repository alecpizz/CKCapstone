using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cacophony
{
    public class CameraDistance : MonoBehaviour
    {
        [SerializeField] Vector3 _direction;
        [SerializeField] float _distPerGridSpace;

        // Start is called before the first frame update
        void Start()
        {
            CameraDist();
        }

        void CameraDist()
        {
            if (GameplayManagers.Instance == null) return;
            float storedX = transform.position.x;
            transform.position = _direction * GameplayManagers.Instance.Grid.ReturnGreaterOfRowsColumns() *
                                 _distPerGridSpace;

            storedX += .5f;
            if (GameplayManagers.Instance.Grid.GetColumns() % 2 != 0)
                storedX += .5f;
            transform.position = new Vector3(storedX, transform.position.y, transform.position.z);
        }


        // Update is called once per frame
        void Update()
        {
        }
    }
}