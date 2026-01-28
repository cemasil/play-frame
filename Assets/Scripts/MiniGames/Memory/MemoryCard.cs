using System;
using UnityEngine;
using UnityEngine.UI;
using PlayFrame.MiniGames.Common;
using PlayFrame.Systems.Input;

namespace PlayFrame.MiniGames.Memory
{
    /// <summary>
    /// Represents a single memory card with flip animations and tap handling.
    /// Uses TapHandler for input abstraction.
    /// </summary>
    [RequireComponent(typeof(TapHandler))]
    public class MemoryCard : BaseGamePiece, ITappable
    {
        [SerializeField] private GameObject cardFront;
        [SerializeField] private GameObject cardBack;

        public int CardId { get; private set; }
        public bool IsRevealed { get; private set; }
        public bool IsMatched { get; private set; }

        private Action<MemoryCard> _onCardTapped;
        private TapHandler _tapHandler;
        private Button _button;

        protected override void OnInitialize()
        {
            _button = GetComponent<Button>();
            _tapHandler = GetComponent<TapHandler>();

            if (_tapHandler == null)
            {
                _tapHandler = gameObject.AddComponent<TapHandler>();
            }
        }

        public void Setup(int cardId, Color cardColor, Action<MemoryCard> tapCallback)
        {
            CardId = cardId;
            _onCardTapped = tapCallback;

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

        /// <summary>
        /// Register callback for tap events
        /// </summary>
        public void OnTapCallback(Action<MemoryCard> tapCallback)
        {
            _onCardTapped = tapCallback;
        }

        /// <summary>
        /// Called by ITappable interface when tap is detected
        /// </summary>
        public void OnTap()
        {
            if (!IsRevealed && !IsMatched)
            {
                _onCardTapped?.Invoke(this);
            }
        }

        public override void SetVisualState(PieceVisualState state)
        {
            switch (state)
            {
                case PieceVisualState.Matched:
                    SetMatched();
                    break;
                case PieceVisualState.Normal:
                    Hide();
                    break;
                case PieceVisualState.Disabled:
                    SetInteractable(false);
                    break;
            }
        }

        public override void ResetPiece()
        {
            IsRevealed = false;
            IsMatched = false;
            _onCardTapped = null;
            if (_button != null) _button.interactable = true;
            Hide();
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
            if (_button != null) _button.interactable = false;
        }

        public void SetInteractable(bool interactable)
        {
            if (_button != null && !IsMatched)
            {
                _button.interactable = interactable;
            }
        }

        protected override void OnCleanup()
        {
            _onCardTapped = null;
        }
    }
}
