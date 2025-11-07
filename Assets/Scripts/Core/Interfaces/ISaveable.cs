namespace MiniGameFramework.Core
{
    /// <summary>
    /// Interface for objects that can be saved and loaded
    /// Use this for game objects that need custom save/load logic
    /// Examples: Player profile, achievements, inventory, custom game states
    /// </summary>
    public interface ISaveable
    {
        /// <summary>
        /// Save this object's data
        /// </summary>
        void SaveData();

        /// <summary>
        /// Load this object's data
        /// </summary>
        void LoadData();
    }
}
