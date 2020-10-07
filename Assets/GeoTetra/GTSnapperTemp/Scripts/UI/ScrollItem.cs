using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using GeoTetra.GTSnapper.ScriptableObjects;

namespace GeoTetra.GTSnapper
{
	public class ScrollItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerUpHandler, IPointerDownHandler, IPointerClickHandler
	{
		[SerializeField] private float _dragThreshold = 40;
		
		private ItemCatalogUI _parentUI;
		private Vector2 _inputMoved;
		private bool spawnItem;
		private ScrollRect _parentScrollRect;
		
		public ItemReference ItemReference { get; private set; }
		
		public RectTransform RectTransform => (RectTransform) transform;

		public void Initialize(ItemCatalogUI parentUI, ItemReference itemReference, ScrollRect parentScrollRect)
		{
			_parentScrollRect = parentScrollRect;
			_parentUI = parentUI;
			ItemReference = itemReference;
		}
		
		public void Deinitialize()
		{
			ItemReference = null;
		}

		public void OnPointerDown(PointerEventData data)
		{
			_inputMoved = Vector2.zero;
			spawnItem = false;
		}
	
		public void OnPointerUp(PointerEventData data)
		{

		}
	
		public void OnPointerClick(PointerEventData data)
		{
			if (!data.dragging) _parentUI.ItemButtonClick(this);
		}
		
		public void OnBeginDrag(PointerEventData data)
		{	
			_parentScrollRect.OnBeginDrag(data);
		}

		public void OnDrag(PointerEventData data)
		{
			_inputMoved.x += Mathf.Abs(data.delta.x);
			_inputMoved.y += Mathf.Abs(data.delta.y);

			RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, data.position, data.pressEventCamera, out Vector2 localPoint);
			if (localPoint.x < 0 && _inputMoved.y < _dragThreshold && !spawnItem)
			{
				spawnItem = true;
				_parentUI.SpawnItemFromMenuDrag(data, this);
			}
			
			if (!spawnItem) _parentScrollRect.OnDrag(data);
		}

		public void OnEndDrag(PointerEventData data)
		{
			_parentScrollRect.OnEndDrag(data);
			Debug.Log("ScrollItem OnEndDrag");
		}
	}
}