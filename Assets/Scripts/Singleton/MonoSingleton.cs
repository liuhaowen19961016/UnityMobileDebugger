using UnityEngine;

/// <summary>
/// 继承Mono的单例模版
/// </summary>
public abstract class MonoSingleton<T> : MonoBehaviour
    where T : MonoSingleton<T>
{
    private static T _instance = null;

    public virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            GameObject.DontDestroyOnLoad(_instance.gameObject);
        }
    }

    public static T Ins
    {
        get
        {
            if (_instance != null) return _instance;

            var go = new GameObject(typeof(T).ToString());
            GameObject.DontDestroyOnLoad(go);
            _instance = go.AddComponent<T>();
            return _instance;
        }
    }

    /// <summary>
    /// 销毁单例
    /// </summary>
    public static void Dispose()
    {
        var go = GameObject.Find(typeof(T).ToString());
        if (go != null)
        {
            GameObject.Destroy(go);
        }
        _instance = null;
    }
}