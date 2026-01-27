using System;

namespace MiniGameFramework.Systems.Events
{
    /// <summary>
    /// Type-safe event without parameters
    /// Usage: 
    ///   public static readonly GameEvent OnGameStarted = new GameEvent();
    ///   EventManager.Instance.Subscribe(OnGameStarted, MyHandler);
    ///   EventManager.Instance.TriggerEvent(OnGameStarted);
    /// </summary>
    public readonly struct GameEvent
    {
        public readonly string Name;

        public GameEvent(string name = null)
        {
            Name = name ?? Guid.NewGuid().ToString();
        }

        public override string ToString() => Name;
        public override int GetHashCode() => Name?.GetHashCode() ?? 0;
    }

    /// <summary>
    /// Type-safe event with a single typed parameter
    /// Usage:
    ///   public static readonly GameEvent&lt;float&gt; OnProgressChanged = new GameEvent&lt;float&gt;("OnProgressChanged");
    ///   EventManager.Instance.Subscribe(OnProgressChanged, (float progress) => Debug.Log(progress));
    ///   EventManager.Instance.TriggerEvent(OnProgressChanged, 0.5f);
    /// </summary>
    public readonly struct GameEvent<T>
    {
        public readonly string Name;

        public GameEvent(string name = null)
        {
            Name = name ?? $"{typeof(T).Name}_{Guid.NewGuid()}";
        }

        public override string ToString() => Name;
        public override int GetHashCode() => Name?.GetHashCode() ?? 0;
    }

    /// <summary>
    /// Type-safe event with two typed parameters
    /// </summary>
    public readonly struct GameEvent<T1, T2>
    {
        public readonly string Name;

        public GameEvent(string name = null)
        {
            Name = name ?? $"{typeof(T1).Name}_{typeof(T2).Name}_{Guid.NewGuid()}";
        }

        public override string ToString() => Name;
        public override int GetHashCode() => Name?.GetHashCode() ?? 0;
    }
}
