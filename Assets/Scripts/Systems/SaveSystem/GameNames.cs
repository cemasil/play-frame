namespace MiniGameFramework.Systems.SaveSystem
{
    /// <summary>
    /// Centralized game name constants
    /// </summary>
    public static class GameNames
    {
        public const string MATCH3 = "Match3";
        public const string MEMORY = "Memory";
        public const string ENDLESS_RUNNER = "EndlessRunner";
        public const string TILE_MATCHING = "TileMatching";
        public const string BLOCK_MATCHING = "BlockMatching";
        public const string TIC_TAC_TOE = "TicTacToe";

        public static string GetDisplayName(string gameName)
        {
            return gameName switch
            {
                MATCH3 => "Match-3 Puzzle",
                MEMORY => "Memory Game",
                ENDLESS_RUNNER => "Endless Runner",
                TILE_MATCHING => "Tile Matching",
                BLOCK_MATCHING => "Block Matching",
                TIC_TAC_TOE => "Tic-Tac-Toe",
                _ => gameName
            };
        }
    }
}
