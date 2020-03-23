using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GeoTetra.GTSnapper
{
    public class ItemGizmo : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private MeshRenderer _meshRenderer;
        
        protected ItemGizmoRoot _parentItemGizmoRoot;
        private Color _originalColor;

        private void Awake()
        {
            _originalColor = _meshRenderer.sharedMaterial.color;
        }

        private void OnDestroy()
        {
            _meshRenderer.sharedMaterial.color = _originalColor;
        }

        private void OnValidate()
        {
            if (_meshRenderer == null) _meshRenderer = GetComponent<MeshRenderer>();
        }
        
        public void Initialize(ItemGizmoRoot parentItemGizmoRoot)
        {
            _parentItemGizmoRoot = parentItemGizmoRoot;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _meshRenderer.sharedMaterial.color = _parentItemGizmoRoot.HighlightColor;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _meshRenderer.sharedMaterial.color = _originalColor;
        }
    }
}