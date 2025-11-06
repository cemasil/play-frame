using UnityEngine;
using MiniGameFramework.Core;

namespace MiniGameFramework.MiniGames.Common
{
    /// <summary>
    /// Base class for all mini-game UI components
    /// </summary>
    public class BaseGameUI : MonoBehaviour, IInitializable, IUpdatable
    {
        protected virtual void Awake()
        {
            Initialize();
        }

        protected virtual void Start()
        {
            OnGameStart();
        }

        protected virtual void Update()
        {
            OnUpdate();
        }

        protected virtual void OnDestroy()
        {
            Cleanup();
        }

        public virtual void Initialize() { }
        protected virtual void OnGameStart() { }
        public virtual void OnUpdate() { }
        public virtual void UpdateScore(int score) { }
        public virtual void ShowGameOver(int finalScore) { }
        protected virtual void Cleanup() { }


    }
}
