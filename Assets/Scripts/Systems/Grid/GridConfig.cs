using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayFrame.Systems.Grid
{
    /// <summary>
    /// Master configuration for a grid-based game.
    /// Create via: Assets -> Create -> PlayFrame -> Grid -> Grid Config
    /// </summary>
    [CreateAssetMenu(fileName = "GridConfig", menuName = "PlayFrame/Grid/Grid Config")]
    public class GridConfig : ScriptableObject
    {
        [Header("Grid Dimensions")]
        [Tooltip("Number of columns")]
        [Range(2, 20)]
        public int columns = 6;

        [Tooltip("Number of rows")]
        [Range(2, 20)]
        public int rows = 6;

        [Tooltip("Grid shape")]
        public GridShape shape = GridShape.Rectangle;

        [Header("Cell Settings")]
        [Tooltip("Size of each cell in pixels")]
        [Range(40f, 200f)]
        public float cellSize = 90f;

        [Tooltip("Spacing between cells in pixels")]
        [Range(0f, 30f)]
        public float cellSpacing = 4f;

        [Tooltip("Custom cell mask (for irregularly shaped grids). True = active cell.")]
        public bool[] customCellMask;

        [Header("Interaction")]
        [Tooltip("Interaction modes enabled for this grid")]
        public GridInteractionMode interactionMode = GridInteractionMode.Swap;

        [Tooltip("For MultiSelect: how many selections before action triggers")]
        [Range(2, 10)]
        public int multiSelectCount = 2;

        [Tooltip("For Chain/Draw: minimum chain length to trigger action")]
        [Range(2, 20)]
        public int minChainLength = 3;

        [Tooltip("Allow diagonal connections for Chain/Draw modes")]
        public bool allowDiagonals = false;

        [Header("Spawn Settings")]
        [Tooltip("How pieces spawn into the grid")]
        public GridSpawnMode spawnMode = GridSpawnMode.FallFromTop;

        [Tooltip("Duration of spawn/fall animation in seconds")]
        [Range(0.1f, 1f)]
        public float spawnAnimationDuration = 0.3f;

        [Tooltip("Delay between each piece spawn")]
        [Range(0f, 0.2f)]
        public float spawnStaggerDelay = 0.05f;

        [Tooltip("Use easing for spawn animations")]
        public bool useSpawnEasing = true;

        [Header("Visual Settings")]
        public GridVisualConfig visualConfig;

        [Header("Audio Settings")]
        public GridAudioConfig audioConfig;

        [Header("Default Cell Properties")]
        [Tooltip("Default interaction capabilities for all cells")]
        public CellInteraction defaultCellInteraction = CellInteraction.All;

        [Header("Board Layout")]
        [Tooltip("Piece sprites available for this grid (index = piece type ID)")]
        public Sprite[] pieceSprites;

        [Tooltip("Use a predefined board layout instead of random piece placement")]
        public bool useBoardLayout = false;

        [Tooltip("Per-cell piece type ID. -1 = empty. Array is [row * columns + col].")]
        [HideInInspector]
        public int[] boardLayout;

        /// <summary>
        /// Get the piece type for a specific cell in the board layout.
        /// Returns -1 if no layout is defined or cell is unset.
        /// </summary>
        public int GetBoardLayoutPieceType(int col, int row)
        {
            if (!useBoardLayout || boardLayout == null) return -1;
            int index = row * columns + col;
            if (index < 0 || index >= boardLayout.Length) return -1;
            return boardLayout[index];
        }

        /// <summary>
        /// Check if a cell position is valid for this grid configuration
        /// </summary>
        public bool IsCellActive(int col, int row)
        {
            if (col < 0 || col >= columns || row < 0 || row >= rows)
                return false;

            if (shape == GridShape.Custom && customCellMask != null)
            {
                int index = row * columns + col;
                return index < customCellMask.Length && customCellMask[index];
            }

            switch (shape)
            {
                case GridShape.Diamond:
                    int centerX = columns / 2;
                    int centerY = rows / 2;
                    return Mathf.Abs(col - centerX) + Mathf.Abs(row - centerY) <= Mathf.Min(centerX, centerY);

                case GridShape.Hexagonal:
                case GridShape.Rectangle:
                case GridShape.Square:
                default:
                    return true;
            }
        }

        /// <summary>
        /// Get total grid pixel dimensions
        /// </summary>
        public Vector2 GetGridPixelSize()
        {
            float width = columns * cellSize + (columns - 1) * cellSpacing;
            float height = rows * cellSize + (rows - 1) * cellSpacing;
            return new Vector2(width, height);
        }

        /// <summary>
        /// Get the local position of a cell within the grid
        /// </summary>
        public Vector2 GetCellLocalPosition(int col, int row)
        {
            Vector2 gridSize = GetGridPixelSize();
            float startX = -gridSize.x / 2f + cellSize / 2f;
            float startY = gridSize.y / 2f - cellSize / 2f;

            return new Vector2(
                startX + col * (cellSize + cellSpacing),
                startY - row * (cellSize + cellSpacing)
            );
        }

        /// <summary>
        /// Get the spawn start position for a cell (based on spawn mode)
        /// </summary>
        public Vector2 GetSpawnStartPosition(int col, int row)
        {
            Vector2 gridSize = GetGridPixelSize();
            Vector2 targetPos = GetCellLocalPosition(col, row);

            switch (spawnMode)
            {
                case GridSpawnMode.FallFromTop:
                    return new Vector2(targetPos.x, gridSize.y / 2f + cellSize);

                case GridSpawnMode.RiseFromBottom:
                    return new Vector2(targetPos.x, -gridSize.y / 2f - cellSize);

                case GridSpawnMode.SlideFromLeft:
                    return new Vector2(-gridSize.x / 2f - cellSize, targetPos.y);

                case GridSpawnMode.SlideFromRight:
                    return new Vector2(gridSize.x / 2f + cellSize, targetPos.y);

                case GridSpawnMode.SpawnInPlace:
                case GridSpawnMode.InitialOnly:
                default:
                    return targetPos;
            }
        }

        /// <summary>
        /// Check if a specific interaction mode is enabled
        /// </summary>
        public bool HasInteraction(GridInteractionMode mode)
        {
            return (interactionMode & mode) != 0;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (shape == GridShape.Square)
                rows = columns;

            if (shape == GridShape.Custom)
            {
                int expectedSize = columns * rows;
                if (customCellMask == null || customCellMask.Length != expectedSize)
                {
                    bool[] newMask = new bool[expectedSize];
                    if (customCellMask != null)
                    {
                        Array.Copy(customCellMask, newMask, Mathf.Min(customCellMask.Length, expectedSize));
                    }
                    else
                    {
                        for (int i = 0; i < expectedSize; i++)
                            newMask[i] = true;
                    }
                    customCellMask = newMask;
                }
            }

            // Resize board layout when dimensions change
            if (useBoardLayout)
            {
                int expectedSize = columns * rows;
                if (boardLayout == null || boardLayout.Length != expectedSize)
                {
                    int[] newLayout = new int[expectedSize];
                    for (int i = 0; i < expectedSize; i++)
                        newLayout[i] = -1;
                    if (boardLayout != null)
                        Array.Copy(boardLayout, newLayout, Mathf.Min(boardLayout.Length, expectedSize));
                    boardLayout = newLayout;
                }
            }
        }
#endif
    }
}
