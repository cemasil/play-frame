using UnityEngine;

namespace PlayFrame.MiniGames.Common
{
    /// <summary>
    /// Base class for all game pieces (gems, cards, tiles, etc.)
    /// Unlike BaseGame which represents the game controller,
    /// this represents individual interactive elements within a game.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public abstract class BaseGamePiece : MonoBehaviour
    {
        protected RectTransform rectTransform;
        protected bool isInitialized = false;

        public RectTransform RectTransform => rectTransform;
        public bool IsInitialized => isInitialized;

        protected virtual void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            Initialize();
        }

        public void Initialize()
        {
            if (isInitialized) return;

            OnInitialize();
            isInitialized = true;
        }

        protected virtual void OnDestroy()
        {
            OnCleanup();
        }

        /// <summary>
        /// Called once during initialization
        /// </summary>
        protected virtual void OnInitialize() { }

        /// <summary>
        /// Called when the piece is being destroyed
        /// </summary>
        protected virtual void OnCleanup() { }

        /// <summary>
        /// Set the visual state of the piece (selected, highlighted, etc.)
        /// </summary>
        public virtual void SetVisualState(PieceVisualState state) { }

        /// <summary>
        /// Reset the piece to its default state
        /// </summary>
        public virtual void ResetPiece() { }
    }

    /// <summary>
    /// Visual states for game pieces
    /// </summary>
    public enum PieceVisualState
    {
        Normal,
        Selected,
        Highlighted,
        Disabled,
        Matched
    }
}
