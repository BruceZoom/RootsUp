using UnityEngine;

public abstract class PassiveSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
                Debug.LogWarning(typeof(T).ToString() + " is NULL.");
            return _instance;
        }
    }

    public virtual void Initialize()
    {
        // initialize instance
        if (_instance != null)
        {
            if (_instance != this)
            {
                Destroy(this);
            }
            return;
        }
        else
        {
            _instance = this as T;
        }

        // more initialization in child class
    }
}
