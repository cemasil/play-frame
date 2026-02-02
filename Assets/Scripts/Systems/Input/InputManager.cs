using System;
using UnityEngine;
using PlayFrame.Core;

namespace PlayFrame.Systems.Input
{
    /// <summary>
    /// Central input manager that handles gesture detection and dispatching.
    /// Singleton that persists across scenes.
    /// </summary>
    public class InputManager : PersistentSingleton<InputManager>
    {
        [Header("Settings")]
        [SerializeField] private GestureSettings gestureSettings;
        [SerializeField] private bool enableOnStart = true;

        private GestureDetector _gestureDetector;

        /// <summary>
        /// Event fired when any gesture is detected
        /// </summary>
        public event Action<GestureData> OnGesture;

        /// <summary>
        /// Event fired on tap
        /// </summary>
        public event Action<GestureData> OnTap;

        /// <summary>
        /// Event fired on double tap
        /// </summary>
        public event Action<GestureData> OnDoubleTap;

        /// <summary>
        /// Event fired on long press
        /// </summary>
        public event Action<GestureData> OnLongPress;

        /// <summary>
        /// Event fired on any swipe
        /// </summary>
        public event Action<GestureData> OnSwipe;

        /// <summary>
        /// Is input detection enabled
        /// </summary>
        public bool IsEnabled
        {
            get => _gestureDetector?.IsEnabled ?? false;
            set
            {
                if (_gestureDetector != null)
                    _gestureDetector.IsEnabled = value;
            }
        }

        /// <summary>
        /// Current gesture settings
        /// </summary>
        public GestureSettings Settings => gestureSettings;

        protected override void OnSingletonAwake()
        {
            InitializeDetector();
        }

        private void InitializeDetector()
        {
            // Use default settings if not assigned
            if (gestureSettings == null)
            {
                gestureSettings = GestureSettings.CreateDefault();
            }

            _gestureDetector = new GestureDetector(gestureSettings);
            _gestureDetector.OnGestureDetected += HandleGestureDetected;
            _gestureDetector.IsEnabled = enableOnStart;
        }

        private void Update()
        {
            _gestureDetector?.UpdateDetection();
        }

        private void HandleGestureDetected(GestureData gesture)
        {
            // Fire general gesture event
            OnGesture?.Invoke(gesture);

            // Fire specific events
            switch (gesture.Type)
            {
                case GestureType.Tap:
                    OnTap?.Invoke(gesture);
                    NotifyTarget<ITappable>(gesture, t => t.OnTap());
                    break;

                case GestureType.DoubleTap:
                    OnDoubleTap?.Invoke(gesture);
                    break;

                case GestureType.LongPress:
                    OnLongPress?.Invoke(gesture);
                    NotifyTarget<ILongPressable>(gesture, t => t.OnLongPress());
                    break;

                case GestureType.SwipeLeft:
                case GestureType.SwipeRight:
                case GestureType.SwipeUp:
                case GestureType.SwipeDown:
                    OnSwipe?.Invoke(gesture);
                    NotifyTarget<ISwipeable>(gesture, t => t.OnSwipe(gesture.SwipeDirection));
                    break;
            }

            // Notify IGestureReceiver if target implements it
            NotifyTarget<IGestureReceiver>(gesture, t => t.OnGestureReceived(gesture));
        }

        private void NotifyTarget<T>(GestureData gesture, Action<T> action) where T : class
        {
            if (gesture.Target == null) return;

            var receiver = gesture.Target.GetComponent<T>();
            if (receiver != null)
            {
                action(receiver);
            }
        }

        /// <summary>
        /// Temporarily disable input (e.g., during animations)
        /// </summary>
        public void DisableInput()
        {
            IsEnabled = false;
        }

        /// <summary>
        /// Re-enable input
        /// </summary>
        public void EnableInput()
        {
            IsEnabled = true;
        }

        protected override void OnDestroy()
        {
            if (_gestureDetector != null)
            {
                _gestureDetector.OnGestureDetected -= HandleGestureDetected;
            }
            base.OnDestroy();
        }
    }
}
