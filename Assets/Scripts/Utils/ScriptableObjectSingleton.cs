/******************************************************************
 *    Author: Alec Pizziferro
 *    Contributors:  nullptr
 *    Date Created: 2/17/2025
 *    Description: Resource based scriptable object singleton base
 *    class. Requires the singleton to exist within the Resources folder.
 *******************************************************************/
using UnityEngine;

public class ScriptableObjectSingleton<T> : ScriptableObject where T : ScriptableObject
{
    private static T _instance;

    /// <summary>
    /// Grabs the singleton instance, loading it if necessary.
    /// </summary>
    public static T Instance
    {
        get
        {
            //no instance, resource load it, assuming that the singleton has the same name.
            if (_instance == null)
            {
                _instance = Resources.Load<T>(typeof(T).ToString());
            }

            if (_instance == null)
            {
                Debug.LogError($"Scriptable object singleton for type {typeof(T).ToString()} has not been created!");
            }

            return _instance;
        }
    }

}