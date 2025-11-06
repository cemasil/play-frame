using System;
using System.Collections.Generic;
using UnityEngine;
using MiniGameFramework.Core;

namespace MiniGameFramework.Systems.Events
{
    /// <summary>
    /// Central event manager for loose coupling between systems
    /// Uses string-based events with optional parameters
    /// </summary>
    public class EventManager : PersistentSingleton<EventManager>
    {
        private Dictionary<string, Action> _eventDictionary = new Dictionary<string, Action>();
        private Dictionary<string, Action<object>> _eventWithParamDictionary = new Dictionary<string, Action<object>>();

        #region Simple Events (No Parameters)

        /// <summary>
        /// Subscribe to an event with no parameters
        /// </summary>
        public void Subscribe(string eventName, Action listener)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                Debug.LogError("[EventManager] Event name cannot be null or empty!");
                return;
            }

            if (_eventDictionary.ContainsKey(eventName))
            {
                _eventDictionary[eventName] += listener;
            }
            else
            {
                _eventDictionary.Add(eventName, listener);
            }

            Debug.Log($"[EventManager] Subscribed to event: {eventName}");
        }

        /// <summary>
        /// Unsubscribe from an event with no parameters
        /// </summary>
        public void Unsubscribe(string eventName, Action listener)
        {
            if (_eventDictionary.ContainsKey(eventName))
            {
                _eventDictionary[eventName] -= listener;

                if (_eventDictionary[eventName] == null)
                {
                    _eventDictionary.Remove(eventName);
                }
            }
        }

        /// <summary>
        /// Trigger an event with no parameters
        /// </summary>
        public void TriggerEvent(string eventName)
        {
            if (_eventDictionary.TryGetValue(eventName, out Action action))
            {
                action?.Invoke();
                Debug.Log($"[EventManager] Triggered event: {eventName}");
            }
        }

        #endregion

        #region Events With Parameters

        /// <summary>
        /// Subscribe to an event with a parameter
        /// </summary>
        public void Subscribe(string eventName, Action<object> listener)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                Debug.LogError("[EventManager] Event name cannot be null or empty!");
                return;
            }

            if (_eventWithParamDictionary.ContainsKey(eventName))
            {
                _eventWithParamDictionary[eventName] += listener;
            }
            else
            {
                _eventWithParamDictionary.Add(eventName, listener);
            }

            Debug.Log($"[EventManager] Subscribed to event with param: {eventName}");
        }

        /// <summary>
        /// Unsubscribe from an event with a parameter
        /// </summary>
        public void Unsubscribe(string eventName, Action<object> listener)
        {
            if (_eventWithParamDictionary.ContainsKey(eventName))
            {
                _eventWithParamDictionary[eventName] -= listener;

                if (_eventWithParamDictionary[eventName] == null)
                {
                    _eventWithParamDictionary.Remove(eventName);
                }
            }
        }

        /// <summary>
        /// Trigger an event with a parameter
        /// </summary>
        public void TriggerEvent(string eventName, object parameter)
        {
            if (_eventWithParamDictionary.TryGetValue(eventName, out Action<object> action))
            {
                action?.Invoke(parameter);
                Debug.Log($"[EventManager] Triggered event with param: {eventName}");
            }
        }

        #endregion

        #region Utility

        /// <summary>
        /// Clear all event subscriptions
        /// </summary>
        public void ClearAllEvents()
        {
            _eventDictionary.Clear();
            _eventWithParamDictionary.Clear();
            Debug.Log("[EventManager] All events cleared");
        }

        /// <summary>
        /// Check if an event has any listeners
        /// </summary>
        public bool HasListeners(string eventName)
        {
            return _eventDictionary.ContainsKey(eventName) || _eventWithParamDictionary.ContainsKey(eventName);
        }

        #endregion
    }
}
