using UnityEngine;

namespace MiniGameFramework.Core
{
    /// <summary>
    /// Generic MonoBehaviour Singleton pattern
    /// Automatically creates GameObject if instance doesn't exist
    /// Set 'persistent' to true to survive scene transitions (DontDestroyOnLoad)
    /// </summary>
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static bool _applicationIsQuitting = false;
        private static readonly object _lock = new object();

        /// <summary>
        /// If true, this singleton will persist across scene loads (DontDestroyOnLoad)
        /// Override this in derived classes to change behavior
        /// </summary>
        protected virtual bool Persistent => false;

        public static bool HasInstance => _instance != null;

        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning($"[MonoSingleton] Instance of {typeof(T)} already destroyed. Returning null.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindFirstObjectByType<T>();

                        if (_instance == null)
                        {
                            GameObject singletonObject = new GameObject($"[Singleton] {typeof(T)}");
                            _instance = singletonObject.AddComponent<T>();
                        }
                    }

                    return _instance;
                }
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;

                if (Persistent)
                {
                    DontDestroyOnLoad(gameObject);
                }

                OnSingletonAwake();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Called after singleton is initialized. Override instead of Awake.
        /// </summary>
        protected virtual void OnSingletonAwake() { }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                _applicationIsQuitting = true;
            }
        }

#if UNITY_EDITOR
        protected virtual void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
            _instance = null;
        }

        /// <summary>
        /// Reset static state when entering/exiting play mode in editor
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
            _applicationIsQuitting = false;
        }
#endif
    }
}
