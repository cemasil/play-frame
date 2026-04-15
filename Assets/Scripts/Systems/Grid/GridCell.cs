using System;
using UnityEngine;

namespace PlayFrame.Systems.Grid
{
    /// <summary>
    /// Represents a single cell in the grid.
    /// Contains cell data, state, and the piece occupying it.
    /// </summary>
    [Serializable]
    public class GridCell
    {
        /// <summary>Column index (x coordinate)</summary>
        public int Col { get; private set; }

        /// <summary>Row index (y coordinate)</summary>
        public int Row { get; private set; }

        /// <summary>Whether this cell is part of the active grid</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Interaction capabilities of this cell</summary>
        public CellInteraction Interaction { get; set; } = CellInteraction.All;

        /// <summary>The piece currently occupying this cell (null if empty)</summary>
        public GridPiece Piece { get; set; }

        /// <summary>Whether this cell is currently empty</summary>
        public bool IsEmpty => Piece == null;

        /// <summary>Whether this cell is blocked (not interactable)</summary>
        public bool IsBlocked => !IsActive || Interaction == CellInteraction.None;

        /// <summary>Local position within the grid RectTransform</summary>
        public Vector2 LocalPosition { get; set; }

        /// <summary>Custom data that can be attached to the cell</summary>
        public object CustomData { get; set; }

        public GridCell(int col, int row)
        {
            Col = col;
            Row = row;
        }

        /// <summary>Get grid coordinate as Vector2Int</summary>
        public Vector2Int Coordinate => new Vector2Int(Col, Row);

        /// <summary>Check if this cell supports a specific interaction</summary>
        public bool SupportsInteraction(CellInteraction interaction)
        {
            return IsActive && (Interaction & interaction) != 0;
        }

        /// <summary>Check if this cell is adjacent to another cell</summary>
        public bool IsAdjacentTo(GridCell other, bool includeDiagonals = false)
        {
            int dx = Mathf.Abs(Col - other.Col);
            int dy = Mathf.Abs(Row - other.Row);

            if (includeDiagonals)
                return dx <= 1 && dy <= 1 && (dx + dy > 0);
            else
                return (dx + dy) == 1;
        }

        public override string ToString()
        {
            return $"Cell({Col},{Row}) Active:{IsActive} Piece:{(Piece != null ? Piece.PieceType : "none")}";
        }
    }
}
