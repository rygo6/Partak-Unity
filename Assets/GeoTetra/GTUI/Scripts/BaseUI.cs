using GeoTetra.GTCommon.Components;
using UnityEngine;
using UnityEngine.UI;

namespace GeoTetra.GTUI
{
    public class BaseUI : SubscribableBehaviour
    {
        [SerializeField] protected Selectable _focusSelectable;
        [SerializeField] private RectTransform _transitionRoot;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private CanvasGroup _group;

        public RectTransform TransitionRoot => _transitionRoot;

        public Canvas Canvas => _canvas;

        public CanvasGroup Group => _group;

        public UIRenderer CurrentlyRenderedBy { get; private set; }
        
        protected virtual void Awake()
        {
            // You disable the transition root, rather than the whole GameObject, in case
            // there is async await logic that must finish.
            _transitionRoot.gameObject.SetActive(false);
        }

        protected virtual void Reset()
        {
            _transitionRoot = transform.Find("Root") as RectTransform;
            _canvas = GetComponentInChildren<Canvas>();
            _group = GetComponentInChildren<CanvasGroup>();
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
            _transitionRoot.gameObject.SetActive(true);
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