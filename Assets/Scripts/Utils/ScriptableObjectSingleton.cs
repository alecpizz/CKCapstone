using UnityEngine;

public class ScriptableObjectSingleton<T> : ScriptableObject where T : ScriptableObject
{
    private static T _instance;

    public static T Instance
    {
        get
        {
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