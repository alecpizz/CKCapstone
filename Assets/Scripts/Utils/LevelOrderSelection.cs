/******************************************************************
 *    Author: Alec Pizziferro
 *    Contributors:  nullptr
 *    Date Created: 2/17/2025
 *    Description: A scriptable object singleton to hold the
 *    selected level order in build.
 *******************************************************************/
using UnityEngine;

[CreateAssetMenu(fileName = "LevelOrderSelection", menuName = "ScriptableObjects/Level Order Singleton", order = 0)]
public class LevelOrderSelection : ScriptableObjectSingleton<LevelOrderSelection>
{
    [field: SerializeField] public LevelOrder SelectedLevelData { get; private set; }
}