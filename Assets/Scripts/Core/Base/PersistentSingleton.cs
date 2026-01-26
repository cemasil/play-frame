using UnityEngine;

namespace MiniGameFramework.Core
{
    /// <summary>
    /// Persistent Singleton that survives scene transitions (DontDestroyOnLoad)
    /// </summary>
    public class PersistentSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>();

                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject($"[Persistent] {typeof(T)}");
                        _instance = singletonObject.AddComponent<T>();
                        DontDestroyOnLoad(singletonObject);
                    }
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

#if UNITY_EDITOR
        protected virtual void OnApplicationQuit()
        {
            // Destroy persistent singleton GameObject when exiting Play Mode in Editor
            if (_instance == this && gameObject != null)
            {
                DestroyImmediate(gameObject);
            }
        }
#endif
    }
}
