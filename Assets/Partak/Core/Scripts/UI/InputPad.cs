﻿using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using GeoTetra.GTCommon.Components;
using GeoTetra.GTPooling;
using UnityEngine.EventSystems;

namespace GeoTetra.Partak.UI
{
    public class InputPad : SubscribableBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler,
        IEndDragHandler, IInitializePotentialDragHandler
    {
        [SerializeField] 
        private ComponentContainerRef _componentContainer;
        
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private CanvasGroup _overlayCanvasGroup;
        [SerializeField] private int _playerIndex;
        
        private CursorStore _cursorStore;
        private Vector3 _hitPosition;
        private bool _visible = true;
        private Vector3 _priorPosition;
        private Vector3 _deltaPosition;
        private bool _dragging;
        private int _particleLayer = 1 << 8;
        
        private void Awake()
        {
            _lineRenderer.positionCount = 2;
            _lineRenderer.enabled = false;
        }

        private void OnValidate()
        {
            _lineRenderer = GetComponentInChildren<LineRenderer>();
            _overlayCanvasGroup = GetComponentInChildren<CanvasGroup>();
        }

        public async void Initialize()
        {
            await _componentContainer.Cache(this);
            _cursorStore = await _componentContainer.AwaitRegister<CursorStore>();
            _lineRenderer.enabled = false;
            _visible = true;
            _overlayCanvasGroup.alpha = 1;
            _lineRenderer.SetPosition(0, Vector3.zero);
            _lineRenderer.SetPosition(1, Vector3.zero);
        }

        public void Visibility(bool state)
        {
            enabled = state;
            _lineRenderer.enabled = state;
            _overlayCanvasGroup.alpha = state ? 1 : 0;
        }

        private void LateUpdate()
        {
            if (_cursorStore == null) return;

            _cursorStore.SetCursorDeltaPositionClamp(_playerIndex, _deltaPosition);

            if (_dragging)
                _deltaPosition = Vector3.zero;
            else
                _deltaPosition = Vector3.Lerp(_deltaPosition, Vector3.zero, .1f);

            _lineRenderer.SetPosition(0, _cursorStore.CursorPositions[_playerIndex]);
            _lineRenderer.SetPosition(1, _hitPosition);
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _dragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerCurrentRaycast.gameObject == eventData.pointerPressRaycast.gameObject)
                CalculateDeltaPosition(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.pointerCurrentRaycast.gameObject == eventData.pointerPressRaycast.gameObject)
                CalculateDeltaPosition(eventData.position);
            _dragging = false;
        }

        private void CalculateDeltaPosition(Vector2 inputPosition)
        {
            _hitPosition = InputParticleLayerHit(inputPosition);
            _deltaPosition = _hitPosition - _priorPosition;
            _priorPosition = _hitPosition;
        }

        private Vector3 InputParticleLayerHit(Vector2 inputPosition)
        {
            Ray ray = Camera.main.ScreenPointToRay(inputPosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 200f, _particleLayer))
            {
                return hit.point;
            }

            return Vector3.zero;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (enabled)
            {
                _lineRenderer.enabled = true;
                _priorPosition = InputParticleLayerHit(eventData.position);
                _hitPosition = _priorPosition;
                _deltaPosition = Vector3.zero;
                FadeOut();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _lineRenderer.enabled = false;
        }

        private void FadeOut()
        {
            if (_visible && enabled)
            {
                StartCoroutine(FadeOutCoroutine());
            }
        }

        private IEnumerator FadeOutCoroutine()
        {
            _visible = false;
            float alpha = 1f;
            while (alpha > 0f)
            {
                alpha -= Time.deltaTime * 2f;
                _overlayCanvasGroup.alpha = alpha;
                yield return null;
            }

            _overlayCanvasGroup.alpha = 0f;
        }
    }
}