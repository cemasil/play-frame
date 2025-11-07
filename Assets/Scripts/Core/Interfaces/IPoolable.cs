namespace MiniGameFramework.Core
{
    /// <summary>
    /// Interface for objects that can be pooled
    /// </summary>
    public interface IPoolable
    {
        void OnSpawn();
        void OnDespawn();
    }
}
