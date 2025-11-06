using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using MiniGameFramework.Systems.Events;

namespace MiniGameFramework.Tests.EditMode.Systems
{
    [TestFixture]
    public class EventManagerTests
    {
        private GameObject _eventManagerGameObject;
        private EventManager _eventManager;
        private bool _eventTriggered;
        private object _receivedParameter;

        [SetUp]
        public void Setup()
        {
            _eventManagerGameObject = new GameObject("TestEventManager");
            _eventManager = _eventManagerGameObject.AddComponent<EventManager>();
            _eventManager.ClearAllEvents();
            _eventTriggered = false;
            _receivedParameter = null;
        }

        [TearDown]
        public void Teardown()
        {
            if (_eventManager != null)
            {
                _eventManager.ClearAllEvents();
            }

            if (_eventManagerGameObject != null)
            {
                Object.DestroyImmediate(_eventManagerGameObject);
            }
        }

        #region Simple Events (No Parameters)

        [Test]
        public void EventManager_Subscribe_ShouldRegisterListener()
        {
            // Arrange
            string eventName = "TestEvent";

            // Act
            _eventManager.Subscribe(eventName, OnTestEvent);

            // Assert
            Assert.IsTrue(_eventManager.HasListeners(eventName),
                "Event should have listeners after subscription");
        }

        [Test]
        public void EventManager_TriggerEvent_ShouldInvokeListener()
        {
            string eventName = "TestEvent";
            _eventManager.Subscribe(eventName, OnTestEvent);

            _eventManager.TriggerEvent(eventName);

            Assert.IsTrue(_eventTriggered, "Event listener should be invoked");
        }

        [Test]
        public void EventManager_Unsubscribe_ShouldRemoveListener()
        {
            string eventName = "TestEvent";
            _eventManager.Subscribe(eventName, OnTestEvent);

            _eventManager.Unsubscribe(eventName, OnTestEvent);
            _eventManager.TriggerEvent(eventName);

            Assert.IsFalse(_eventTriggered,
                "Event listener should not be invoked after unsubscribe");
        }

        [Test]
        public void EventManager_TriggerEvent_ShouldNotThrowError_WhenNoListeners()
        {
            string eventName = "NonExistentEvent";

            Assert.DoesNotThrow(() => _eventManager.TriggerEvent(eventName),
                "Triggering event with no listeners should not throw error");
        }

        [Test]
        public void EventManager_Subscribe_MultipleListeners_ShouldInvokeAll()
        {
            string eventName = "MultiListenerEvent";
            int callCount = 0;

            _eventManager.Subscribe(eventName, () => callCount++);
            _eventManager.Subscribe(eventName, () => callCount++);
            _eventManager.Subscribe(eventName, () => callCount++);

            _eventManager.TriggerEvent(eventName);

            Assert.AreEqual(3, callCount, "All listeners should be invoked");
        }

        #endregion

        #region Helper Methods

        private void OnTestEvent()
        {
            _eventTriggered = true;
        }

        private void OnParameterEvent(object parameter)
        {
            _eventTriggered = true;
            _receivedParameter = parameter;
        }

        private class TestData
        {
            public int Value { get; set; }
            public string Name { get; set; }
        }

        #endregion
    }
}
