using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T s_Instance;

    public static T Instance => s_Instance;

    protected virtual void Awake()
    {
        if (s_Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            s_Instance = this as T;
        }
    }

    protected virtual void OnDestroy()
    {
        if (s_Instance == this)
        {
            s_Instance = null;
        }
    }
}