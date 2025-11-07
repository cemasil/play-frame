using System;
using System.Collections.Generic;
using UnityEngine;
using MiniGameFramework.Core;

namespace MiniGameFramework.Systems.Events
{
    /// <summary>
    /// Central event manager for loose coupling between systems
    /// </summary>
    public class EventManager : PersistentSingleton<EventManager>
    {
        private Dictionary<string, Delegate> _eventDictionary = new Dictionary<string, Delegate>();

        public void Subscribe(string eventName, Action listener)
        {
            SubscribeInternal(eventName, listener);
        }

        public void Subscribe(string eventName, Action<object> listener)
        {
            SubscribeInternal(eventName, listener);
        }

        private void SubscribeInternal(string eventName, Delegate listener)
        {
            if (string.IsNullOrEmpty(eventName)) return;

            if (_eventDictionary.ContainsKey(eventName))
            {
                _eventDictionary[eventName] = Delegate.Combine(_eventDictionary[eventName], listener);
            }
            else
            {
                _eventDictionary.Add(eventName, listener);
            }
        }

        public void Unsubscribe(string eventName, Action listener)
        {
            UnsubscribeInternal(eventName, listener);
        }

        public void Unsubscribe(string eventName, Action<object> listener)
        {
            UnsubscribeInternal(eventName, listener);
        }

        private void UnsubscribeInternal(string eventName, Delegate listener)
        {
            if (_eventDictionary.ContainsKey(eventName))
            {
                _eventDictionary[eventName] = Delegate.Remove(_eventDictionary[eventName], listener);

                if (_eventDictionary[eventName] == null)
                {
                    _eventDictionary.Remove(eventName);
                }
            }
        }

        public void TriggerEvent(string eventName, object parameter = null)
        {
            if (!_eventDictionary.TryGetValue(eventName, out Delegate del)) return;

            if (del is Action<object> actionWithParam)
            {
                actionWithParam?.Invoke(parameter);
            }
            else if (del is Action action)
            {
                action?.Invoke();
            }
        }

        public void ClearAllEvents()
        {
            _eventDictionary.Clear();
        }

        public bool HasListeners(string eventName)
        {
            return _eventDictionary.ContainsKey(eventName) && _eventDictionary[eventName] != null;
        }
    }
}
