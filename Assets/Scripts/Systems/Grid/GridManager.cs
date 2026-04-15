using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using PlayFrame.Core.Logging;
using PlayFrame.Core.Pooling;
using PlayFrame.Systems.Audio;
using ILogger = PlayFrame.Core.Logging.ILogger;

namespace PlayFrame.Systems.Grid
{
    /// <summary>
    /// Runtime grid manager. Creates and manages a grid of cells and pieces.
    /// Handles piece interactions, spawn/destroy animations, and grid events.
    /// Attach to a GameObject with a RectTransform.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class GridManager : MonoBehaviour
    {
        private static readonly ILogger _logger = LoggerFactory.CreateGame("GridManager");

        [Header("Configuration")]
        [SerializeField] private GridConfig config;

        [Header("Piece Prefab")]
        [Tooltip("Prefab must have a GridPiece component")]
        [SerializeField] private GridPiece piecePrefab;

        [Header("Grid Container")]
        [Tooltip("Parent transform for grid cells and pieces. If null, uses this transform")]
        [SerializeField] private RectTransform gridContainer;

        [Header("Cell Background Prefab")]
        [Tooltip("Optional prefab for cell background visuals")]
        [SerializeField] private GameObject cellBackgroundPrefab;

        [Header("Grid Background")]
        [Tooltip("Optional Image component for grid background")]
        [SerializeField] private Image gridBackgroundImage;

        [Header("Grid Border")]
        [Tooltip("Optional Image component for grid border")]
        [SerializeField] private Image gridBorderImage;

        [Header("Pool Settings")]
        [SerializeField] private int initialPoolSize = 36;
        [SerializeField] private int maxPoolSize = 100;

        // Grid data
        private GridCell[,] _cells;
        private ObjectPool<GridPiece> _piecePool;
        private RectTransform _rectTransform;
        private bool _isInitialized;
        private bool _isProcessing;

        // Interaction state
        private GridPiece _selectedPiece;
        private GridPiece _draggedPiece;
        private List<GridPiece> _chainedPieces = new List<GridPiece>();
        private List<GridPiece> _multiSelectedPieces = new List<GridPiece>();
        private Vector2 _dragStartPosition;

        // Audio state
        private float _lastSoundTime;

        #region Properties

        /// <summary>Grid configuration</summary>
        public GridConfig Config => config;

        /// <summary>Number of columns</summary>
        public int Columns => config.columns;

        /// <summary>Number of rows</summary>
        public int Rows => config.rows;

        /// <summary>Is the grid currently processing (animations, etc.)</summary>
        public bool IsProcessing => _isProcessing;

        /// <summary>Is the grid initialized</summary>
        public bool IsInitialized => _isInitialized;

        #endregion

        #region Events

        /// <summary>Fired when a piece is tapped</summary>
        public event Action<GridPiece> OnPieceTapped;

        /// <summary>Fired when two pieces are swapped</summary>
        public event Action<GridPiece, GridPiece> OnPiecesSwapped;

        /// <summary>Fired when a piece is dragged and dropped</summary>
        public event Action<GridPiece, GridCell, GridCell> OnPieceDragDropped;

        /// <summary>Fired when multi-select count is reached</summary>
        public event Action<List<GridPiece>> OnMultiSelectCompleted;

        /// <summary>Fired when a chain/draw is completed (finger lifted)</summary>
        public event Action<List<GridPiece>> OnChainCompleted;

        /// <summary>Fired when pieces are matched and ready for destruction</summary>
        public event Action<List<GridPiece>> OnPiecesMatched;

        /// <summary>Fired when pieces are destroyed</summary>
        public event Action<List<GridPiece>> OnPiecesDestroyed;

        /// <summary>Fired when new pieces are spawned</summary>
        public event Action<List<GridPiece>> OnPiecesSpawned;

        /// <summary>Fired when a piece selection changes</summary>
        public event Action<GridPiece> OnPieceSelected;

        /// <summary>Fired when a piece is deselected</summary>
        public event Action<GridPiece> OnPieceDeselected;

        /// <summary>
        /// Piece factory: called to configure a new piece. 
        /// Parameters: (GridPiece piece, int col, int row)
        /// The game must set PieceType, PieceId, sprite/color, etc.
        /// </summary>
        public event Action<GridPiece, int, int> OnConfigurePiece;

        #endregion

        #region Initialization

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (gridContainer == null)
                gridContainer = _rectTransform;
        }

