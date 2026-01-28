using UnityEngine;

namespace MiniGameFramework.Systems.Input
{
    /// <summary>
    /// Configuration settings for gesture detection.
    /// Create via: Create → MiniGameFramework → Input → Gesture Settings
    /// </summary>
    [CreateAssetMenu(fileName = "GestureSettings", menuName = "MiniGameFramework/Input/Gesture Settings")]
    public class GestureSettings : ScriptableObject
    {
        [Header("Swipe Settings")]
        [Tooltip("Minimum distance in pixels to register as swipe")]
        [SerializeField] private float swipeThreshold = 50f;

        [Tooltip("Maximum time in seconds for a swipe")]
        [SerializeField] private float maxSwipeTime = 0.5f;

        [Header("Tap Settings")]
        [Tooltip("Maximum distance in pixels for a tap")]
        [SerializeField] private float tapThreshold = 20f;

        [Tooltip("Maximum time in seconds for a tap")]
        [SerializeField] private float maxTapTime = 0.3f;

        [Tooltip("Maximum time between taps for double tap")]
        [SerializeField] private float doubleTapInterval = 0.3f;

        [Header("Long Press Settings")]
        [Tooltip("Minimum time in seconds for long press")]
        [SerializeField] private float longPressTime = 0.5f;

        [Header("General Settings")]
        [Tooltip("Dead zone for initial touch movement")]
        [SerializeField] private float deadZone = 10f;

        // Properties
        public float SwipeThreshold => swipeThreshold;
        public float MaxSwipeTime => maxSwipeTime;
        public float TapThreshold => tapThreshold;
        public float MaxTapTime => maxTapTime;
        public float DoubleTapInterval => doubleTapInterval;
        public float LongPressTime => longPressTime;
        public float DeadZone => deadZone;

        /// <summary>
        /// Default settings for quick setup
        /// </summary>
        public static GestureSettings CreateDefault()
        {
            var settings = CreateInstance<GestureSettings>();
            settings.swipeThreshold = 50f;
            settings.maxSwipeTime = 0.5f;
            settings.tapThreshold = 20f;
            settings.maxTapTime = 0.3f;
            settings.doubleTapInterval = 0.3f;
            settings.longPressTime = 0.5f;
            settings.deadZone = 10f;
            return settings;
        }
    }
}
