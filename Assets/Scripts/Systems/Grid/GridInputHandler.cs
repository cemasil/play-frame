using UnityEngine;
using UnityEngine.EventSystems;

namespace PlayFrame.Systems.Grid
{
    /// <summary>
    /// Bridges Unity's EventSystem input to GridManager interaction methods.
    /// Attach to the same GameObject as GridManager (or any child with a RectTransform + Image).
    /// Automatically adds a transparent Image if none exists so the EventSystem can raycast to it.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(UnityEngine.UI.Image))]
    public class GridInputHandler : MonoBehaviour,
        IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [Header("References")]
        [SerializeField] private GridManager gridManager;

        [Header("Settings")]
        [SerializeField] private float swipeThreshold = 30f;
        [SerializeField] private float dragStartThreshold = 15f;

        private Camera _uiCamera;
        private RectTransform _rectTransform;
        private Vector2 _pointerDownPosition;
        private Vector2 _pointerDownLocalPosition;
        private GridPiece _pointerDownPiece;
        private bool _isDragging;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();

            if (gridManager == null)
                gridManager = GetComponent<GridManager>();

            if (gridManager == null)
                gridManager = GetComponentInParent<GridManager>();

            // Ensure the image is transparent and receives raycasts
            var img = GetComponent<UnityEngine.UI.Image>();
            if (img != null)
            {
                img.color = new Color(0, 0, 0, 0);
                img.raycastTarget = true;
            }

            // Get camera for Screen Space - Camera canvases; null for Overlay
            var canvas = GetComponentInParent<UnityEngine.Canvas>();
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                _uiCamera = canvas.worldCamera;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (gridManager == null || !gridManager.IsInitialized || gridManager.IsProcessing)
                return;

            _pointerDownPosition = eventData.position;
            _isDragging = false;

            if (TryGetLocalPosition(eventData, out var localPos))
            {
                _pointerDownLocalPosition = localPos;
                var cell = gridManager.GetCellAtPosition(localPos);
                _pointerDownPiece = cell?.Piece;

                // Chain/Draw: add first piece on touch start
                if (_pointerDownPiece != null)
                {
                    var config = gridManager.Config;
                    if (config.HasInteraction(GridInteractionMode.Chain) ||
                        config.HasInteraction(GridInteractionMode.Draw))
                    {
                        gridManager.HandleChainAdd(_pointerDownPiece);
                    }
                }
            }
            else
            {
                _pointerDownPiece = null;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (gridManager == null || _pointerDownPiece == null)
                return;

            if (!TryGetLocalPosition(eventData, out var localPos))
                return;

            var config = gridManager.Config;

            // Drag-and-drop mode
            if (config.HasInteraction(GridInteractionMode.DragAndDrop))
            {
                float dist = Vector2.Distance(eventData.position, _pointerDownPosition);
                if (!_isDragging && dist >= dragStartThreshold)
                {
                    _isDragging = true;
                    gridManager.HandleDragStart(_pointerDownPiece, _pointerDownLocalPosition);
                }

                if (_isDragging)
                {
                    gridManager.HandleDragUpdate(localPos);
                }
            }

            // Chain / Draw mode
            if (config.HasInteraction(GridInteractionMode.Chain) ||
                config.HasInteraction(GridInteractionMode.Draw))
            {
                var cell = gridManager.GetCellAtPosition(localPos);
                if (cell?.Piece != null)
                {
                    gridManager.HandleChainAdd(cell.Piece);
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (gridManager == null)
                return;

            if (!TryGetLocalPosition(eventData, out var localPos))
            {
                ResetState();
                return;
            }

            var config = gridManager.Config;

            // End drag
            if (_isDragging && config.HasInteraction(GridInteractionMode.DragAndDrop))
            {
                gridManager.HandleDragEnd(localPos);
                ResetState();
                return;
            }

            // End chain
            if (config.HasInteraction(GridInteractionMode.Chain) ||
                config.HasInteraction(GridInteractionMode.Draw))
            {
                gridManager.HandleChainEnd();
            }

            ResetState();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (gridManager == null || !gridManager.IsInitialized || gridManager.IsProcessing)
                return;

            // Skip if this was a drag
            if (_isDragging) return;

            if (!TryGetLocalPosition(eventData, out var localPos))
                return;

            var config = gridManager.Config;
            float dist = Vector2.Distance(eventData.position, _pointerDownPosition);

            // Detect swipe on click end
            if (dist >= swipeThreshold &&
                config.HasInteraction(GridInteractionMode.Swap) &&
                _pointerDownPiece != null)
            {
                Vector2 delta = eventData.position - _pointerDownPosition;
                Vector2Int dir;
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    dir = delta.x > 0 ? Vector2Int.right : Vector2Int.left;
                else
                    dir = delta.y > 0 ? Vector2Int.up : Vector2Int.down;

                gridManager.HandlePieceSwipe(_pointerDownPiece, dir);
                return;
            }

            // Tap
            var cell = gridManager.GetCellAtPosition(localPos);
            if (cell?.Piece != null)
            {
                gridManager.HandlePieceTap(cell.Piece);
            }
        }

        private void ResetState()
        {
            _pointerDownPiece = null;
            _isDragging = false;
        }

        private bool TryGetLocalPosition(PointerEventData eventData, out Vector2 localPos)
        {
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rectTransform, eventData.position, _uiCamera, out localPos);
        }
    }
}
