namespace MiniGameFramework.MiniGames.Common
{
    /// <summary>
    /// Standard game states for mini-games
    /// </summary>
    public enum GameState
    {
        /// <summary>
        /// Game is initializing (loading assets, setting up)
        /// </summary>
        Initializing,

        /// <summary>
        /// Game is ready to start, waiting for player input
        /// </summary>
        Ready,

        /// <summary>
        /// Game is actively being played
        /// </summary>
        Playing,

        /// <summary>
        /// Game is processing (animations, matching, etc.)
        /// Input should be blocked
        /// </summary>
        Processing,

        /// <summary>
        /// Game is paused
        /// </summary>
        Paused,

        /// <summary>
        /// Game is over (win or lose)
        /// </summary>
        GameOver
    }
}
