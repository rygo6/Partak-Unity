using System;
using System.Collections;
using GeoTetra.GTCommon.Utility;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GeoTetra.GTSnapper
{
    public class ItemMoveGizmo : ItemGizmo, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private float _snapIncrement = 45;

        private Vector3 _startPosition;
        private Vector3 _priorPosition;

        public void OnBeginDrag(PointerEventData eventData)
        {
            _startPosition = eventData.pointerCurrentRaycast.worldPosition;
            _priorPosition = _startPosition;
            _plane = new Plane(Vector3.up, eventData.pointerCurrentRaycast.worldPosition);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector3 worldPosition = RaycastPlane(_plane, eventData);
            Vector3 deltaPosition = worldPosition - _priorPosition;
            _priorPosition = worldPosition;
            _parentItemGizmoRoot.Translate(deltaPosition.x, deltaPosition.y, deltaPosition.z);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
        }
    }
}