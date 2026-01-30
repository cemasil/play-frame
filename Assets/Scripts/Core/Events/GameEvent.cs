using System;

namespace PlayFrame.Core.Events
{
    /// <summary>
    /// Type-safe event definition without parameters.
    /// This is a lightweight struct used for event registration and triggering.
    /// Usage: 
    ///   public static readonly GameEvent OnGameStarted = new GameEvent("OnGameStarted");
    ///   EventManager.Instance.Subscribe(OnGameStarted, MyHandler);
    ///   EventManager.Instance.TriggerEvent(OnGameStarted);
    /// </summary>
    public readonly struct GameEvent : IEquatable<GameEvent>
    {
        public readonly string Name;

        public GameEvent(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override string ToString() => Name;
        public override int GetHashCode() => Name?.GetHashCode() ?? 0;

        public bool Equals(GameEvent other) => Name == other.Name;
        public override bool Equals(object obj) => obj is GameEvent other && Equals(other);

        public static bool operator ==(GameEvent left, GameEvent right) => left.Equals(right);
        public static bool operator !=(GameEvent left, GameEvent right) => !left.Equals(right);
    }

    /// <summary>
    /// Type-safe event definition with a single typed parameter.
    /// Usage:
    ///   public static readonly GameEvent&lt;float&gt; OnProgressChanged = new GameEvent&lt;float&gt;("OnProgressChanged");
    ///   EventManager.Instance.Subscribe(OnProgressChanged, (float progress) => Debug.Log(progress));
    ///   EventManager.Instance.TriggerEvent(OnProgressChanged, 0.5f);
    /// </summary>
    public readonly struct GameEvent<T> : IEquatable<GameEvent<T>>
    {
        public readonly string Name;

        public GameEvent(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override string ToString() => $"{Name}<{typeof(T).Name}>";
        public override int GetHashCode() => Name?.GetHashCode() ?? 0;

        public bool Equals(GameEvent<T> other) => Name == other.Name;
        public override bool Equals(object obj) => obj is GameEvent<T> other && Equals(other);

        public static bool operator ==(GameEvent<T> left, GameEvent<T> right) => left.Equals(right);
        public static bool operator !=(GameEvent<T> left, GameEvent<T> right) => !left.Equals(right);
    }

    /// <summary>
    /// Type-safe event definition with two typed parameters.
    /// </summary>
    public readonly struct GameEvent<T1, T2> : IEquatable<GameEvent<T1, T2>>
    {
        public readonly string Name;

        public GameEvent(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override string ToString() => $"{Name}<{typeof(T1).Name}, {typeof(T2).Name}>";
        public override int GetHashCode() => Name?.GetHashCode() ?? 0;

        public bool Equals(GameEvent<T1, T2> other) => Name == other.Name;
        public override bool Equals(object obj) => obj is GameEvent<T1, T2> other && Equals(other);

        public static bool operator ==(GameEvent<T1, T2> left, GameEvent<T1, T2> right) => left.Equals(right);
        public static bool operator !=(GameEvent<T1, T2> left, GameEvent<T1, T2> right) => !left.Equals(right);
    }
}
