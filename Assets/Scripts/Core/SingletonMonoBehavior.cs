using UnityEngine;

public class SingletonMonoBehavior<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    public static bool HasInstance => Instance != null;
        
    protected virtual void Awake()
    {
        if (HasInstance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this as T;
        RegisterEvents();
    }
        
    protected virtual void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        UnRegisterEvents();
    }
    
    protected virtual void RegisterEvents(){}
    
    protected virtual void UnRegisterEvents(){}
}