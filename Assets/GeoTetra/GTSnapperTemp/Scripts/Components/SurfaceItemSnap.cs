using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace GeoTetra.GTSnapper
{
    public class SurfaceItemSnap : ItemSnap
    {
        [SerializeField] private int _allowedChildren = -1;
        [SerializeField] private Collider _collider;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(this.transform.position, .01f);
        }

        public override Ray Snap(Item item, PointerEventData data)
        {
            Vector3 attachPoint = GetAttachPointOffset(data.pointerCurrentRaycast.worldPosition, item);
            Vector3 closestPoint = _collider.ClosestPoint(attachPoint);
            
            Ray ray = new Ray(closestPoint, (_collider.bounds.center - closestPoint).normalized);
            ray.origin = ray.GetPoint(-.1f); //move back a little so raycast works
            
            Debug.DrawRay(ray.origin, ray.direction, Color.cyan);

            if (_collider.Raycast(ray, out RaycastHit hit, 100.0f))
            {
                return new Ray(hit.point, hit.normal);
            }

            Debug.DrawRay(closestPoint, Vector3.up * 100, Color.red);
            Debug.Log("Ray didn't hit!'");
            return new Ray(closestPoint, Vector3.one);
        }

        public override Vector3 NearestPoint(PointerEventData data)
        {
            return _collider.ClosestPoint(data.pointerCurrentRaycast.worldPosition);
        }

        public override bool AvailableSnap()
        {
            return _allowedChildren <= 0 || ChildItemList.Count < _allowedChildren;
        }
    }
}