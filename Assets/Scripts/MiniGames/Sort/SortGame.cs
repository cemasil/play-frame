using System.Collections.Generic;
using PlayFrame.MiniGames.Common;
using PlayFrame.Systems.Grid;
using UnityEngine;

public class SortGame : BaseGame
{
    [SerializeField] private GridManager gridManager;

    private int score = 0;

    protected override async void OnGameStart()
    {
        // Validate GridInputHandler is attached
        if (gridManager.GetComponent<GridInputHandler>() == null)
        {
            Debug.LogError($"[SortGame] GridInputHandler is missing on '{gridManager.gameObject.name}'. " +
                           "Add GridInputHandler component to the GridManager GameObject in the Inspector.");
            return;
        }

        // Subscribe to piece configuration
        // If you don't subscribe to OnConfigurePiece, GridManager auto-configures
        // from GridConfig.pieceSprites + boardLayout. Subscribe only if you need custom logic.
        gridManager.OnConfigurePiece += ConfigurePiece;

        // --- Interaction Event Subscriptions ---
        // Uncomment/comment the ones you need for your game.

        // TAP: fires when a piece is tapped (short press)
        gridManager.OnPieceTapped += OnPieceTapped;

        // SWAP: fires when two pieces are swapped (swipe or tap-tap)
        gridManager.OnPiecesSwapped += OnPiecesSwapped;

        // DRAG & DROP: fires when a piece is dragged and dropped onto another cell
        gridManager.OnPieceDragDropped += OnPieceDragDropped;

        // MULTI-SELECT: fires when N pieces have been selected (N = GridConfig.multiSelectCount)
        gridManager.OnMultiSelectCompleted += OnMultiSelectCompleted;

        // CHAIN / DRAW: fires when finger is lifted after chaining/drawing through pieces
        gridManager.OnChainCompleted += OnChainCompleted;

        // MATCH: fires when pieces are about to be destroyed (before animation)
        gridManager.OnPiecesMatched += OnPiecesMatched;

        // DESTROY: fires after pieces are removed from the grid
        gridManager.OnPiecesDestroyed += OnPiecesDestroyed;

        // SPAWN: fires after new pieces are spawned
        gridManager.OnPiecesSpawned += OnPiecesSpawned;

        // Initialize and fill
        gridManager.InitializeGrid();
        await gridManager.FillGridAsync();
    }

    /// <summary>
    /// Called for each piece spawned. Set sprite, type, color here.
    /// If GridConfig.useBoardLayout is true, reads piece type from the board layout.
    /// Otherwise assigns randomly from GridConfig.pieceSprites.
    /// </summary>
    private void ConfigurePiece(GridPiece piece, int col, int row)
    {
        var config = gridManager.Config;
        var sprites = config.pieceSprites;

        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning("No pieceSprites assigned in GridConfig!");
            return;
        }

        int type;
        if (config.useBoardLayout)
        {
            type = config.GetBoardLayoutPieceType(col, row);
            if (type < 0 || type >= sprites.Length)
                type = Random.Range(0, sprites.Length);
        }
        else
        {
            type = Random.Range(0, sprites.Length);
        }

