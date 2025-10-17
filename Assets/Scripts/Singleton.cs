using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T: MonoBehaviour
{
    private static T _instance;
    private static bool _isQuitting = false;

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning($"Duplicate {typeof(T).Name} detected! Destroying this instance.");
            Destroy(gameObject);
            return;
        }
        
        _instance = this as T;
        DontDestroyOnLoad(gameObject);
    }

    public static T Instance
    {
        get
        {
            // Don't create instances when application is quitting
            if (_isQuitting)
            {
                return null;
            }
            
            if (_instance != null)
            {
                return _instance;
            }
            
            // Check if an instance already exists in the scene
            var existingInstances = FindObjectsByType<T>(FindObjectsSortMode.None);
            if (existingInstances.Length > 0)
            {
                _instance = existingInstances[0];
                
                // Destroy any duplicate instances
                for (int i = 1; i < existingInstances.Length; i++)
                {
                    Debug.LogWarning($"Destroying duplicate {typeof(T).Name} instance!");
                    Destroy(existingInstances[i].gameObject);
                }
                
                return _instance;
            }

            // Create new instance if none exists
            var singletonObject = new GameObject(typeof(T).Name);
            _instance = singletonObject.AddComponent<T>();
            DontDestroyOnLoad(singletonObject);
                
            return _instance;
        }
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
    
    protected virtual void OnApplicationQuit()
    {
        _isQuitting = true;
    }
}