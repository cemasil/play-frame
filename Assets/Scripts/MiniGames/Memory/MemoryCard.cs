using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MiniGameFramework.MiniGames.Common;

namespace MiniGameFramework.MiniGames.Memory
{
    /// <summary>
    /// Represents a single memory card with flip animations and click handling
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class MemoryCard : BaseGame, IPointerClickHandler
    {
        [SerializeField] private GameObject cardFront;
        [SerializeField] private GameObject cardBack;

        public int CardId { get; private set; }
        public bool IsRevealed { get; private set; }
        public bool IsMatched { get; private set; }

        private Action<MemoryCard> onCardClicked;
        private Button button;

        protected override void OnInitialize()
        {
            button = GetComponent<Button>();
        }

        public void Initialize(int cardId, Color cardColor, Action<MemoryCard> clickCallback)
        {
            CardId = cardId;
            onCardClicked = clickCallback;

            if (cardFront != null)
            {
                Image frontImage = cardFront.GetComponent<Image>();
                if (frontImage != null)
                {
                    frontImage.color = cardColor;
                }
            }

            Hide();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsRevealed && !IsMatched)
            {
                onCardClicked?.Invoke(this);
            }
        }

        public void Reveal()
        {
            IsRevealed = true;
            if (cardFront != null) cardFront.SetActive(true);
            if (cardBack != null) cardBack.SetActive(false);
        }

        public void Hide()
        {
            IsRevealed = false;
            if (cardFront != null) cardFront.SetActive(false);
            if (cardBack != null) cardBack.SetActive(true);
        }

        public void SetMatched()
        {
            IsMatched = true;
            IsRevealed = true;
            if (button != null) button.interactable = false;
        }

        public void SetInteractable(bool interactable)
        {
            if (button != null && !IsMatched)
            {
                button.interactable = interactable;
            }
        }
    }
}
