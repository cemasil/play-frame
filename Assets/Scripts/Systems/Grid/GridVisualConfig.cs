using System;
using UnityEngine;

namespace PlayFrame.Systems.Grid
{
    /// <summary>
    /// Visual configuration for the grid.
    /// Create via: Assets -> Create -> PlayFrame -> Grid -> Visual Config
    /// </summary>
    [CreateAssetMenu(fileName = "GridVisualConfig", menuName = "PlayFrame/Grid/Visual Config")]
    public class GridVisualConfig : ScriptableObject
    {
        [Header("Grid Background")]
        [Tooltip("Background sprite for the entire grid area")]
        public Sprite gridBackground;

        [Tooltip("Background color/tint")]
        public Color gridBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);

        [Tooltip("Padding around the grid content (left, right, top, bottom)")]
        public int gridPaddingLeft = 10;
        public int gridPaddingRight = 10;
        public int gridPaddingTop = 10;
        public int gridPaddingBottom = 10;

        /// <summary>Get padding as a RectOffset at runtime</summary>
        public RectOffset GridPadding => new RectOffset(gridPaddingLeft, gridPaddingRight, gridPaddingTop, gridPaddingBottom);

        [Header("Grid Border/Frame")]
        [Tooltip("Border/frame sprite around the grid")]
        public Sprite gridBorderSprite;

        [Tooltip("Border color")]
        public Color gridBorderColor = Color.white;

        [Tooltip("Border width in pixels")]
        [Range(0f, 20f)]
        public float borderWidth = 4f;

        [Tooltip("Dynamically adjust border to fit grid shape")]
        public bool dynamicBorder = true;

        [Header("Cell Visuals")]
        [Tooltip("Default background sprite for each cell")]
        public Sprite cellBackground;

        [Tooltip("Cell background color")]
        public Color cellBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

        [Tooltip("Cell corner radius (if using rounded sprite)")]
        [Range(0f, 1f)]
        public float cellCornerRadius = 0.1f;

        [Header("Selection Visuals")]
        [Tooltip("Overlay sprite when a piece is selected")]
        public Sprite selectionOverlay;

        [Tooltip("Selection highlight color")]
        public Color selectionColor = new Color(1f, 1f, 0f, 0.5f);

        [Tooltip("Scale multiplier when piece is selected")]
        [Range(1f, 1.5f)]
        public float selectionScale = 1.1f;

        [Header("Drag Visuals")]
        [Tooltip("Scale multiplier when piece is being dragged")]
        [Range(1f, 2f)]
        public float dragScale = 1.2f;

        [Tooltip("Alpha when piece is being dragged")]
        [Range(0.3f, 1f)]
        public float dragAlpha = 0.8f;

        [Tooltip("Show ghost at original position during drag")]
        public bool showDragGhost = true;

        [Tooltip("Ghost alpha")]
        [Range(0f, 0.5f)]
        public float dragGhostAlpha = 0.3f;

        [Header("Chain/Draw Visuals")]
        [Tooltip("Line material for drawing chain connections")]
        public Material chainLineMaterial;

        [Tooltip("Line color for chain connections")]
        public Color chainLineColor = Color.white;

        [Tooltip("Line width for chain connections")]
        [Range(1f, 10f)]
        public float chainLineWidth = 3f;

        [Header("Match/Destroy Visuals")]
        [Tooltip("Particle effect prefab for piece destruction")]
        public GameObject destroyParticlePrefab;

        [Tooltip("Scale animation on match before destroy")]
        public bool matchScaleAnimation = true;

        [Tooltip("Match highlight color")]
        public Color matchHighlightColor = new Color(1f, 1f, 1f, 0.8f);

        [Header("Spawn Visuals")]
        [Tooltip("Start scale for spawn animation (0 = invisible)")]
        [Range(0f, 1f)]
        public float spawnStartScale = 0f;

        [Tooltip("Spawn start alpha")]
        [Range(0f, 1f)]
        public float spawnStartAlpha = 0f;

        [Header("Hint Visuals")]
        [Tooltip("Hint highlight sprite")]
        public Sprite hintOverlay;

        [Tooltip("Hint color")]
        public Color hintColor = new Color(1f, 1f, 1f, 0.4f);

        [Tooltip("Hint pulse speed")]
        [Range(0.5f, 4f)]
        public float hintPulseSpeed = 2f;
    }
}
