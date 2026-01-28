using System;
using UnityEngine;
using UnityEngine.UI;
using MiniGameFramework.MiniGames.Common;
using MiniGameFramework.Systems.Input;

namespace MiniGameFramework.MiniGames.Match3
{
    /// <summary>
    /// Represents a single gem on the Match3 grid.
    /// Uses SwipeHandler for input abstraction.
    /// </summary>
    [RequireComponent(typeof(SwipeHandler))]
    public class Gem : BaseGamePiece, ISwipeable
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int ColorIndex { get; private set; }

        private Image _image;
        private SwipeHandler _swipeHandler;
        private Action<Gem, Vector2Int> _onGemSwipe;

        protected override void OnInitialize()
        {
            _image = GetComponent<Image>();
            _swipeHandler = GetComponent<SwipeHandler>();

            if (_swipeHandler == null)
            {
                _swipeHandler = gameObject.AddComponent<SwipeHandler>();
            }
        }

        /// <summary>
        /// Register callback for swipe events
        /// </summary>
        public void OnSwipeCallback(Action<Gem, Vector2Int> swipeCallback)
        {
            _onGemSwipe = swipeCallback;
        }

        /// <summary>
        /// Called by ISwipeable interface when swipe is detected
        /// </summary>
        public void OnSwipe(SwipeDirection direction)
        {
            var directionVector = SwipeHandler.GetDirectionVector(direction);
            _onGemSwipe?.Invoke(this, directionVector);
        }

        public void SetColor(Color color, int colorIndex)
        {
            if (_image != null)
            {
                _image.color = color;
                ColorIndex = colorIndex;
            }
        }

        public void SetPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override void SetVisualState(PieceVisualState state)
        {
            switch (state)
            {
                case PieceVisualState.Selected:
                    // Add selection visual (e.g., scale up, glow)
                    break;
                case PieceVisualState.Normal:
                    // Reset to normal state
                    break;
            }
        }

        public override void ResetPiece()
        {
            _onGemSwipe = null;
        }

        protected override void OnCleanup()
        {
            _onGemSwipe = null;
        }
    }
}
