using System;

namespace PlayFrame.Core.Events
{
    /// <summary>
    /// Base interface for all events in the framework.
    /// Provides a common contract for event identification and metadata.
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// Unique identifier for the event type
        /// </summary>
        string EventId { get; }

        /// <summary>
        /// Timestamp when the event was created
        /// </summary>
        DateTime Timestamp { get; }
    }

    /// <summary>
    /// Interface for events that carry data payload
    /// </summary>
    /// <typeparam name="T">Type of the event data</typeparam>
    public interface IEvent<out T> : IEvent
    {
        /// <summary>
        /// The data payload of the event
        /// </summary>
        T Data { get; }
    }
}
