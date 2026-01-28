using System;

namespace MiniGameFramework.Systems.Input
{
    /// <summary>
    /// Interface for gesture detection.
    /// Implement this for different input methods (touch, mouse, controller).
    /// </summary>
    public interface IGestureDetector
    {
        /// <summary>
        /// Called when a gesture is detected
        /// </summary>
        event Action<GestureData> OnGestureDetected;

        /// <summary>
        /// Enable or disable gesture detection
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Update gesture detection (called each frame)
        /// </summary>
        void UpdateDetection();
    }

    /// <summary>
    /// Interface for objects that can receive gestures
    /// </summary>
    public interface IGestureReceiver
    {
        /// <summary>
        /// Handle a detected gesture
        /// </summary>
        void OnGestureReceived(GestureData gesture);
    }

    /// <summary>
    /// Interface for swipeable objects (simplified for common use case)
    /// </summary>
    public interface ISwipeable
    {
        /// <summary>
        /// Called when object is swiped
        /// </summary>
        void OnSwipe(SwipeDirection direction);
    }

    /// <summary>
    /// Interface for tappable objects
    /// </summary>
    public interface ITappable
    {
        /// <summary>
        /// Called when object is tapped
        /// </summary>
        void OnTap();
    }

    /// <summary>
    /// Interface for long-pressable objects
    /// </summary>
    public interface ILongPressable
    {
        /// <summary>
        /// Called when object is long-pressed
        /// </summary>
        void OnLongPress();
    }
}
