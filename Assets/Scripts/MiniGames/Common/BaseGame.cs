using UnityEngine;
using MiniGameFramework.Core;

namespace MiniGameFramework.MiniGames.Common
{
    /// <summary>
    /// Base class for all mini-games
    /// </summary>
    public class BaseGame : MonoBehaviour, IInitializable, IUpdatable
    {
        private void Awake()
        {
            Initialize();
        }
        public void Initialize()
        {
            OnInitialize();
        }
        private void Start()
        {
            OnGameStart();
        }

        public void Update()
        {
            OnUpdate();
        }

        protected virtual void OnDestroy()
        {
            Cleanup();
        }

        protected virtual void OnInitialize() { }
        protected virtual void OnGameStart() { }
        protected virtual void OnUpdate() { }
        protected virtual void UpdateScore(int score) { }
        protected virtual void ShowGameOver(int finalScore) { }
        protected virtual void Cleanup() { }


    }
}