        /// <summary>
        /// Initialize the grid with the current config. Call this to set up the grid.
        /// </summary>
        public void InitializeGrid()
        {
            if (config == null)
            {
                _logger.LogError("GridConfig is not assigned!");
                return;
            }

            CreateCells();
            InitializePool();
            ApplyVisuals();
            _isInitialized = true;
            _logger.Log($"Grid initialized: {Columns}x{Rows}, Mode: {config.interactionMode}");
        }

        /// <summary>
        /// Initialize with a specific config
        /// </summary>
        public void InitializeGrid(GridConfig gridConfig)
        {
            config = gridConfig;
            InitializeGrid();
        }

        private void CreateCells()
        {
            _cells = new GridCell[Columns, Rows];

            for (int col = 0; col < Columns; col++)
            {
                for (int row = 0; row < Rows; row++)
                {
                    var cell = new GridCell(col, row)
                    {
                        IsActive = config.IsCellActive(col, row),
                        Interaction = config.defaultCellInteraction,
                        LocalPosition = config.GetCellLocalPosition(col, row)
                    };
                    _cells[col, row] = cell;

                    // Create cell background visual
                    if (cell.IsActive && cellBackgroundPrefab != null)
                    {
                        CreateCellBackground(cell);
                    }
                }
            }
        }

        private void CreateCellBackground(GridCell cell)
        {
            var bg = Instantiate(cellBackgroundPrefab, gridContainer);
            var rt = bg.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = cell.LocalPosition;
                rt.sizeDelta = Vector2.one * config.cellSize;
            }

