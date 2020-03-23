//#define LOG

using UnityEngine;
using System.Collections.Generic;
using GeoTetra.GTCommon.Utility;
using UnityEngine.EventSystems;

namespace GeoTetra.GTSnapper
{
    [RequireComponent(typeof(Item))]
    public class ItemDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Item _item;
        [SerializeField] private Renderer[] _accessoryRendererArray;
        [SerializeField] private float _minScale = .4f;
        [SerializeField] private float _maxScale = 4f;
        
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
            get => _parentItemSnap;
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
            get => _parentItemDrop;
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
            get => _accessoryRendererState;
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
            get => _thisEnteredDropItem;
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

        public float MinScale => _minScale;

        public float MaxScale => _maxScale;

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
            switch (_item.State)
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
            if (TargetTransform.position != _item.transform.position || TargetTransform.eulerAngles != _item.transform.eulerAngles || TargetTransform.localScale != _item.transform.localScale)
            {
                SmoothToTarget(TargetTransform.position, PosDamp, TargetTransform.rotation, RotDamp, TargetTransform.localScale, ScaleDamp);
                return false;
            }
            else
            {
                return true;
            }
        }

        private const float PosDamp = .1f;
        private const float RotDamp = .1f;
        private const float ScaleDamp = .1f;

        /// <summary>
        /// Sets the target position rotation.
        /// </summary>
        public void SetTargetPositionRotation(Vector3 position, Vector3 direction)
        {
//            TranslateTargetPositionRotationRecursive(position - TargetTransform.position,direction - TargetTransform.up);
            TranslateTargetPositionRotationRecursive(position - TargetTransform.position, Quaternion.identity);
        }

        public void TranslateTargetPositionRotationRecursive(Vector3 deltaPosition, Quaternion delatRotation)
        {
            TargetTransform.Translate(deltaPosition, Space.World);
//            TargetTransform.up = TargetTransform.up + deltaDirection;
            if (_item.Drop != null)
            {
                for (int i = 0; i < _item.Drop.ChildItemDragList.Count; ++i)
                {
                    _item.Drop.ChildItemDragList[i].TranslateTargetPositionRotationRecursive(deltaPosition, delatRotation);
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
            _smoothScaleVelocity = Vector3.zero;
            _smoothRotationVelocity = Quaternion.identity;
        }

        /// <summary>
        /// Sets the target to actual position direction.
        /// </summary>
        public void SetTargetToActualPositionDirection()
        {
            TargetTransform.position = _item.transform.position;
            TargetTransform.rotation = _item.transform.rotation;
            TargetTransform.localScale = _item.transform.localScale;
        }

        /// <summary>
        /// Smooths to point and direction.
        /// </summary>
        /// <param name="point">Point.</param>
        /// <param name="moveSmooth">Move smooth.</param>
        /// <param name="direction">Direction.</param>
        /// <param name="rotSmooth">Rot smooth.</param>
        private void SmoothToTarget(Vector3 point, float moveSmooth, Quaternion rotation, float rotSmooth, Vector3 scale, float scaleSmooth)
        {
            _item.transform.position = Vector3.SmoothDamp(transform.position, point, ref _smoothVelocity, moveSmooth);
            _item.transform.rotation = QuaternionUtility.SmoothDamp(_item.transform.rotation, rotation,ref _smoothRotationVelocity, rotSmooth);
            _item.transform.localScale = Vector3.SmoothDamp(_item.transform.localScale, scale, ref _smoothScaleVelocity, scaleSmooth);
        }

        private Vector3 _smoothVelocity;
        private Vector3 _smoothScaleVelocity;
        private Quaternion _smoothRotationVelocity;

        public void OnBeginDrag(PointerEventData data)
        {
#if LOG
		Debug.Log( "OnBeginDrag " + this.name + " " + _item.State );
#endif

            ItemUtility.StateSwitch(data, _item.State,
                OnBeginDragAttached,
                OnBeginDragAttachedHighlighted,
                null,
                OnBeginDragAttachedHighlighted,
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
            SetTargetToActualPositionDirection();
            _item.ResetColliderSize();
            _item.ItemRoot.BeginDragging(this);
            _item.State = ItemState.Dragging;
            _item.SetLayerRecursive(_item.ItemRoot.IgnoreLayer);
            _dragFirstCycle = true;

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

        /// <summary>
        /// Used to skip the first cycle of the drag. This allows OnEnter to get called on a dropped item
        /// so that ThisEnteredDropItem is appropriately set, otherwise Item may jump one frame.
        /// </summary>
        private bool _dragFirstCycle;

        public void OnDrag(PointerEventData data)
        {
#if LOG
//		Debug.Log("OnDrag " + this.name);
#endif
            if (_dragFirstCycle)
            {
                _dragFirstCycle = false;
                return;
            }
            
            
            ItemUtility.StateSwitch(data, _item.State,
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
//            Debug.Log("OnDragDragging "+this.name);
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
		Debug.Log( "OnEndDrag " + this.name + " " + _item.State );
#endif

            ItemUtility.StateSwitch(data, _item.State,
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
				StartCoroutine(_item.DestroyItemCoroutine());
//                _item.State = ItemState.Floating;
//                _item.SetLayerRecursive(_item.ItemRoot.ItemLayer);
            }
            else
            {
                ParentItemDrop = ThisEnteredDropItem;
                _item.ItemRoot.EndDragging(this);
                AccessoryRendererState = true;
                _item.SetLayerRecursive(_item.ItemRoot.ItemLayer);
                _item.State = ItemState.AttachedHighlighted;
            }
        }
    }
}