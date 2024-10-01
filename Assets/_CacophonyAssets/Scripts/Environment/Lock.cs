using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: Trinity
/// Description: Controls the lock that prevents the player from accessing a room until they complete the required rooms
/// </summary>
namespace Cacophony
{
    public class Lock : MonoBehaviour
    {
        [SerializeField] List<LockProgressIndicator> progressIndicators;

        [Space] [SerializeField] GameObject lockVisuals;
        [SerializeField] bool indicatorsRemainOnUnlock = true;

        [Space] [Header("Materials")] [SerializeField]
        Material baseMaterial;

        [SerializeField] Material completedMaterial;

        Collider lockCollider;

        private void Awake()
        {
            lockCollider = GetComponent<Collider>();

            foreach (LockProgressIndicator p in progressIndicators)
            {
                p.progressRenderer.material = baseMaterial;
            }
        }

        private void Start()
        {
            if (CheckIndicatorProgress())
                Unlock();
        }

        private bool CheckIndicatorProgress()
        {
            bool isFullyComplete = true;
            foreach (LockProgressIndicator p in progressIndicators)
            {
                if (SaveSceneData.Instance.GetSceneCompletion(p.requiredSceneID))
                    SetIndicatorComplete(p);
                else if (isFullyComplete)
                    isFullyComplete = false;
            }

            return isFullyComplete;
        }

        private void SetIndicatorComplete(LockProgressIndicator indicator)
        {
            indicator.isCompleted = true;
            indicator.progressRenderer.material = completedMaterial;
        }

        private void Unlock()
        {
            if (indicatorsRemainOnUnlock)
                lockVisuals.SetActive(false);
            else
                gameObject.SetActive(false);

            lockCollider.enabled = false;

            GridStats tile = GameplayManagers.Instance.Grid.DetermineGridSpaceFromPosition(transform.position);
            if (tile != null)
                tile.SetGridAvailability(GridAvailability.Empty);
        }
    }

    [System.Serializable]
    public class LockProgressIndicator
    {
        public int requiredSceneID;
        public Renderer progressRenderer;
        public bool isCompleted = false;
    }
}