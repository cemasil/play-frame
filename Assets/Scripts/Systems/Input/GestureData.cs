using System;
using UnityEngine;

namespace MiniGameFramework.Systems.Input
{
    /// <summary>
    /// Gesture types supported by the input system
    /// </summary>
    public enum GestureType
    {
        Tap,
        DoubleTap,
        LongPress,
        SwipeLeft,
        SwipeRight,
        SwipeUp,
        SwipeDown,
        DragStart,
        Dragging,
        DragEnd
    }

    /// <summary>
    /// Swipe direction for convenience
    /// </summary>
    public enum SwipeDirection
    {
        None,
        Left,
        Right,
        Up,
        Down
    }

    /// <summary>
    /// Data structure containing gesture information
    /// </summary>
    public struct GestureData
    {
        public GestureType Type;
        public Vector2 StartPosition;
        public Vector2 EndPosition;
        public Vector2 Delta;
        public float Duration;
        public SwipeDirection SwipeDirection;
        public GameObject Target;

        /// <summary>
        /// Get direction as Vector2Int (useful for grid-based games)
        /// </summary>
        public Vector2Int DirectionVector
        {
            get
            {
                return SwipeDirection switch
                {
                    SwipeDirection.Left => Vector2Int.left,
                    SwipeDirection.Right => Vector2Int.right,
                    SwipeDirection.Up => Vector2Int.up,
                    SwipeDirection.Down => Vector2Int.down,
                    _ => Vector2Int.zero
                };
            }
        }

        public static GestureData Create(GestureType type, Vector2 start, Vector2 end, float duration, GameObject target = null)
        {
            var delta = end - start;
            var swipeDir = SwipeDirection.None;

            if (type == GestureType.SwipeLeft || type == GestureType.SwipeRight ||
                type == GestureType.SwipeUp || type == GestureType.SwipeDown)
            {
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                {
                    swipeDir = delta.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
                }
                else
                {
                    swipeDir = delta.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
                }
            }

            return new GestureData
            {
                Type = type,
                StartPosition = start,
                EndPosition = end,
                Delta = delta,
                Duration = duration,
                SwipeDirection = swipeDir,
                Target = target
            };
        }
    }
}
