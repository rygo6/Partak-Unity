using UnityEngine;
using UnityEngine.EventSystems;

namespace GeoTetra.GTSnapper
{
    public class ItemMoveGizmo : ItemGizmo, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private Vector3 _startPosition;
        private Vector3 _priorPosition;

        public override void OnPointerDown(PointerEventData eventData)
        {
            _parentItemGizmoRoot.TargetedItem.OnPointerDown(eventData);
            base.OnPointerDown(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            Debug.Log("OnPointerUp");
            _parentItemGizmoRoot.TargetedItem.OnPointerUp(eventData);
            base.OnPointerUp(eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // _parentItemGizmoRoot.gameObject.se
            // _item.SetLayerRecursive(_item.ItemRoot.IgnoreLayer);
            _parentItemGizmoRoot.TargetedItem.Drag.OnBeginDrag(eventData);
            // _startPosition = eventData.pointerCurrentRaycast.worldPosition;
            // _priorPosition = _startPosition;
            // _plane = new Plane(Vector3.up, eventData.pointerCurrentRaycast.worldPosition);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Debug.Log("OnDrag");
            _parentItemGizmoRoot.TargetedItem.Drag.OnDrag(eventData);
            // Vector3 worldPosition = RaycastPlane(_plane, eventData);
            // Vector3 deltaPosition = worldPosition - _priorPosition;
            // _priorPosition = worldPosition;
            // _parentItemGizmoRoot.Translate(deltaPosition.x, deltaPosition.y, deltaPosition.z);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log("OnEndDrag");
            _parentItemGizmoRoot.TargetedItem.Drag.OnEndDrag(eventData);
        }
    }
}