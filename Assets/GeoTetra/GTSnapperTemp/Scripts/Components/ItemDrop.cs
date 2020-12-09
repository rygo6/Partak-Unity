// #define LOG

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace GeoTetra.GTSnapper
{
    [RequireComponent(typeof(Item))]
    public class ItemDrop : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Item _item;
        [SerializeField] private ItemSnap[] _itemSnapArray;

        //Do we need this if the children are in the snaps? It should be more performant to iterate
        //this than a list of lists. So for sake of improving dragging perf in complex composite objects
        //keep it like this.
        public List<ItemDrag> ChildItemDragList = new List<ItemDrag>();
        private ItemDrag _itemDragEnteredThis;

        /// <summary>
        /// Stores the Item that Entered or Exited this object via the EventSystem.
        /// </summary>
        /// <value>The item entered.</value>
        public ItemDrag ItemDragEnteredThis
        {
            get { return _itemDragEnteredThis; }
            set
            {
                //this be some clever stuff to make it so EnterIntoItemDrop
                //and ItemDragEntered can set each others setters back and forth
                //without causing a stack overflow, recursively calling each other
                //back and forth.	
                if (value == null)
                {
                    ItemDrag tempItemDrag = _itemDragEnteredThis;
                    _itemDragEnteredThis = null;
                    if (tempItemDrag != null)
                    {
                        tempItemDrag.ThisEnteredDropItem = null;
                    }
                }
                else if (_itemDragEnteredThis != value)
                {
                    if (_itemDragEnteredThis != null)
                    {
                        _itemDragEnteredThis.ThisEnteredDropItem = null;
                    }

                    _itemDragEnteredThis = value;
                    _itemDragEnteredThis.ThisEnteredDropItem = this;
                }
            }
        }

        public ItemSnap[] ItemSnapArray => _itemSnapArray;

        private void Reset()
        {
            _itemSnapArray = GetComponentsInChildren<ItemSnap>();
            _item = GetComponent<Item>();
        }

        /// <summary>
        /// Returns the preallocated and sorted _itemSnapList.
        /// Pass in a PointerEventData and it returns the itemSnapList
        /// with the itemSnaps in order of nearest to farthest.
        /// </summary>
        /// <returns>The item snap list.</returns>
        /// <param name="data">Data.</param>
        public List<ItemSnap> SortedItemSnapList(PointerEventData data)
        {
            if (_sortedItemSnapList == null)
            {
                _sortedItemSnapList = new List<ItemSnap>();
                for (int i = 0; i < _itemSnapArray.Length; ++i)
                {
                    _sortedItemSnapList.Add(_itemSnapArray[i]);
                }
            }

            _sortedItemSnapList.Sort(
                delegate(ItemSnap itemSnap0, ItemSnap itemSnap1)
                {
                    float distance0 = (data.pointerCurrentRaycast.worldPosition - itemSnap0.NearestPoint(data))
                        .sqrMagnitude;
                    float distance1 = (data.pointerCurrentRaycast.worldPosition - itemSnap1.NearestPoint(data))
                        .sqrMagnitude;
                    if (distance0 > distance1)
                    {
                        return 1;
                    }
                    else if (distance0 == distance1)
                    {
                        return 0;
                    }
                    else
                    {
                        return -1;
                    }
                }
            );
            return _sortedItemSnapList;
        }

        private List<ItemSnap> _sortedItemSnapList;

        public void Serialize(ItemDatum itemDatum)
        {
            if (itemDatum._itemSnapUniqueTicks == null)
            {
                itemDatum._itemSnapUniqueTicks = new List<string>();
            }
            else
            {
                itemDatum._itemSnapUniqueTicks.Clear();
            }
            for (int i = 0; i < _itemSnapArray.Length; ++i)
            {
                itemDatum._itemSnapUniqueTicks.Add(_itemSnapArray[i].UniqueTick);
            }
        }

        public void Deserialize(ItemDatum itemDatum)
        {
            for (int i = 0; i < itemDatum._itemSnapUniqueTicks.Count; ++i)
            {
                _itemSnapArray[i].UniqueTick = itemDatum._itemSnapUniqueTicks[i];
            }
        }
        
        public bool CanAttach(string[] tagArray)
        {
            for (int i = 0; i < _itemSnapArray.Length; ++i)
            {
                if (_itemSnapArray[i].TagMatch(tagArray) && _itemSnapArray[i].AvailableSnap())
                {
                    return true;
                }
            }

            return false;
        }

        public bool ContainsItem(Item containedItem)
        {
            for (int i = 0; i < _itemSnapArray.Length; ++i)
            {
                if (_itemSnapArray[i].ContainsItem(containedItem))
                {
                    return true;
                }
            }

            return false;
        }

        public void OnDrop(PointerEventData data)
        {
#if LOG
			Debug.Log( "OnDrop " + this.name );
#endif

            ItemUtility.StateSwitch(data, _item.State,
                OnDropAttached,
                null,
                null,
                null,
                null,
                null
            );
        }

        private void OnDropAttached(PointerEventData data)
        {

        }

        public void OnPointerExit(PointerEventData data)
        {
#if LOG
			Debug.Log( "OnPointerExit " + this.name );
#endif

            ItemUtility.StateSwitch(data, GetComponent<Item>().State,
                OnPointerExitAttached,
                null,
                null,
                null,
                OnPointerExitInstantiate,
                null
            );
        }

        private void OnPointerExitAttached(PointerEventData data)
        {
#if LOG
			Debug.Log( "OnPointerExitAttached " + this.name );
#endif
            
            _item.SetShaderNormal();
            if (ItemDragEnteredThis != null)
            {
                ItemDragEnteredThis.AccessoryRendererState = false;
                ItemDragEnteredThis = null;
            }
        }

        private void OnPointerExitInstantiate(PointerEventData data)
        {
#if LOG
            Debug.Log( "OnPointerExitInstantiate " + this.name );
#endif
            
            _item.SetShaderOutline(_item.ItemRoot.ItemSettings.InstantiateOutlineColor);
        }
        
        public void OnPointerEnter(PointerEventData data)
        {
#if LOG
			Debug.Log( "OnPointerEnter " + this.name );
#endif

            ItemUtility.StateSwitch(data, GetComponent<Item>().State,
                OnPointerEnterAttached,
                null,
                null,
                null,
                OnPointerEnterAttached,
                null
            );
        }

        private void OnPointerEnterAttached(PointerEventData data)
        {
#if LOG
			Debug.Log( "OnPointerEnterAttached " + this.name );
#endif

            if (_item.ItemRoot.CurrentlyUsedItem != null)
            {
                Item item = _item.ItemRoot.CurrentlyUsedItem;
                // Item item = data.pointerPress.GetComponent<Item>(); //TODO nonalloc
                if (item != null && CanAttach(item.TagArray) ||
                    item != null && ContainsItem(item))
                {
                    _item.SetShaderOutline(_item.ItemRoot.ItemSettings.DropOutlineColor);
                    ItemDragEnteredThis = item.GetComponent<ItemDrag>();
                    ItemDragEnteredThis.AccessoryRendererState = true;
                }
            }
        }
    }
}