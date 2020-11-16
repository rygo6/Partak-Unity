using UnityEngine;
using UnityEngine.UI;

namespace GeoTetra.GTUI
{
    public class BaseUI : MonoBehaviour
    {
        [SerializeField] protected Selectable _focusSelectable;
        [SerializeField] private RectTransform _transitionRoot;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private CanvasGroup _group;

        public RectTransform TransitionRoot => _transitionRoot;

        public Canvas Canvas => _canvas;

        public CanvasGroup Group => _group;

        public UIRenderer CurrentlyRenderedBy { get; private set; }

        protected virtual void Reset()
        {
            _transitionRoot = transform.Find("Root") as RectTransform;
            _canvas = GetComponentInChildren<Canvas>();
            _group = GetComponentInChildren<CanvasGroup>();
        }

        protected virtual void Awake()
        {
            gameObject.SetActive(false);
        }

        public virtual void OnTransitionInStart(UIRenderer uiRenderer)
        {
            if (CurrentlyRenderedBy == null)
            {
                CurrentlyRenderedBy = uiRenderer;
                Canvas.worldCamera = uiRenderer.UICamera;
                Canvas.renderMode = RenderMode.ScreenSpaceCamera;
            }

            Group.blocksRaycasts = false;
            gameObject.SetActive(true);
        }

        public virtual void OnTransitionInFinish()
        {
            if (_focusSelectable != null) _focusSelectable.Select();
            Group.blocksRaycasts = true;
        }
        
        public virtual void OnTransitionOutStart()
        {
            Group.blocksRaycasts = false;
        }

        public virtual void OnTransitionOutFinish()
        {
            gameObject.SetActive(false);
        }
    }
}