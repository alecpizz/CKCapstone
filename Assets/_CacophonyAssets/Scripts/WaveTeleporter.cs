using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cacophony
{
    public class WaveTeleporter : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _teleporters;


        // Start is called before the first frame update
        void Start()
        {
        }

        public GameObject FindOtherTeleporter(GameObject inputTeleporter)
        {
            foreach (GameObject teleporter in _teleporters)
                if (teleporter != inputTeleporter)
                    return teleporter;
            return null;
        }
    }
}