        piece.PieceId = type;
        piece.PieceType = type.ToString();
        if (sprites[type] != null)
            piece.SetSprite(sprites[type]);
    }

    // ──────────────────────────────────────
    //  TAP
    // ──────────────────────────────────────
    private async void OnPieceTapped(GridPiece piece)
    {
        Debug.Log($"[Tap] Piece {piece.PieceType} at ({piece.Col},{piece.Row})");

        // Example: remove tapped piece
        await gridManager.RemovePiecesAsync(new List<GridPiece> { piece });
        await gridManager.CollapseGridAsync();
        await gridManager.FillGridAsync();
    }

    // ──────────────────────────────────────
    //  SWAP (Match-3 style)
    // ──────────────────────────────────────
    private async void OnPiecesSwapped(GridPiece a, GridPiece b)
    {
        Debug.Log($"[Swap] {a.PieceType} ↔ {b.PieceType}");

        var matches = FindMatches();
        if (matches.Count > 0)
        {
            await gridManager.RemovePiecesAsync(matches);
            await gridManager.CollapseGridAsync();
            await gridManager.FillGridAsync();
        }
        else
        {
            await gridManager.SwapBackAsync(a, b);
        }
    }

    // ──────────────────────────────────────
    //  DRAG & DROP
    // ──────────────────────────────────────
    private void OnPieceDragDropped(GridPiece piece, GridCell fromCell, GridCell toCell)
    {
        Debug.Log($"[DragDrop] {piece.PieceType} from ({fromCell.Col},{fromCell.Row}) → ({toCell.Col},{toCell.Row})");

        // Example: move piece to target cell
        if (toCell.IsEmpty)
        {
            fromCell.Piece = null;
            toCell.Piece = piece;
            piece.SetGridPosition(toCell.Col, toCell.Row);
            piece.Cell = toCell;
            piece.SetPosition(toCell.LocalPosition);
        }
        else
        {
            // Target occupied — swap or reject
            piece.SetPosition(fromCell.LocalPosition);
        }
    }

    // ──────────────────────────────────────
    //  MULTI-SELECT
    // ──────────────────────────────────────
    private async void OnMultiSelectCompleted(List<GridPiece> selected)
    {
        Debug.Log($"[MultiSelect] {selected.Count} pieces selected");

        // Example: check if all selected pieces match
        bool allMatch = true;
        int firstId = selected[0].PieceId;
        for (int i = 1; i < selected.Count; i++)
        {
            if (selected[i].PieceId != firstId)
            {
                allMatch = false;
                break;
            }
        }

        if (allMatch)
        {
            score += selected.Count * 100;
            await gridManager.RemovePiecesAsync(selected);
            await gridManager.CollapseGridAsync();
            await gridManager.FillGridAsync();
        }
        else
        {
            foreach (var p in selected)
                p.OnDeselected();
        }
    }

    // ──────────────────────────────────────
    //  CHAIN / DRAW
    // ──────────────────────────────────────
    private async void OnChainCompleted(List<GridPiece> chain)
    {
        Debug.Log($"[Chain] {chain.Count} pieces chained (type: {chain[0].PieceType})");

        score += chain.Count * chain.Count * 10;
        await gridManager.RemovePiecesAsync(chain);
        await gridManager.CollapseGridAsync();
        await gridManager.FillGridAsync();
    }

    // ──────────────────────────────────────
    //  LIFECYCLE EVENTS
    // ──────────────────────────────────────
    private void OnPiecesMatched(List<GridPiece> matched)
    {
        Debug.Log($"[Matched] {matched.Count} pieces matched");
        score += matched.Count * 100;
    }

    private void OnPiecesDestroyed(List<GridPiece> destroyed)
    {
        Debug.Log($"[Destroyed] {destroyed.Count} pieces removed from grid");
    }

    private void OnPiecesSpawned(List<GridPiece> spawned)
    {
        Debug.Log($"[Spawned] {spawned.Count} new pieces");
    }

    // ──────────────────────────────────────
    //  GAME LOGIC
    // ──────────────────────────────────────
    private List<GridPiece> FindMatches()
    {
        var matched = new HashSet<GridPiece>();
        int cols = gridManager.Columns;
        int rows = gridManager.Rows;

        // Horizontal matches (3+)
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols - 2; col++)
            {
                var p = gridManager.GetPiece(col, row);
                if (p == null) continue;

                int id = p.PieceId;
                int end = col + 1;
                while (end < cols)
                {
                    var next = gridManager.GetPiece(end, row);
                    if (next == null || next.PieceId != id) break;
                    end++;
                }

                if (end - col >= 3)
                {
                    for (int c = col; c < end; c++)
                        matched.Add(gridManager.GetPiece(c, row));
                }
            }
        }

        // Vertical matches (3+)
        for (int col = 0; col < cols; col++)
        {
            for (int row = 0; row < rows - 2; row++)
            {
                var p = gridManager.GetPiece(col, row);
                if (p == null) continue;

                int id = p.PieceId;
                int end = row + 1;
                while (end < rows)
                {
                    var next = gridManager.GetPiece(col, end);
                    if (next == null || next.PieceId != id) break;
                    end++;
                }

                if (end - row >= 3)
                {
                    for (int r = row; r < end; r++)
                        matched.Add(gridManager.GetPiece(col, r));
                }
            }
        }

        return new List<GridPiece>(matched);
    }
}