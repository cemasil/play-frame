using UnityEngine;
using PlayFrame.Core;

namespace PlayFrame.UI.Base
{
    /// <summary>
    /// Base class for all UI panels
    /// </summary>
    public abstract class UIPanel : MonoBehaviour, IInitializable, IUpdatable
    {
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected bool hideOnAwake = false;

        protected bool isVisible = false;

        public bool IsVisible => isVisible;

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            Initialize();

            if (hideOnAwake)
            {
                Hide();
            }
        }

        public virtual void Initialize()
        {
            OnInitialize();
        }
        public void Update()
        {
            OnUpdate();
        }
        protected virtual void OnInitialize() { }
        protected virtual void OnUpdate() { }
        public virtual void Show()
        {
            gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            isVisible = true;

            OnShow();
        }
        public virtual void Hide()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            isVisible = false;

            OnHide();
        }
        protected virtual void OnShow() { }
        protected virtual void OnHide() { }
        protected virtual void OnCleanup() { }
        public virtual void UpdatePanel() { }
        protected virtual void OnDestroy()
        {
            OnCleanup();
        }
    }
}
