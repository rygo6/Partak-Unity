//#define LOG

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GeoTetra;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace GeoTetra.GTSnapper
{
    [RequireComponent(typeof(Item))]
    public class ItemDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Item _item;
        [SerializeField] private Renderer[] _accessoryRendererArray;
        
        private ItemDrop _thisEnteredDropItem;
        private ItemSnap _parentItemSnap;
        private ItemDrop _parentItemDrop;
        private bool _accessoryRendererState = true;

        /// <summary>
        /// Gets the transform that object SmoothDamps too.
        /// </summary>
        /// <value>The target transform.</value>
        public Transform TargetTransform { get; private set; }

        public AttachPoint[] AttachPointArray { get; private set; }

        public ItemSnap ParentItemSnap
        {
            get { return _parentItemSnap; }
            set
            {
                if (_parentItemSnap != null)
                {
                    _parentItemSnap.RemoveItem(GetComponent<Item>());
                    _parentItemSnap = null;
                }

                _parentItemSnap = value;
                if (_parentItemSnap != null)
                {
                    _parentItemSnap.AddItem(GetComponent<Item>());
                }
            }
        }

        public ItemDrop ParentItemDrop
        {
            get { return _parentItemDrop; }
            set
            {
                if (_parentItemDrop != null)
                {
                    _parentItemDrop.ChildItemDragList.Remove(this);
                    _parentItemDrop = null;
                    ParentItemSnap = null;
                }

                _parentItemDrop = value;
                if (_parentItemDrop != null)
                {
                    _parentItemDrop.ChildItemDragList.Add(this);
                }
            }
        }

        public bool AccessoryRendererState
        {
            get { return _accessoryRendererState; }
            set
            {
                if (_accessoryRendererState != value)
                {
                    for (int i = 0; i < _accessoryRendererArray.Length; ++i)
                    {
                        _accessoryRendererArray[i].enabled = value;
                    }

                    _accessoryRendererState = value;
                }
            }
        }

        public ItemDrop ThisEnteredDropItem
        {
            get { return _thisEnteredDropItem; }
            set
            {
                //this be some clever stuff to make it so EnterIntoItemDrop
                //and ItemDragEntered can set each others setters back and forth
                //without causing a stack overflow, recursively calling each other
                //back and forth. Is there a smarter way to do this?	
                if (value == null && _thisEnteredDropItem != null)
                {
                    if (_thisEnteredDropItem != null)
                    {
                        _thisEnteredDropItem.ItemDragEnteredThis = null;
                    }

                    _thisEnteredDropItem = null;
                }
                else if (_thisEnteredDropItem != value)
                {
                    if (_thisEnteredDropItem != null)
                    {
                        _thisEnteredDropItem.ItemDragEnteredThis = null;
                    }

                    _thisEnteredDropItem = value;
                    _thisEnteredDropItem.ItemDragEnteredThis = this;
                }
            }
        }

        public Item Item => _item;

        private void Awake()
        {
            GameObject gameObject = new GameObject(this.name + "Target");
            TargetTransform = gameObject.transform;
            TargetTransform.position = _item.transform.position;
            TargetTransform.up = _item.transform.up;
            AttachPointArray = GetComponentsInChildren<AttachPoint>();
        }

        private void Reset()
        {
            _item = GetComponent<Item>();
        }

        private void Update()
        {
            switch (GetComponent<Item>().State)
            {
                case ItemState.Attached:
                    SmoothToTargetPositionRotation();
                    break;
                case ItemState.AttachedHighlighted:
                    SmoothToTargetPositionRotation();
                    break;
                case ItemState.Dragging:
                    SmoothToTargetPositionRotation();
                    break;
                case ItemState.Floating:
                    SmoothToTargetPositionRotation();
                    break;
                case ItemState.Instantiate:
                    SmoothToTargetPositionRotation();
                    break;
                case ItemState.NoInstantiate:
                    SmoothToTargetPositionRotation();
                    break;
            }
        }

        public ItemSnap NearestItemSnap(PointerEventData data)
        {
            if (ThisEnteredDropItem != null)
            {
                List<ItemSnap> sortedItemSnapList = ThisEnteredDropItem.SortedItemSnapList(data);
                for (int i = 0; i < sortedItemSnapList.Count; ++i)
                {
                    if ((sortedItemSnapList[i].TagMatch(GetComponent<Item>().TagArray) &&
                         sortedItemSnapList[i].AvailableSnap()) ||
                        sortedItemSnapList[i].ContainsItem(GetComponent<Item>()))
                    {
                        return sortedItemSnapList[i];
                    }
                }
            }

            return null;
        }

        public void FloatObjectInMainCamera(PointerEventData data)
        {
            Ray ray = Camera.main.ScreenPointToRay(data.position);
            SetTargetPositionRotation(ray.GetPoint(_item.ItemRoot.ItemSettings.FloatDistance), Vector3.up);
        }

        /// <summary>
        /// Smooths to target position rotation.
        /// </summary>
        /// <returns><c>true</c>, if position and target are equal, <c>false</c> otherwise.</returns>
        private bool SmoothToTargetPositionRotation()
        {
            Item item = GetComponent<Item>();
            if (TargetTransform.position != item.transform.position ||
                TargetTransform.eulerAngles != item.transform.eulerAngles)
            {
                SmoothToPointAndDirection(TargetTransform.position, _posDamp, TargetTransform.up, _rotDamp);
                return false;
            }
            else
            {
                return true;
            }
        }

        private const float _posDamp = .1f;
        private const float _rotDamp = .2f;

        /// <summary>
        /// Sets the target position rotation.
        /// </summary>
        public void SetTargetPositionRotation(Vector3 position, Vector3 direction)
        {
            TranslateTargetPositionRotationRecursive(position - TargetTransform.position,
                direction - TargetTransform.up);
        }

        public void TranslateTargetPositionRotationRecursive(Vector3 deltaPosition, Vector3 deltaDirection)
        {
            TargetTransform.Translate(deltaPosition, Space.World);
            TargetTransform.up = TargetTransform.up + deltaDirection;
            ItemDrop dropItemMod = GetComponent<ItemDrop>();
            if (dropItemMod != null)
            {
                for (int i = 0; i < dropItemMod.ChildItemDragList.Count; ++i)
                {
                    dropItemMod.ChildItemDragList[i]
                        .TranslateTargetPositionRotationRecursive(deltaPosition, deltaDirection);
                }
            }
        }

        /// <summary>
        /// Sets the actual position to target.
        /// Immediately set transform equal to the targetPosition and Rotation.
        ///	This is done in instances where object may switch to attached before fully done smoothing to target.
        /// </summary>
        public void SetActualPositionRotationToTarget()
        {
             _item.transform.position = TargetTransform.position;
             _item.transform.rotation = TargetTransform.rotation;
            _smoothVelocity = Vector3.zero;
            _smoothAngleVelocity = Vector3.zero;
        }

        /// <summary>
        /// Sets the target to actual position direction.
        /// </summary>
        public void SetTargetToAcualPositionDirection()
        {
            SetTargetPositionRotation(_item.transform.position, _item.transform.up);
        }

        /// <summary>
        /// Smooths to point and direction.
        /// </summary>
        /// <param name="point">Point.</param>
        /// <param name="moveSmooth">Move smooth.</param>
        /// <param name="direction">Direction.</param>
        /// <param name="rotSmooth">Rot smooth.</param>
        private void SmoothToPointAndDirection(Vector3 point, float moveSmooth, Vector3 direction, float rotSmooth)
        {
            _item.transform.position = Vector3.SmoothDamp(transform.position, point, ref _smoothVelocity, moveSmooth);
            _item.transform.up =
                Vector3.SmoothDamp(transform.up, direction, ref _smoothAngleVelocity, rotSmooth);
        }

        private Vector3 _smoothVelocity;
        private Vector3 _smoothAngleVelocity;

        public void OnBeginDrag(PointerEventData data)
        {
#if LOG
		Debug.Log( "OnBeginDrag " + this.name );
#endif

            ItemUtility.StateSwitch(data, _item.State,
                OnBeginDragAttached,
                OnBeginDragAttachedHighlighted,
                null,
                null,
                OnBeginDragInstantiate,
                OnBeginDragNoInstantiate
            );
        }

        private void OnBeginDragAttached(PointerEventData data)
        {
#if LOG
            Debug.Log( "OnBeginDragAttached " + this.name );
#endif
//			Orbit.Instance.InputNoOrbit[data.pointerId.NoNegative()] = false;	
            _item.SetShaderNormal();
        }

        private void OnBeginDragAttachedHighlighted(PointerEventData data)
        {
#if LOG
            Debug.Log( "OnBeginDragAttachedHighlighted " + this.name );
#endif
            _item.SetShaderOutline(_item.ItemRoot.ItemSettings.HighlightItemColor);
            SwitchAttachedToDragging(data);
        }

        private void OnBeginDragInstantiate(PointerEventData data)
        {
#if LOG
            Debug.Log( "OnBeginDragInstantiate " + this.name );
#endif
            _item.SetShaderOutline(_item.ItemRoot.ItemSettings.InstantiateOutlineColor);
        }

        private void OnBeginDragNoInstantiate(PointerEventData data)
        {
#if LOG
            Debug.Log( "OnBeginDragNoInstantiate " + this.name );
#endif
            _item.SetShaderNormal();
        }

        private void SwitchAttachedToDragging(PointerEventData data)
        {
            AccessoryRendererState = false;
            ParentItemDrop = null;
            _item.ResetColliderSize();
            _item.AddToHoldList();
            _item.State = ItemState.Dragging;

            //This ensure that the item is still hovering over the item_Drop it was attached to
            //otherwise it disattaches it, this is done because OnPointerExit will only get called on
            //the item_Drop if the mouse pointer actuall exits it's collider, which may not always occur
//			if (data.pointerCurrentRaycast.gameObject == null ||
//			   (data.pointerCurrentRaycast.gameObject != null && ThisEnteredDropItem != null &&
//			   data.pointerCurrentRaycast.gameObject.transform.parent.gameObject != ThisEnteredDropItem.gameObject))
//			{
//				ThisEnteredDropItem = null;
//			}
        }

        public void OnDrag(PointerEventData data)
        {
#if LOG
		Debug.Log("OnDrag "+this.name);
#endif

            ItemUtility.StateSwitch(data, GetComponent<Item>().State,
                null,
                null,
                OnDragDragging,
                null,
                null,
                null
            );
        }

        private void OnDragDragging(PointerEventData data)
        {
#if LOG
            Debug.Log("OnDragDragging "+this.name);
#endif
            
            if (ThisEnteredDropItem == null)
            {
                FloatObjectInMainCamera(data);
                ParentItemSnap = null;
            }
            else
            {
                ItemSnap currentNearestItemSnap = NearestItemSnap(data);
                if (currentNearestItemSnap == null)
                {
                    FloatObjectInMainCamera(data);
                    ParentItemSnap = null;
                }
                else
                {
                    if (ParentItemSnap != currentNearestItemSnap)
                    {
                        ParentItemSnap = currentNearestItemSnap;
                    }

                    Ray ray = ParentItemSnap.Snap(_item, data);
                    SetTargetPositionRotation(ray.origin, ray.direction);
                }
            }
        }

        public void OnEndDrag(PointerEventData data)
        {
#if LOG
		Debug.Log( "OnEndDrag " + this.name );
#endif

            ItemUtility.StateSwitch(data, GetComponent<Item>().State,
                null,
                null,
                OnEndDragDragging,
                null,
                null,
                null
            );
        }

        private void OnEndDragDragging(PointerEventData data)
        {
#if LOG
            Debug.Log( "OnEndDragDragging " + this.name );
#endif
            
            if (ThisEnteredDropItem == null)
            {
//				StartCoroutine(_item.DestroyItemCoroutine());
                _item.SetLayerRecursive(0);
            }
            else
            {
                _item.RemoveFromHoldList();
                AccessoryRendererState = true;
                _item.SetLayerRecursive(0);
                _item.State = ItemState.AttachedHighlighted;
            }
        }
    }
}