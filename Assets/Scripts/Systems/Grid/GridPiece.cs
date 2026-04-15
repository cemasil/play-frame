using System;
using UnityEngine;
using UnityEngine.UI;

namespace PlayFrame.Systems.Grid
{
    /// <summary>
    /// Base class for all pieces that can be placed on the grid.
    /// Games should derive from this to create their specific piece types (gems, cards, tiles, etc.)
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class GridPiece : MonoBehaviour
    {
        [Header("Piece Identity")]
        [SerializeField] private string pieceType = "";
        [SerializeField] private int pieceId = -1;

        private RectTransform _rectTransform;
        private Image _image;
        private CanvasGroup _canvasGroup;

        /// <summary>Piece type identifier (e.g., "red", "blue", "star")</summary>
        public string PieceType
        {
            get => pieceType;
            set => pieceType = value;
        }

        /// <summary>Numeric piece identifier</summary>
        public int PieceId
        {
            get => pieceId;
            set => pieceId = value;
        }

        /// <summary>Current grid column</summary>
        public int Col { get; set; }

        /// <summary>Current grid row</summary>
        public int Row { get; set; }

        /// <summary>Current visual state</summary>
        public GridPieceState State { get; private set; } = GridPieceState.Idle;

        /// <summary>The cell this piece occupies</summary>
        public GridCell Cell { get; set; }

        /// <summary>Whether this piece can be interacted with</summary>
        public bool IsInteractable { get; set; } = true;

        /// <summary>RectTransform for UI positioning</summary>
        public RectTransform RectTransform => _rectTransform;

        /// <summary>Image component for visual changes</summary>
        public Image Image => _image;

        /// <summary>Custom data for game-specific logic</summary>
        public object CustomData { get; set; }

        /// <summary>Event fired when piece state changes</summary>
        public event Action<GridPiece, GridPieceState> OnStateChanged;

        protected virtual void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _image = GetComponent<Image>();
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        /// <summary>Set piece grid position</summary>
        public void SetGridPosition(int col, int row)
        {
            Col = col;
            Row = row;
        }

        /// <summary>Get grid coordinate</summary>
        public Vector2Int GridCoordinate => new Vector2Int(Col, Row);

        /// <summary>Set the visual state of this piece</summary>
        public void SetState(GridPieceState newState)
        {
            if (State == newState) return;
            var oldState = State;
            State = newState;

            ApplyVisualState(newState);
            OnStateChanged?.Invoke(this, newState);
            OnVisualStateChanged(oldState, newState);
        }

        /// <summary>Override to handle state changes with custom visuals</summary>
        protected virtual void OnVisualStateChanged(GridPieceState oldState, GridPieceState newState) { }

        private void ApplyVisualState(GridPieceState state)
        {
            // Base visual changes - games can override OnVisualStateChanged for custom behavior
            switch (state)
            {
                case GridPieceState.Idle:
                    SetScale(1f);
                    SetAlpha(1f);
                    break;
                case GridPieceState.Disabled:
                    SetAlpha(0.5f);
                    break;
            }
        }

        /// <summary>Set piece sprite</summary>
        public void SetSprite(Sprite sprite)
        {
            if (_image != null)
                _image.sprite = sprite;
        }

        /// <summary>Set piece color</summary>
        public void SetColor(Color color)
        {
            if (_image != null)
                _image.color = color;
        }

        /// <summary>Set piece scale</summary>
        public void SetScale(float scale)
        {
            if (_rectTransform != null)
                _rectTransform.localScale = Vector3.one * scale;
        }

        /// <summary>Set piece alpha</summary>
        public void SetAlpha(float alpha)
        {
            if (_canvasGroup != null)
                _canvasGroup.alpha = alpha;
        }

        /// <summary>Set anchored position</summary>
        public void SetPosition(Vector2 position)
        {
            if (_rectTransform != null)
                _rectTransform.anchoredPosition = position;
        }

        /// <summary>Reset piece to default state</summary>
        public virtual void ResetPiece()
        {
            State = GridPieceState.Idle;
            IsInteractable = true;
            Cell = null;
            CustomData = null;
            SetScale(1f);
            SetAlpha(1f);
        }

        /// <summary>Check if this piece matches another (same type)</summary>
        public virtual bool Matches(GridPiece other)
        {
            if (other == null) return false;
            return PieceId == other.PieceId;
        }

        /// <summary>Called when piece is selected</summary>
        public virtual void OnSelected() { SetState(GridPieceState.Selected); }

        /// <summary>Called when piece is deselected</summary>
        public virtual void OnDeselected() { SetState(GridPieceState.Idle); }

        /// <summary>Called when piece starts being dragged</summary>
        public virtual void OnDragStarted() { SetState(GridPieceState.Dragging); }

        /// <summary>Called when piece stops being dragged</summary>
        public virtual void OnDragEnded() { SetState(GridPieceState.Idle); }

        /// <summary>Called when piece is matched</summary>
        public virtual void OnMatched() { SetState(GridPieceState.Matched); }

        /// <summary>Called when piece is about to be destroyed</summary>
        public virtual void OnBeforeDestroy() { SetState(GridPieceState.Destroying); }
    }
}
