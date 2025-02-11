using UnityEngine;

[CreateAssetMenu(fileName = "LevelOrderSelection", menuName = "ScriptableObjects/Level Order Singleton", order = 0)]
public class LevelOrderSelection : ScriptableObjectSingleton<LevelOrderSelection>
{
    [field: SerializeField] public LevelOrder SelectedLevelData { get; private set; }
}