using System;
using System.Collections.Generic;
using UnityEngine;
using PlayFrame.Core;

namespace PlayFrame.Systems.Events
{
    /// <summary>
    /// Central event manager for loose coupling between systems.
    /// Supports both type-safe generic events and legacy string-based events.
    /// </summary>
    public class EventManager : PersistentSingleton<EventManager>
    {
        private readonly Dictionary<string, Delegate> _eventDictionary = new Dictionary<string, Delegate>();

        #region Type-Safe Generic Events

        /// <summary>
        /// Subscribe to a type-safe event without parameters
        /// </summary>
        public void Subscribe(GameEvent gameEvent, Action listener)
        {
            SubscribeInternal(gameEvent.Name, listener);
        }

        /// <summary>
        /// Subscribe to a type-safe event with one typed parameter
        /// </summary>
        public void Subscribe<T>(GameEvent<T> gameEvent, Action<T> listener)
        {
            SubscribeInternal(gameEvent.Name, listener);
        }

        /// <summary>
        /// Subscribe to a type-safe event with two typed parameters
        /// </summary>
        public void Subscribe<T1, T2>(GameEvent<T1, T2> gameEvent, Action<T1, T2> listener)
        {
            SubscribeInternal(gameEvent.Name, listener);
        }

        /// <summary>
        /// Unsubscribe from a type-safe event without parameters
        /// </summary>
        public void Unsubscribe(GameEvent gameEvent, Action listener)
        {
            UnsubscribeInternal(gameEvent.Name, listener);
        }

        /// <summary>
        /// Unsubscribe from a type-safe event with one typed parameter
        /// </summary>
        public void Unsubscribe<T>(GameEvent<T> gameEvent, Action<T> listener)
        {
            UnsubscribeInternal(gameEvent.Name, listener);
        }

        /// <summary>
        /// Unsubscribe from a type-safe event with two typed parameters
        /// </summary>
        public void Unsubscribe<T1, T2>(GameEvent<T1, T2> gameEvent, Action<T1, T2> listener)
        {
            UnsubscribeInternal(gameEvent.Name, listener);
        }

        /// <summary>
        /// Trigger a type-safe event without parameters
        /// </summary>
        public void TriggerEvent(GameEvent gameEvent)
        {
            if (!_eventDictionary.TryGetValue(gameEvent.Name, out Delegate del)) return;

            if (del is Action action)
            {
                action.Invoke();
            }
            else
            {
                Debug.LogWarning($"[EventManager] Event '{gameEvent.Name}' has mismatched delegate type. Expected Action.");
            }
        }

        /// <summary>
        /// Trigger a type-safe event with one typed parameter
        /// </summary>
        public void TriggerEvent<T>(GameEvent<T> gameEvent, T parameter)
        {
            if (!_eventDictionary.TryGetValue(gameEvent.Name, out Delegate del)) return;

            if (del is Action<T> action)
            {
                action.Invoke(parameter);
            }
            else
            {
                Debug.LogWarning($"[EventManager] Event '{gameEvent.Name}' has mismatched delegate type. Expected Action<{typeof(T).Name}>.");
            }
        }

        /// <summary>
        /// Trigger a type-safe event with two typed parameters
        /// </summary>
        public void TriggerEvent<T1, T2>(GameEvent<T1, T2> gameEvent, T1 param1, T2 param2)
        {
            if (!_eventDictionary.TryGetValue(gameEvent.Name, out Delegate del)) return;

            if (del is Action<T1, T2> action)
            {
                action.Invoke(param1, param2);
            }
            else
            {
                Debug.LogWarning($"[EventManager] Event '{gameEvent.Name}' has mismatched delegate type. Expected Action<{typeof(T1).Name}, {typeof(T2).Name}>.");
            }
        }

        #endregion

        #region Legacy String-Based Events (Backward Compatibility)

        /// <summary>
        /// [Legacy] Subscribe to a string-based event without parameters
        /// Consider using type-safe GameEvent instead
        /// </summary>
        public void Subscribe(string eventName, Action listener)
        {
            SubscribeInternal(eventName, listener);
        }

        /// <summary>
        /// [Legacy] Subscribe to a string-based event with object parameter
        /// Consider using type-safe GameEvent&lt;T&gt; instead
        /// </summary>
        public void Subscribe(string eventName, Action<object> listener)
        {
            SubscribeInternal(eventName, listener);
        }

        /// <summary>
        /// [Legacy] Unsubscribe from a string-based event
        /// </summary>
        public void Unsubscribe(string eventName, Action listener)
        {
            UnsubscribeInternal(eventName, listener);
        }

        /// <summary>
        /// [Legacy] Unsubscribe from a string-based event with object parameter
        /// </summary>
        public void Unsubscribe(string eventName, Action<object> listener)
        {
            UnsubscribeInternal(eventName, listener);
        }

        /// <summary>
        /// [Legacy] Trigger a string-based event
        /// Consider using type-safe TriggerEvent(GameEvent) instead
        /// </summary>
        public void TriggerEvent(string eventName, object parameter = null)
        {
            if (!_eventDictionary.TryGetValue(eventName, out Delegate del)) return;

            if (parameter == null && del is Action action)
            {
                action.Invoke();
            }
            else if (del is Action<object> actionWithParam)
            {
                actionWithParam.Invoke(parameter);
            }
            else if (del is Action actionNoParam)
            {
                actionNoParam.Invoke();
            }
        }

        #endregion

        #region Internal Methods

        private void SubscribeInternal(string eventName, Delegate listener)
        {
            if (string.IsNullOrEmpty(eventName)) return;

            if (_eventDictionary.TryGetValue(eventName, out Delegate existing))
            {
                _eventDictionary[eventName] = Delegate.Combine(existing, listener);
            }
            else
            {
                _eventDictionary.Add(eventName, listener);
            }
        }

        private void UnsubscribeInternal(string eventName, Delegate listener)
        {
            if (!_eventDictionary.TryGetValue(eventName, out Delegate existing)) return;

            var newDelegate = Delegate.Remove(existing, listener);

            if (newDelegate == null)
            {
                _eventDictionary.Remove(eventName);
            }
            else
            {
                _eventDictionary[eventName] = newDelegate;
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Clear all event subscriptions
        /// </summary>
        public void ClearAllEvents()
        {
            _eventDictionary.Clear();
        }

        /// <summary>
        /// Check if an event has any listeners
        /// </summary>
        public bool HasListeners(string eventName)
        {
            return _eventDictionary.ContainsKey(eventName) && _eventDictionary[eventName] != null;
        }

        /// <summary>
        /// Check if a type-safe event has any listeners
        /// </summary>
        public bool HasListeners(GameEvent gameEvent)
        {
            return HasListeners(gameEvent.Name);
        }

        /// <summary>
        /// Check if a type-safe event has any listeners
        /// </summary>
        public bool HasListeners<T>(GameEvent<T> gameEvent)
        {
            return HasListeners(gameEvent.Name);
        }

        #endregion
    }
}
