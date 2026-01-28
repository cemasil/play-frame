using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MiniGameFramework.Systems.Input
{
    /// <summary>
    /// Attach to any UI element to receive swipe gestures.
    /// Simpler alternative to implementing ISwipeable manually.
    /// Works independently of InputManager for UI-specific interactions.
    /// </summary>
    public class SwipeHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [Header("Settings")]
        [SerializeField] private float swipeThreshold = 50f;
        [SerializeField] private bool detectOnDrag = true;

        /// <summary>
        /// Called when swipe is detected
        /// </summary>
        public event Action<SwipeDirection> OnSwipeDetected;

        /// <summary>
        /// Called when swipe is detected with this object reference
        /// </summary>
        public event Action<SwipeHandler, SwipeDirection> OnSwipeWithSender;

        private Vector2 _startPosition;
        private bool _isTracking;
        private bool _swipeHandled;

        public void OnPointerDown(PointerEventData eventData)
        {
            _startPosition = eventData.position;
            _isTracking = true;
            _swipeHandled = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isTracking || !detectOnDrag || _swipeHandled) return;

            TryDetectSwipe(eventData.position);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_isTracking && !_swipeHandled)
            {
                TryDetectSwipe(eventData.position);
            }

            _isTracking = false;
            _swipeHandled = false;
        }

        private void TryDetectSwipe(Vector2 endPosition)
        {
            Vector2 delta = endPosition - _startPosition;

            if (delta.magnitude < swipeThreshold) return;

            SwipeDirection direction;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                direction = delta.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
            }
            else
            {
                direction = delta.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
            }

            _swipeHandled = true;
            EmitSwipe(direction);
        }

        private void EmitSwipe(SwipeDirection direction)
        {
            OnSwipeDetected?.Invoke(direction);
            OnSwipeWithSender?.Invoke(this, direction);

            // Also notify if this object implements ISwipeable
            var swipeable = GetComponent<ISwipeable>();
            swipeable?.OnSwipe(direction);
        }

        /// <summary>
        /// Get direction as Vector2Int (useful for grid-based games)
        /// Note: Y is inverted for UI grid systems where row 0 is at the top
        /// </summary>
        public static Vector2Int GetDirectionVector(SwipeDirection direction)
        {
            return direction switch
            {
                SwipeDirection.Left => Vector2Int.left,
                SwipeDirection.Right => Vector2Int.right,
                SwipeDirection.Up => Vector2Int.down,    // UI grid: up swipe = decrease Y
                SwipeDirection.Down => Vector2Int.up,    // UI grid: down swipe = increase Y
                _ => Vector2Int.zero
            };
        }
    }
}