            if (config.visualConfig != null)
            {
                var img = bg.GetComponent<Image>();
                if (img != null)
                {
                    img.color = config.visualConfig.cellBackgroundColor;
                    if (config.visualConfig.cellBackground != null)
                        img.sprite = config.visualConfig.cellBackground;
                }
            }
        }

        private void InitializePool()
        {
            if (piecePrefab == null)
            {
                _logger.LogError("Piece prefab is not assigned!");
                return;
            }

            _piecePool = new ObjectPool<GridPiece>(
                piecePrefab,
                gridContainer,
                initialPoolSize,
                maxPoolSize,
                onCreate: null,
                onGet: p => p.gameObject.SetActive(true),
                onRelease: p => { p.ResetPiece(); p.gameObject.SetActive(false); }
            );
        }

        private void ApplyVisuals()
        {
            if (config.visualConfig == null) return;

            var vc = config.visualConfig;

            // Grid background
            if (gridBackgroundImage != null)
            {
                gridBackgroundImage.color = vc.gridBackgroundColor;
                if (vc.gridBackground != null)
                    gridBackgroundImage.sprite = vc.gridBackground;

                // Size the background to fit the grid
                var bgRt = gridBackgroundImage.GetComponent<RectTransform>();
                if (bgRt != null)
                {
                    Vector2 gridSize = config.GetGridPixelSize();
                    bgRt.sizeDelta = gridSize + new Vector2(
                        vc.gridPadding.left + vc.gridPadding.right,
                        vc.gridPadding.top + vc.gridPadding.bottom
                    );
                }
            }

            // Grid border
            if (gridBorderImage != null && vc.dynamicBorder)
            {
                gridBorderImage.color = vc.gridBorderColor;
                if (vc.gridBorderSprite != null)
                    gridBorderImage.sprite = vc.gridBorderSprite;

                var borderRt = gridBorderImage.GetComponent<RectTransform>();
                if (borderRt != null)
                {
                    Vector2 gridSize = config.GetGridPixelSize();
                    borderRt.sizeDelta = gridSize + new Vector2(
                        vc.gridPadding.left + vc.gridPadding.right + vc.borderWidth * 2,
                        vc.gridPadding.top + vc.gridPadding.bottom + vc.borderWidth * 2
                    );
                }
            }
        }

        #endregion

        #region Piece Management

        /// <summary>
        /// Fill all empty active cells with new pieces.
        /// Calls OnConfigurePiece for each piece to let the game set it up.
        /// </summary>
        public async UniTask FillGridAsync(CancellationToken ct = default)
        {
            var newPieces = new List<GridPiece>();

            for (int col = 0; col < Columns; col++)
            {
                for (int row = 0; row < Rows; row++)
                {
                    var cell = _cells[col, row];
                    if (!cell.IsActive || !cell.IsEmpty) continue;

                    var piece = SpawnPiece(col, row);
                    if (piece != null)
                        newPieces.Add(piece);
                }
            }

            if (newPieces.Count > 0)
            {
                await AnimateSpawnAsync(newPieces, ct);
                OnPiecesSpawned?.Invoke(newPieces);
            }
        }

        /// <summary>
        /// Fill grid synchronously (no animation)
        /// </summary>
        public void FillGridImmediate()
        {
            var newPieces = new List<GridPiece>();

            for (int col = 0; col < Columns; col++)
            {
                for (int row = 0; row < Rows; row++)
                {
                    var cell = _cells[col, row];
                    if (!cell.IsActive || !cell.IsEmpty) continue;

                    var piece = SpawnPiece(col, row);
                    if (piece != null)
                    {
                        piece.SetPosition(cell.LocalPosition);
                        piece.SetScale(1f);
                        piece.SetAlpha(1f);
                        newPieces.Add(piece);
                    }
                }
            }

            if (newPieces.Count > 0)
                OnPiecesSpawned?.Invoke(newPieces);
        }

        private GridPiece SpawnPiece(int col, int row)
        {
            if (_piecePool == null) return null;

            var piece = _piecePool.Get();
            var cell = _cells[col, row];

            piece.SetGridPosition(col, row);
            piece.Cell = cell;
            cell.Piece = piece;

            // Size the piece
            piece.RectTransform.sizeDelta = Vector2.one * config.cellSize;

            // Let the game configure the piece (type, color, sprite, etc.)
            OnConfigurePiece?.Invoke(piece, col, row);

            return piece;
        }

        /// <summary>
        /// Remove a piece from the grid and return it to the pool
        /// </summary>
        public void RemovePiece(GridPiece piece)
        {
            if (piece == null) return;

            var cell = GetCell(piece.Col, piece.Row);
            if (cell != null && cell.Piece == piece)
                cell.Piece = null;

            piece.Cell = null;
            _piecePool?.Release(piece);
        }

        /// <summary>
        /// Remove multiple pieces with optional animation
        /// </summary>
        public async UniTask RemovePiecesAsync(List<GridPiece> pieces, CancellationToken ct = default)
        {
            if (pieces == null || pieces.Count == 0) return;

            // Notify matched
            foreach (var piece in pieces)
                piece.OnMatched();

            OnPiecesMatched?.Invoke(pieces);
            PlaySound(config.audioConfig?.matchSound);

            // Animate destruction
            await AnimateDestroyAsync(pieces, ct);

            // Actually remove
            foreach (var piece in pieces)
                RemovePiece(piece);

            OnPiecesDestroyed?.Invoke(pieces);
            PlaySound(config.audioConfig?.destroySound);
        }

        /// <summary>
        /// Collapse/gravity: move pieces down to fill empty cells
        /// </summary>
        public async UniTask CollapseGridAsync(CancellationToken ct = default)
        {
            var movedPieces = new List<(GridPiece piece, Vector2 from, Vector2 to)>();

            switch (config.spawnMode)
            {
                case GridSpawnMode.FallFromTop:
                    CollapseFallFromTop(movedPieces);
                    break;
                case GridSpawnMode.RiseFromBottom:
                    CollapseRiseFromBottom(movedPieces);
                    break;
                default:
                    // No collapse for in-place or initial-only modes
                    return;
            }

            // Animate movements
            if (movedPieces.Count > 0)
            {
                var tasks = new List<UniTask>();
                foreach (var (piece, from, to) in movedPieces)
                {
                    tasks.Add(AnimateMovePieceAsync(piece, from, to, config.spawnAnimationDuration, ct));
                }
                await UniTask.WhenAll(tasks);
            }
        }

        private void CollapseFallFromTop(List<(GridPiece, Vector2, Vector2)> movedPieces)
        {
            for (int col = 0; col < Columns; col++)
            {
                int emptyRow = Rows - 1;

                for (int row = Rows - 1; row >= 0; row--)
                {
                    var cell = _cells[col, row];
                    if (!cell.IsActive) continue;

                    if (!cell.IsEmpty)
                    {
                        if (row != emptyRow)
                        {
                            var piece = cell.Piece;
                            var targetCell = _cells[col, emptyRow];

                            Vector2 fromPos = cell.LocalPosition;
                            Vector2 toPos = targetCell.LocalPosition;

                            // Move piece data
                            cell.Piece = null;
                            targetCell.Piece = piece;
                            piece.SetGridPosition(col, emptyRow);
                            piece.Cell = targetCell;

                            movedPieces.Add((piece, fromPos, toPos));
                        }
                        emptyRow--;
                    }
                }
            }
        }

        private void CollapseRiseFromBottom(List<(GridPiece, Vector2, Vector2)> movedPieces)
        {
            for (int col = 0; col < Columns; col++)
            {
                int emptyRow = 0;

                for (int row = 0; row < Rows; row++)
                {
                    var cell = _cells[col, row];
                    if (!cell.IsActive) continue;

                    if (!cell.IsEmpty)
                    {
                        if (row != emptyRow)
                        {
                            var piece = cell.Piece;
                            var targetCell = _cells[col, emptyRow];

                            Vector2 fromPos = cell.LocalPosition;
                            Vector2 toPos = targetCell.LocalPosition;

                            cell.Piece = null;
                            targetCell.Piece = piece;
                            piece.SetGridPosition(col, emptyRow);
                            piece.Cell = targetCell;

                            movedPieces.Add((piece, fromPos, toPos));
                        }
                        emptyRow++;
                    }
                }
            }
        }

        #endregion

        #region Grid Access

        /// <summary>Get a cell by coordinate</summary>
        public GridCell GetCell(int col, int row)
        {
            if (col < 0 || col >= Columns || row < 0 || row >= Rows)
                return null;
            return _cells[col, row];
        }

        /// <summary>Get a piece at a grid position</summary>
        public GridPiece GetPiece(int col, int row)
        {
            return GetCell(col, row)?.Piece;
        }

        /// <summary>Check if a position is valid and active</summary>
        public bool IsValidCell(int col, int row)
        {
            var cell = GetCell(col, row);
            return cell != null && cell.IsActive;
        }

        /// <summary>Get all active cells</summary>
        public List<GridCell> GetActiveCells()
        {
            var cells = new List<GridCell>();
            for (int col = 0; col < Columns; col++)
                for (int row = 0; row < Rows; row++)
                    if (_cells[col, row].IsActive)
                        cells.Add(_cells[col, row]);
            return cells;
        }

        /// <summary>Get all pieces currently on the grid</summary>
        public List<GridPiece> GetAllPieces()
        {
            var pieces = new List<GridPiece>();
            for (int col = 0; col < Columns; col++)
                for (int row = 0; row < Rows; row++)
                    if (_cells[col, row].Piece != null)
                        pieces.Add(_cells[col, row].Piece);
            return pieces;
        }

        /// <summary>Get neighboring cells</summary>
        public List<GridCell> GetNeighbors(int col, int row, bool includeDiagonals = false)
        {
            var neighbors = new List<GridCell>();
            int[][] offsets = includeDiagonals
                ? new[] { new[] { 0, 1 }, new[] { 0, -1 }, new[] { 1, 0 }, new[] { -1, 0 },
                          new[] { 1, 1 }, new[] { 1, -1 }, new[] { -1, 1 }, new[] { -1, -1 } }
                : new[] { new[] { 0, 1 }, new[] { 0, -1 }, new[] { 1, 0 }, new[] { -1, 0 } };

            foreach (var offset in offsets)
            {
                var cell = GetCell(col + offset[0], row + offset[1]);
                if (cell != null && cell.IsActive)
                    neighbors.Add(cell);
            }
            return neighbors;
        }

        /// <summary>Get the cell at a local position (for hit testing)</summary>
        public GridCell GetCellAtPosition(Vector2 localPosition)
        {
            float halfCellSize = config.cellSize / 2f;

            for (int col = 0; col < Columns; col++)
            {
                for (int row = 0; row < Rows; row++)
                {
                    var cell = _cells[col, row];
                    if (!cell.IsActive) continue;

                    if (Mathf.Abs(localPosition.x - cell.LocalPosition.x) <= halfCellSize &&
                        Mathf.Abs(localPosition.y - cell.LocalPosition.y) <= halfCellSize)
                    {
                        return cell;
                    }
                }
            }
            return null;
        }

        #endregion

        #region Piece Swap

        /// <summary>
        /// Swap two pieces (for swap-based games). Handles animation.
        /// </summary>
        public async UniTask SwapPiecesAsync(GridPiece piece1, GridPiece piece2, CancellationToken ct = default)
        {
            if (piece1 == null || piece2 == null) return;

            _isProcessing = true;
            PlaySound(config.audioConfig?.swapStartSound);

            var pos1 = piece1.RectTransform.anchoredPosition;
            var pos2 = piece2.RectTransform.anchoredPosition;

            // Animate swap
            await UniTask.WhenAll(
                AnimateMovePieceAsync(piece1, pos1, pos2, 0.3f, ct),
                AnimateMovePieceAsync(piece2, pos2, pos1, 0.3f, ct)
            );

            // Update grid data
            var cell1 = _cells[piece1.Col, piece1.Row];
            var cell2 = _cells[piece2.Col, piece2.Row];

            cell1.Piece = piece2;
            cell2.Piece = piece1;

            int tempCol = piece1.Col, tempRow = piece1.Row;
            piece1.SetGridPosition(piece2.Col, piece2.Row);
            piece2.SetGridPosition(tempCol, tempRow);
            piece1.Cell = cell2;
            piece2.Cell = cell1;

            OnPiecesSwapped?.Invoke(piece1, piece2);
            _isProcessing = false;
        }

        /// <summary>
        /// Swap pieces back (undo swap, for failed match)
        /// </summary>
        public async UniTask SwapBackAsync(GridPiece piece1, GridPiece piece2, CancellationToken ct = default)
        {
            PlaySound(config.audioConfig?.swapFailSound);
            await SwapPiecesAsync(piece1, piece2, ct);
        }

        #endregion

        #region Interaction Handlers

        /// <summary>
        /// Handle tap on a piece (called by game's input system)
        /// </summary>
        public void HandlePieceTap(GridPiece piece)
        {
            if (_isProcessing || piece == null || !piece.IsInteractable) return;

            var cell = GetCell(piece.Col, piece.Row);
            if (cell == null || !cell.SupportsInteraction(CellInteraction.Tappable)) return;

            PlaySound(config.audioConfig?.tapSound);

            if (config.HasInteraction(GridInteractionMode.Tap))
            {
                OnPieceTapped?.Invoke(piece);
            }

            if (config.HasInteraction(GridInteractionMode.MultiSelect))
            {
                HandleMultiSelect(piece);
            }

            if (config.HasInteraction(GridInteractionMode.Swap))
            {
                HandleSwapSelect(piece);
            }
        }

        private void HandleSwapSelect(GridPiece piece)
        {
            if (_selectedPiece == null)
            {
                _selectedPiece = piece;
                piece.OnSelected();
                OnPieceSelected?.Invoke(piece);
            }
            else if (_selectedPiece == piece)
            {
                _selectedPiece.OnDeselected();
                OnPieceDeselected?.Invoke(_selectedPiece);
                _selectedPiece = null;
            }
            else
            {
                // Check if adjacent
                var cell1 = GetCell(_selectedPiece.Col, _selectedPiece.Row);
                var cell2 = GetCell(piece.Col, piece.Row);

                if (cell1 != null && cell2 != null && cell1.IsAdjacentTo(cell2, config.allowDiagonals))
                {
                    var prev = _selectedPiece;
                    prev.OnDeselected();
                    _selectedPiece = null;
                    SwapPiecesAsync(prev, piece, this.GetCancellationTokenOnDestroy()).Forget();
                }
                else
                {
                    _selectedPiece.OnDeselected();
                    OnPieceDeselected?.Invoke(_selectedPiece);
                    _selectedPiece = piece;
                    piece.OnSelected();
                    OnPieceSelected?.Invoke(piece);
                }
            }
        }

        private void HandleMultiSelect(GridPiece piece)
        {
            if (_multiSelectedPieces.Contains(piece))
            {
                _multiSelectedPieces.Remove(piece);
                piece.OnDeselected();
                OnPieceDeselected?.Invoke(piece);
                PlaySound(config.audioConfig?.deselectSound);
            }
            else
            {
                _multiSelectedPieces.Add(piece);
                piece.OnSelected();
                OnPieceSelected?.Invoke(piece);
                PlaySound(config.audioConfig?.multiSelectSound);

                if (_multiSelectedPieces.Count >= config.multiSelectCount)
                {
                    PlaySound(config.audioConfig?.multiSelectCompleteSound);
                    var selected = new List<GridPiece>(_multiSelectedPieces);
                    ClearMultiSelection();
                    OnMultiSelectCompleted?.Invoke(selected);
                }
            }
        }

        /// <summary>
        /// Handle swipe on a piece (called by game's input system)
        /// </summary>
        public void HandlePieceSwipe(GridPiece piece, Vector2Int direction)
        {
            if (_isProcessing || piece == null || !piece.IsInteractable) return;
            if (!config.HasInteraction(GridInteractionMode.Swap)) return;

            var cell = GetCell(piece.Col, piece.Row);
            if (cell == null || !cell.SupportsInteraction(CellInteraction.Swipeable)) return;

            int targetCol = piece.Col + direction.x;
            int targetRow = piece.Row - direction.y; // Y is inverted in grid space

            var targetPiece = GetPiece(targetCol, targetRow);
            if (targetPiece == null || !targetPiece.IsInteractable) return;

            PlaySound(config.audioConfig?.swapStartSound);

            // Clear any selection
            if (_selectedPiece != null)
            {
                _selectedPiece.OnDeselected();
                _selectedPiece = null;
            }

            SwapPiecesAsync(piece, targetPiece, this.GetCancellationTokenOnDestroy()).Forget();
        }

        /// <summary>
        /// Start dragging a piece
        /// </summary>
        public void HandleDragStart(GridPiece piece, Vector2 position)
        {
            if (_isProcessing || piece == null || !piece.IsInteractable) return;
            if (!config.HasInteraction(GridInteractionMode.DragAndDrop)) return;

            var cell = GetCell(piece.Col, piece.Row);
            if (cell == null || !cell.SupportsInteraction(CellInteraction.Draggable)) return;

            _draggedPiece = piece;
            _dragStartPosition = piece.RectTransform.anchoredPosition;
            piece.OnDragStarted();
            PlaySound(config.audioConfig?.dragStartSound);

            if (config.visualConfig != null)
            {
                piece.SetScale(config.visualConfig.dragScale);
                piece.SetAlpha(config.visualConfig.dragAlpha);
            }
        }

        /// <summary>
        /// Update drag position
        /// </summary>
        public void HandleDragUpdate(Vector2 localPosition)
        {
            if (_draggedPiece == null) return;
            _draggedPiece.SetPosition(localPosition);
        }

        /// <summary>
        /// End drag - drop piece
        /// </summary>
        public void HandleDragEnd(Vector2 localPosition)
        {
            if (_draggedPiece == null) return;

            var targetCell = GetCellAtPosition(localPosition);
            var sourceCell = GetCell(_draggedPiece.Col, _draggedPiece.Row);

            _draggedPiece.OnDragEnded();
            _draggedPiece.SetScale(1f);
            _draggedPiece.SetAlpha(1f);

            if (targetCell != null && targetCell != sourceCell && targetCell.IsActive)
            {
                PlaySound(config.audioConfig?.dropSuccessSound);
                OnPieceDragDropped?.Invoke(_draggedPiece, sourceCell, targetCell);
            }
            else
            {
                // Return to original position
                PlaySound(config.audioConfig?.dropFailSound);
                _draggedPiece.SetPosition(_dragStartPosition);
            }

            _draggedPiece = null;
        }

        /// <summary>
        /// Add piece to chain (for Draw/Chain mode)
        /// </summary>
        public void HandleChainAdd(GridPiece piece)
        {
            if (_isProcessing || piece == null || !piece.IsInteractable) return;
            if (!config.HasInteraction(GridInteractionMode.Chain) &&
                !config.HasInteraction(GridInteractionMode.Draw)) return;

            var cell = GetCell(piece.Col, piece.Row);
            if (cell == null || !cell.SupportsInteraction(CellInteraction.Chainable)) return;

            // Check if this piece is already in the chain
            if (_chainedPieces.Contains(piece))
            {
                // If it's the second-to-last piece, allow backtracking
                if (_chainedPieces.Count >= 2 && _chainedPieces[_chainedPieces.Count - 2] == piece)
                {
                    var last = _chainedPieces[_chainedPieces.Count - 1];
                    last.OnDeselected();
                    _chainedPieces.RemoveAt(_chainedPieces.Count - 1);
                    PlaySound(config.audioConfig?.chainRemoveSound);
                }
                return;
            }

            // For Chain mode, check adjacency and matching
            if (_chainedPieces.Count > 0)
            {
                var lastPiece = _chainedPieces[_chainedPieces.Count - 1];
                var lastCell = GetCell(lastPiece.Col, lastPiece.Row);

                if (!cell.IsAdjacentTo(lastCell, config.allowDiagonals))
                    return;

                // For Chain mode, pieces must match
                if (config.HasInteraction(GridInteractionMode.Chain) && !piece.Matches(lastPiece))
                    return;
            }

            _chainedPieces.Add(piece);
            piece.OnSelected();
            piece.SetState(GridPieceState.Chaining);
            PlaySound(config.audioConfig?.chainAddSound);
        }

        /// <summary>
        /// Complete the chain (finger lifted)
        /// </summary>
        public void HandleChainEnd()
        {
            if (_chainedPieces.Count >= config.minChainLength)
            {
                PlaySound(config.audioConfig?.chainConfirmSound);
                var chained = new List<GridPiece>(_chainedPieces);
                ClearChain();
                OnChainCompleted?.Invoke(chained);
            }
            else
            {
                PlaySound(config.audioConfig?.chainFailSound);
                ClearChain();
            }
        }

        /// <summary>Clear all selections and interaction state</summary>
        public void ClearAllInteractionState()
        {
            ClearSelection();
            ClearMultiSelection();
            ClearChain();
            if (_draggedPiece != null)
            {
                _draggedPiece.OnDragEnded();
                _draggedPiece.SetScale(1f);
                _draggedPiece.SetAlpha(1f);
                _draggedPiece.SetPosition(_dragStartPosition);
                _draggedPiece = null;
            }
        }

        private void ClearSelection()
        {
            if (_selectedPiece != null)
            {
                _selectedPiece.OnDeselected();
                _selectedPiece = null;
            }
        }

        private void ClearMultiSelection()
        {
            foreach (var piece in _multiSelectedPieces)
                piece.OnDeselected();
            _multiSelectedPieces.Clear();
        }

        private void ClearChain()
        {
            foreach (var piece in _chainedPieces)
            {
                piece.OnDeselected();
                piece.SetState(GridPieceState.Idle);
            }
            _chainedPieces.Clear();
        }

        #endregion

        #region Animation

        private async UniTask AnimateSpawnAsync(List<GridPiece> pieces, CancellationToken ct)
        {
            var tasks = new List<UniTask>();
            float delay = 0f;

            foreach (var piece in pieces)
            {
                var cell = GetCell(piece.Col, piece.Row);
                if (cell == null) continue;

                Vector2 startPos = config.GetSpawnStartPosition(piece.Col, piece.Row);
                Vector2 endPos = cell.LocalPosition;

                piece.SetPosition(startPos);

                if (config.visualConfig != null)
                {
                    piece.SetScale(config.visualConfig.spawnStartScale);
                    piece.SetAlpha(config.visualConfig.spawnStartAlpha);
                }

                float capturedDelay = delay;
                tasks.Add(SpawnSinglePieceAsync(piece, startPos, endPos, capturedDelay, ct));
                delay += config.spawnStaggerDelay;
            }

            await UniTask.WhenAll(tasks);
        }

        private async UniTask SpawnSinglePieceAsync(GridPiece piece, Vector2 from, Vector2 to, float delay, CancellationToken ct)
        {
            if (delay > 0f)
                await UniTask.WaitForSeconds(delay, cancellationToken: ct);

            piece.SetState(GridPieceState.Spawning);
            float duration = config.spawnAnimationDuration;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                if (config.useSpawnEasing)
                    t = EaseOutBack(t);

                piece.SetPosition(Vector2.Lerp(from, to, t));
                piece.SetScale(Mathf.Lerp(config.visualConfig?.spawnStartScale ?? 0f, 1f, t));
                piece.SetAlpha(Mathf.Lerp(config.visualConfig?.spawnStartAlpha ?? 0f, 1f, t));

                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            piece.SetPosition(to);
            piece.SetScale(1f);
            piece.SetAlpha(1f);
            piece.SetState(GridPieceState.Idle);
        }

        private async UniTask AnimateDestroyAsync(List<GridPiece> pieces, CancellationToken ct)
        {
            float duration = 0.2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                foreach (var piece in pieces)
                {
                    if (piece == null) continue;
                    piece.SetScale(Mathf.Lerp(1f, 0f, t));
                    piece.SetAlpha(Mathf.Lerp(1f, 0f, t));
                }

                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
        }

        private async UniTask AnimateMovePieceAsync(GridPiece piece, Vector2 from, Vector2 to, float duration, CancellationToken ct)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (piece == null) return;
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = EaseOutQuad(t);
                piece.SetPosition(Vector2.Lerp(from, to, t));
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            if (piece != null)
                piece.SetPosition(to);
        }

        // Easing functions
        private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        #endregion

        #region Audio

        private void PlaySound(AudioClip clip)
        {
            if (clip == null || !AudioManager.HasInstance) return;

            float interval = config.audioConfig?.minSoundInterval ?? 0f;
            if (Time.time - _lastSoundTime < interval) return;

            float pitchVar = config.audioConfig?.pitchVariation ?? 0f;
            if (pitchVar > 0f)
            {
                // Use PlayOneShot for now; pitch variation would need custom implementation
                AudioManager.Instance.PlaySFX(clip);
            }
            else
            {
                AudioManager.Instance.PlaySFX(clip);
            }
            _lastSoundTime = Time.time;
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Clear the entire grid
        /// </summary>
        public void ClearGrid()
        {
            ClearAllInteractionState();

            if (_cells != null)
            {
                for (int col = 0; col < Columns; col++)
                {
                    for (int row = 0; row < Rows; row++)
                    {
                        var cell = _cells[col, row];
                        if (cell?.Piece != null)
                        {
                            _piecePool?.Release(cell.Piece);
                            cell.Piece = null;
                        }
                    }
                }
            }

            _isInitialized = false;
        }

        private void OnDestroy()
        {
            ClearGrid();
            _piecePool?.Clear();
        }

        #endregion
    }
}
