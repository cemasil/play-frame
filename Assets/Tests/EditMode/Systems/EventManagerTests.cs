using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using PlayFrame.Core.Events;

namespace PlayFrame.Tests.EditMode.Systems
{
    [TestFixture]
    public class EventManagerTests
    {
        private GameObject _eventManagerGameObject;
        private EventManager _eventManager;
        private bool _eventTriggered;
        private int _receivedIntParameter;
        private string _receivedStringParameter;

        // Type-safe test events
        private static readonly GameEvent TestEvent = new("TestEvent");
        private static readonly GameEvent MultiListenerEvent = new("MultiListenerEvent");
        private static readonly GameEvent<int> TestIntEvent = new("TestIntEvent");
        private static readonly GameEvent<string> TestStringEvent = new("TestStringEvent");

        [SetUp]
        public void Setup()
        {
            _eventManagerGameObject = new GameObject("TestEventManager");
            _eventManager = _eventManagerGameObject.AddComponent<EventManager>();
            _eventManager.ClearAllEvents();
            _eventTriggered = false;
            _receivedIntParameter = 0;
            _receivedStringParameter = null;
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
            // Act
            _eventManager.Subscribe(TestEvent, OnTestEvent);

            // Assert
            Assert.IsTrue(_eventManager.HasListeners(TestEvent),
                "Event should have listeners after subscription");
        }

        [Test]
        public void EventManager_TriggerEvent_ShouldInvokeListener()
        {
            _eventManager.Subscribe(TestEvent, OnTestEvent);

            _eventManager.TriggerEvent(TestEvent);

            Assert.IsTrue(_eventTriggered, "Event listener should be invoked");
        }

        [Test]
        public void EventManager_Unsubscribe_ShouldRemoveListener()
        {
            _eventManager.Subscribe(TestEvent, OnTestEvent);

            _eventManager.Unsubscribe(TestEvent, OnTestEvent);
            _eventManager.TriggerEvent(TestEvent);

            Assert.IsFalse(_eventTriggered,
                "Event listener should not be invoked after unsubscribe");
        }

        [Test]
        public void EventManager_TriggerEvent_ShouldNotThrowError_WhenNoListeners()
        {
            var nonExistentEvent = new GameEvent("NonExistentEvent");

            Assert.DoesNotThrow(() => _eventManager.TriggerEvent(nonExistentEvent),
                "Triggering event with no listeners should not throw error");
        }

        [Test]
        public void EventManager_Subscribe_MultipleListeners_ShouldInvokeAll()
        {
            int callCount = 0;

            _eventManager.Subscribe(MultiListenerEvent, () => callCount++);
            _eventManager.Subscribe(MultiListenerEvent, () => callCount++);
            _eventManager.Subscribe(MultiListenerEvent, () => callCount++);

            _eventManager.TriggerEvent(MultiListenerEvent);

            Assert.AreEqual(3, callCount, "All listeners should be invoked");
        }

        #endregion

        #region Parameterized Events

        [Test]
        public void EventManager_TriggerEvent_WithIntParameter_ShouldPassParameter()
        {
            _eventManager.Subscribe(TestIntEvent, OnIntParameterEvent);

            _eventManager.TriggerEvent(TestIntEvent, 42);

            Assert.IsTrue(_eventTriggered, "Event listener should be invoked");
            Assert.AreEqual(42, _receivedIntParameter, "Parameter should be passed correctly");
        }

        [Test]
        public void EventManager_TriggerEvent_WithStringParameter_ShouldPassParameter()
        {
            _eventManager.Subscribe(TestStringEvent, OnStringParameterEvent);

            _eventManager.TriggerEvent(TestStringEvent, "TestValue");

            Assert.IsTrue(_eventTriggered, "Event listener should be invoked");
            Assert.AreEqual("TestValue", _receivedStringParameter, "Parameter should be passed correctly");
        }

        [Test]
        public void EventManager_Unsubscribe_WithParameter_ShouldRemoveListener()
        {
            _eventManager.Subscribe(TestIntEvent, OnIntParameterEvent);

            _eventManager.Unsubscribe(TestIntEvent, OnIntParameterEvent);
            _eventManager.TriggerEvent(TestIntEvent, 100);

            Assert.IsFalse(_eventTriggered,
                "Event listener should not be invoked after unsubscribe");
        }

        #endregion

        #region Helper Methods

        private void OnTestEvent()
        {
            _eventTriggered = true;
        }

        private void OnIntParameterEvent(int parameter)
        {
            _eventTriggered = true;
            _receivedIntParameter = parameter;
        }

        private void OnStringParameterEvent(string parameter)
        {
            _eventTriggered = true;
            _receivedStringParameter = parameter;
        }

        #endregion
    }
}
