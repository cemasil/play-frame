using System;
using UnityEngine;

namespace PlayFrame.Systems.Grid
{
    /// <summary>
    /// Interaction modes supported by the grid system.
    /// Multiple modes can be combined using flags.
    /// </summary>
    [Flags]
    public enum GridInteractionMode
    {
        None = 0,

        /// <summary>Tap to select a single piece</summary>
        Tap = 1 << 0,

        /// <summary>Swipe between two adjacent pieces to swap them</summary>
        Swap = 1 << 1,

        /// <summary>Drag and drop a piece to another cell</summary>
        DragAndDrop = 1 << 2,

        /// <summary>Select multiple pieces one by one, action triggers after N selections</summary>
        MultiSelect = 1 << 3,

        /// <summary>Draw/trace a path through connected pieces without lifting finger</summary>
        Draw = 1 << 4,

        /// <summary>Chain/connect adjacent same-type pieces without lifting finger</summary>
        Chain = 1 << 5
    }

    /// <summary>
    /// How pieces spawn into the grid
    /// </summary>
    public enum GridSpawnMode
    {
        /// <summary>Pieces fall from the top row</summary>
        FallFromTop,

        /// <summary>Pieces rise from the bottom row</summary>
        RiseFromBottom,

        /// <summary>Pieces slide from the left column</summary>
        SlideFromLeft,

        /// <summary>Pieces slide from the right column</summary>
        SlideFromRight,

        /// <summary>Pieces appear in place (at destroyed positions)</summary>
        SpawnInPlace,

        /// <summary>Pieces are placed during initialization only (no respawn)</summary>
        InitialOnly
    }

    /// <summary>
    /// Grid shape types
    /// </summary>
    public enum GridShape
    {
        Rectangle,
        Square,
        Hexagonal,
        Diamond,
        Custom
    }

    /// <summary>
    /// Cell interaction capability
    /// </summary>
    [Flags]
    public enum CellInteraction
    {
        None = 0,
        Swipeable = 1 << 0,
        Tappable = 1 << 1,
        Draggable = 1 << 2,
        Selectable = 1 << 3,
        Chainable = 1 << 4,
        All = ~0
    }

    /// <summary>
    /// Piece visual state in the grid
    /// </summary>
    public enum GridPieceState
    {
        Idle,
        Selected,
        Highlighted,
        Dragging,
        Chaining,
        Matched,
        Destroying,
        Spawning,
        Falling,
        Disabled
    }

    /// <summary>
    /// Direction for grid neighbor lookups
    /// </summary>
    public enum GridDirection
    {
        Up,
        Down,
        Left,
        Right,
        UpLeft,
        UpRight,
        DownLeft,
        DownRight
    }
}
