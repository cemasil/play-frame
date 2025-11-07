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

        public void Subscribe(string eventName, Action listener)
        {
            if (string.IsNullOrEmpty(eventName))
            {
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
        }

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

        public void TriggerEvent(string eventName)
        {
            if (_eventDictionary.TryGetValue(eventName, out Action action))
            {
                action?.Invoke();
            }
        }

        public void Subscribe(string eventName, Action<object> listener)
        {
            if (string.IsNullOrEmpty(eventName))
            {
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
        }

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

        public void TriggerEvent(string eventName, object parameter)
        {
            if (_eventWithParamDictionary.TryGetValue(eventName, out Action<object> action))
            {
                action?.Invoke(parameter);
            }
        }

        public void ClearAllEvents()
        {
            _eventDictionary.Clear();
            _eventWithParamDictionary.Clear();
        }

        public bool HasListeners(string eventName)
        {
            return _eventDictionary.ContainsKey(eventName) || _eventWithParamDictionary.ContainsKey(eventName);
        }
    }
}
