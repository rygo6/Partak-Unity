using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace GeoTetra.GTSnapper
{
	public class PointItemSnap : ItemSnap
	{
		private void OnDrawGizmos()
		{	
			Gizmos.color = Color.magenta;
			Gizmos.DrawSphere(this.transform.position, .01f);
		}

		public override Ray Snap(Item item, PointerEventData data)
		{
			return new Ray(GetAttachPointOffset(transform.position, item), -transform.up);
		}

		public override Vector3 NearestPoint(PointerEventData data)
		{
			return transform.position;
		}

		public override void AddItem(Item item)
		{
			ChildItemList.Add(item);
		}

		public override void RemoveItem(Item item)
		{
			ChildItemList.Remove(item);
		}

		public override bool ContainsItem(Item item)
		{
			return ChildItemList.Contains(item);
		}

		public override bool AvailableSnap()
		{
			return ChildItemList.Count == 0;
		}
	}
}
