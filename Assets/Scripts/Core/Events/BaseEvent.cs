using System;

namespace PlayFrame.Core.Events
{
    /// <summary>
    /// Abstract base class for all events.
    /// Provides common functionality like timestamp and event identification.
    /// </summary>
    public abstract class BaseEvent : IEvent
    {
        public string EventId { get; }
        public DateTime Timestamp { get; }

        protected BaseEvent(string eventId)
        {
            EventId = eventId;
            Timestamp = DateTime.UtcNow;
        }

        public override string ToString()
        {
            return $"[{EventId}] at {Timestamp:HH:mm:ss.fff}";
        }
    }

    /// <summary>
    /// Abstract base class for events with typed data payload.
    /// </summary>
    /// <typeparam name="T">Type of the event data</typeparam>
    public abstract class BaseEvent<T> : BaseEvent, IEvent<T>
    {
        public T Data { get; }

        protected BaseEvent(string eventId, T data) : base(eventId)
        {
            Data = data;
        }

        public override string ToString()
        {
            return $"[{EventId}] Data: {Data} at {Timestamp:HH:mm:ss.fff}";
        }
    }
}
