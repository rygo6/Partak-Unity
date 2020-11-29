using System;
using System.Collections;
using GeoTetra.GTCommon.Utility;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GeoTetra.GTSnapper
{
    public class ItemRotateGizmo : ItemGizmo, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private float _snapIncrement = 45;

        private float _startAngle;
        private float _priorAngle;

        public void OnBeginDrag(PointerEventData eventData)
        {
            float angle = Vector3Utility.AngleAroundLocalAxis(_parentItemGizmoRoot.transform, eventData.pointerCurrentRaycast.worldPosition);
            _startAngle = SnapAngle(angle);
            _priorAngle = _startAngle;
            _plane = new Plane(Vector3.up, eventData.pointerCurrentRaycast.worldPosition);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector3 worldPosition = RaycastPlane(_plane, eventData);
            
            float angle = Vector3Utility.AngleAroundLocalAxis(_parentItemGizmoRoot.transform, worldPosition);
            float snappedAngle = SnapAngle(angle);
            float deltaAngle = snappedAngle - _priorAngle;
            _priorAngle = snappedAngle;
            _parentItemGizmoRoot.Rotate(0, deltaAngle, 0);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
        }

        private float SnapAngle(float angle)
        {
            return Mathf.Round(angle / _snapIncrement) * _snapIncrement;
        }
    }
}