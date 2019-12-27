using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace GeoTetra
{
	public class InputCatcher : MonoBehaviour,
							IPointerEnterHandler, 
							IPointerExitHandler, 
							IBeginDragHandler, 
							IDragHandler, 
							IEndDragHandler, 
							IPointerClickHandler, 
							IPointerDownHandler,
							IPointerUpHandler
	{
		[SerializeField] private Camera _parentCamera;

		public System.Action EmptyClickAction
		{
			get
			{
				return _emptyClickAction;
			}
			set
			{
				_emptyClickAction = value;
			}
		}

		public readonly List<PointerEventData> PointerEventDataList = new List<PointerEventData>();
		private Action _emptyClickAction;

		public void OnPointerExit(PointerEventData data)
		{

		}

		public void OnPointerEnter(PointerEventData data)
		{

		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			int index = PointerEventDataList.FindIndex(e => e.pointerId == eventData.pointerId);
			PointerEventDataList[index] = eventData;
		}

		public void OnDrag(PointerEventData eventData)
		{
			int index = PointerEventDataList.FindIndex(e => e.pointerId == eventData.pointerId);
			PointerEventDataList[index] = eventData;
		}

		public void OnEndDrag(PointerEventData eventData)
		{

		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (EmptyClickAction != null)
			{
				EmptyClickAction();
				EmptyClickAction = null;
			}
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			PointerEventDataList.Add(eventData);

			if (PointerEventDataList.Count == 1)
			{
//				_parentCamera.depth = 100;
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			int index = PointerEventDataList.FindIndex(e => e.pointerId == eventData.pointerId);
			PointerEventDataList.RemoveAt(index);

			if (PointerEventDataList.Count == 0)
			{
//				_parentCamera.depth = 1;
			}
		}
	}
}