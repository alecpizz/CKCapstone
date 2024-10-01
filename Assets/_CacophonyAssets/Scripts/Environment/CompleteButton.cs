using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cacophony
{
    public class CompleteButton : MonoBehaviour
    {
        [SerializeField] Renderer visual;

        [SerializeField] Material baseMaterial;

        [SerializeField] Material completedMaterial;

        bool isCompleted;

        private void Start()
        {
            isCompleted = GameplayManagers.Instance.Room.GetRoomSolved();

            UpdateVisuals();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            GameplayManagers.Instance.Room.RoomVictory();

            isCompleted = true;

            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            Material newMaterial = isCompleted ? completedMaterial : baseMaterial;

            visual.material = newMaterial;
        }
    }
}