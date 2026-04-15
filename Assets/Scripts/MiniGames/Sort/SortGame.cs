using PlayFrame.MiniGames.Common;
using PlayFrame.Systems.Grid;
using UnityEngine;

public class SortGame : BaseGame
{
    [SerializeField] private GridManager gridManager;

    protected override void OnGameStart()
    {
        gridManager.OnConfigurePiece += ConfigurePiece;
        gridManager.InitializeGrid();
        gridManager.FillGridImmediate();
        StartGame(); // Explicit start
    }

    private void ConfigurePiece(GridPiece piece, int col, int row)
    {
        // Configure randomly
    }
}