using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cacophony
{
    public class PlayerMoveExample : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed;

        private void Update()
        {
            Vector2 inputDir = Vector2.zero;
            if (Input.GetKey(KeyCode.W))
            {
                inputDir.y = 1;
            }

            if (Input.GetKey(KeyCode.S))
            {
                inputDir.y = -1;
            }

            if (Input.GetKey(KeyCode.D))
            {
                inputDir.x = 1;
            }

            if (Input.GetKey(KeyCode.A))
            {
                inputDir.x = -1;
            }

            inputDir.Normalize();
            Vector3 moveDir = new Vector3(inputDir.x, 0, inputDir.y);

            transform.position += moveDir * _moveSpeed * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.name == "ExampleExit")
            {
                SceneTransitions.Instance.LoadSceneWithTransition(SceneTransitions.TransitionType.WipeUp,
                    SceneTransitions.Instance.GetBuildIndex());
            }
        }
    }
}