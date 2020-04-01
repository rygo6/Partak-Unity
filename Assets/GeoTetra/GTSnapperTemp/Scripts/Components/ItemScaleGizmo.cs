using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GeoTetra.GTSnapper
{
    public class ItemScaleGizmo : ItemGizmo, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private float _scaleMultiplier = .5f;
        private float _snapIncrement = .5f;
        
        private float _startDistance;
        private float _priorDistance;

        public void OnBeginDrag(PointerEventData eventData)
        {
            float distance = Vector3.Distance(_parentItemGizmoRoot.transform.position, eventData.pointerCurrentRaycast.worldPosition);
            _startDistance = SnapDistance(distance);
            _priorDistance = _startDistance;
        }

        public void OnDrag(PointerEventData eventData)
        {
            float distance = Vector3.Distance(_parentItemGizmoRoot.transform.position, eventData.pointerCurrentRaycast.worldPosition);
            float snappedDistance = SnapDistance(distance);
            float deltaDistance = snappedDistance - _priorDistance;
            _priorDistance = snappedDistance;
            _parentItemGizmoRoot.Scale(deltaDistance * _scaleMultiplier);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
        }
        
        private float SnapDistance(float distance)
        {
            return Mathf.Round(distance / _snapIncrement) * _snapIncrement;
        }
    }
}