using UnityEngine;

namespace MiniGameFramework.Core
{
    /// <summary>
    /// Persistent Singleton that survives scene transitions (DontDestroyOnLoad)
    /// This is a convenience class that extends MonoSingleton with Persistent = true
    /// </summary>
    public class PersistentSingleton<T> : MonoSingleton<T> where T : MonoBehaviour
    {
        protected override bool Persistent => true;
    }
}
