using System.Collections;
using UnityEngine;

namespace MiniGameFramework.Core.Utils
{
    /// <summary>
    /// Utility class for running coroutines from non-MonoBehaviour classes
    /// </summary>
    public class CoroutineRunner : MonoSingleton<CoroutineRunner>
    {
        public static Coroutine Run(IEnumerator coroutine)
        {
            return Instance.StartCoroutine(coroutine);
        }

        public static void Stop(Coroutine coroutine)
        {
            if (coroutine != null)
            {
                Instance.StopCoroutine(coroutine);
            }
        }
    }
}
