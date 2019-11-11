using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace GeoTetra.GTSnapper
{
    public abstract class ItemSnap : MonoBehaviour
    {
        [SerializeField] private Item _parentItem;
        
        public readonly List<Item> ChildItemList = new List<Item>();

        //Should each snap have its own GUID?
        //Yes because if you append some kind of key onto the end
        //of the GUID to specify ItemSnap, then you will have to 
        //do a whole bunch more string parsing and GC allocs when 
        //deserializing.
        public string UniqueTick
        {
            get
            {
                if (string.IsNullOrEmpty(_uniqueTick))
                {
                    _uniqueTick = ItemUtility.TickToString();
                    Root.UniqueTickDictionary.Add(_uniqueTick, this);
                }

                return _uniqueTick;
            }
            set
            {
                if (!string.IsNullOrEmpty(_uniqueTick))
                {
                    Root.UniqueTickDictionary.Remove(_uniqueTick);
                }

                _uniqueTick = value;
                Root.UniqueTickDictionary.Add(_uniqueTick, this);
            }
        }

        private string _uniqueTick;

        public ItemRoot Root { get; private set; }

        public string[] ChildTagArray => _childTagArray;

        public Item ParentItem => _parentItem;

        [Header(
            "Enter a specific set of tags for this point. If no tags are entered it will inherit them from the parent item")]
        [SerializeField]
        private string[] _childTagArray;

        protected virtual void Awake()
        {
            //TODO make automaticaly set by generator
            Root = FindObjectOfType<ItemRoot>();

            if (_childTagArray == null || _childTagArray.Length == 0)
            {
                _childTagArray = GetComponentInParent<Item>().ChildTagArray;
            }
        }

        protected void Reset()
        {
            _parentItem = GetComponentInParent<Item>();
        }

        public bool TagMatch(string[] tagArray)
        {
            return ItemUtility.TestTagArrays(tagArray, ChildTagArray);
        }

        /// <summary>
        /// Gets the point to snap to.
        /// </summary>
        public abstract Ray Snap(Item item, PointerEventData data);

        /// <summary>
        /// Nearest Snappable point.
        /// </summary>
        /// <returns>The point.</returns>
        /// <param name="data">Data.</param>
        public abstract Vector3 NearestPoint(PointerEventData data);

        /// <summary>
        /// Is called from ItemDrag when setting ParentItemSnap value
        /// </summary>
        public virtual void AddItem(Item item)
        {
            ChildItemList.Add(item);
        }

        /// <summary>
        /// Is called from ItemDrag when setting ParentItemSnap value
        /// </summary>
        public virtual void RemoveItem(Item item)
        {
            ChildItemList.Remove(item);
        }

        /// <summary>
        /// Returns if this snap already contains a given item.
        /// </summary>
        /// <returns><c>true</c>, if item was containsed, <c>false</c> otherwise.</returns>
        /// <param name="item">Item.</param>
        public virtual bool ContainsItem(Item item)
        {
            return ChildItemList.Contains(item);
        }

        /// <summary>
        /// Determines whether this instance can snap the specified item.
        /// </summary>
        /// <returns><c>true</c> if this instance can snap the specified item; otherwise, <c>false</c>.</returns>
        /// <param name="item">Item.</param>
        public abstract bool AvailableSnap();
        
        /// <summary>
        /// If any of the ItemDrag's attach points have appropriate tag, return the position offset by that attachpoint.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected Vector3 GetAttachPointOffset(Vector3 position, Item item)
        {
            if (item.Drag.AttachPointArray != null && item.Drag.AttachPointArray.Length > 0)
            {
                for (int i = 0; i < item.Drag.AttachPointArray.Length; ++i)
                {
                    if (ItemUtility.TestTagArrays(item.Drag.AttachPointArray[i].TagArray, ChildTagArray))
                    {
                        return position - item.Drag.AttachPointArray[i].transform.localPosition;
                    }
                }
                return position;
            }
            else
            {
                return position;
            }
        }
    }
}