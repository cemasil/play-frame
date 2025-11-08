using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MiniGameFramework.MiniGames.Common;

namespace MiniGameFramework.MiniGames.Match3
{
    /// <summary>
    /// Represents a single gem on the Match3 grid
    /// </summary>
    public class Gem : BaseGame, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int ColorIndex { get; set; }

        private Image image;
        private Vector2 dragStartPos;
        private bool isDragging = false;
        private const float DRAG_THRESHOLD = 30f;

        private System.Action<Gem, Vector2Int> onGemSwipe;

        protected override void OnInitialize()
        {
            image = GetComponent<Image>();
        }

        public void OnSwipe(System.Action<Gem, Vector2Int> swipeCallback)
        {
            onGemSwipe = swipeCallback;
        }

        public void SetColor(Color color, int colorIndex)
        {
            if (image != null)
            {
                image.color = color;
                ColorIndex = colorIndex;
            }
        }

        public void SetPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            dragStartPos = eventData.position;
            isDragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            Vector2 dragDelta = eventData.position - dragStartPos;

            if (dragDelta.magnitude > DRAG_THRESHOLD)
            {
                Vector2Int direction = Vector2Int.zero;

                if (Mathf.Abs(dragDelta.x) > Mathf.Abs(dragDelta.y))
                {
                    direction = dragDelta.x > 0 ? Vector2Int.right : Vector2Int.left;
                }
                else
                {
                    direction = dragDelta.y > 0 ? Vector2Int.down : Vector2Int.up;
                }

                onGemSwipe?.Invoke(this, direction);
                isDragging = false;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isDragging = false;
        }
    }
}
