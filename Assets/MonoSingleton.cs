using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    public static bool IsQuiting = false;

    public static bool IsInited { get { return _instance != null; } }

    public static T Instance
    {
        get
        {
            // Instance requiered for the first time, we look for it
            if (_instance == null)
            {
                if (IsQuiting == true)
                {
                    Debug.LogError("Trying to get monosigleton when application is quitting");
                    return _instance;
                }

                _instance = GameObject.FindObjectOfType(typeof(T)) as T;

                // Object not found, we create a temporary one
                if (_instance == null)
                {
                    Debug.LogWarning("No instance of " + typeof(T).ToString() + ", a temporary one is created.");
                    _instance = new GameObject("Temp Instance of " + typeof(T).ToString(), typeof(T)).GetComponent<T>();

                    // Problem during the creation, this should not happen
                    if (_instance == null)
                    {
                        Debug.LogError("Problem during the creation of " + typeof(T).ToString());
                    }
                }
                _instance.HandleAwake();
            }
            return _instance;
        }
    }
    // If no other monobehaviour request the instance in an awake function
    // executing before this one, no need to search the object.
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            _instance.HandleAwake();
        }
    }

    // This function is called when the instance is used the first time
    // Put all the initializations you need here, as you would do in Awake
    protected virtual void HandleAwake() { }

    // Make sure the instance isn't referenced anymore when the user quit, just in case.
    private void OnApplicationQuit()
    {
        HandleApplicationQuit();

        _instance = null;
        IsQuiting = true;
    }

    protected virtual void HandleApplicationQuit() { }

    private static T _instance = null;
}