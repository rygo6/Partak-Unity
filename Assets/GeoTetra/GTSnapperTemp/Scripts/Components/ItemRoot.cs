using System;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using GeoTetra.GTCommon.Components;
using GeoTetra.GTPooling;
using GeoTetra.GTSnapper.ScriptableObjects;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.Serialization;

namespace GeoTetra.GTSnapper
{
    public class ItemRoot : SubscribableBehaviour
    {
        [SerializeField] 
        private ComponentContainerRef _componentContainer;
        
        [SerializeField] private InputCatcher _inputCatcher;
        [SerializeField] private ItemSettings _itemSettings;
        [SerializeField] private List<Item> _rootItems;
        [SerializeField] private ItemGizmoRoot _itemGizmoRoot;
        [SerializeField] private Material _highlightMaterial;

        public event Action DeserializationComplete;
        
        public readonly Dictionary<string, MonoBehaviour> UniqueTickDictionary = new Dictionary<string, MonoBehaviour>();

        private readonly List<ItemDrag> ItemDragList = new List<ItemDrag>();
        private readonly List<Item> ItemHighlightList = new List<Item>();
        
        private readonly List<AsyncOperationHandle<ItemReference>> _referenceHandles = new List<AsyncOperationHandle<ItemReference>>();
        private ItemRootDatum _itemRootDatum;
        
        /// <summary>
        /// The Item which events are currently being forwarded to.
        /// Necessary because when you manipulate via a Gizmo, the data.pointerPress
        /// will be the gizmo, and not this.
        /// </summary>
        public Item CurrentlyUsedItem { get; set; }
        public int IgnoreLayer { get; private set; }
        public int ItemLayer { get; private set; }
        public int ItemGizmoLayer { get; private set; }
        
        public int ItemCount { get; set; }

        public ItemSettings ItemSettings => _itemSettings;
        
        public InputCatcher InputCatcher => _inputCatcher;

        public Material HighlightMaterial => _highlightMaterial;

        private void Awake()
        {
            IgnoreLayer = LayerMask.NameToLayer("Ignore Raycast");
            ItemLayer = LayerMask.NameToLayer("Item");
            ItemGizmoLayer = LayerMask.NameToLayer("ItemGizmo");
            _itemGizmoRoot.Initialize(this);
            for (int i = 0; i < _rootItems.Count; ++i)
            {
                _rootItems[i].Initialize(this, null, _inputCatcher);
            }
        }

        protected override async Task StartAsync()
        {
            await _componentContainer.CacheAndRegister(this);
            await base.StartAsync();
        }

        protected override void OnDestroy()
        {
            foreach (KeyValuePair<string,MonoBehaviour> pair in UniqueTickDictionary)
            {
                if (pair.Value is Item)
                {
                    Addressables.ReleaseInstance(pair.Value.gameObject);
                }
            }

            foreach (AsyncOperationHandle<ItemReference> referenceHandle in _referenceHandles)
            {
                Addressables.Release(referenceHandle);
            }
            base.OnDestroy();
        }

        public void BeginDragging(ItemDrag itemDrag)
        {
            ItemDragList.Add(itemDrag);
            if (ItemDragList.Count == 1)
            {
                _itemGizmoRoot.SetIgnoreRaycastLayer();
                // _itemGizmoRoot.UntargetItem();
            }
        }

        public void EndDragging(ItemDrag itemDrag)
        {
            ItemDragList.Remove(itemDrag);
            if (ItemDragList.Count == 0)
            {
                _itemGizmoRoot.SetItemGizmoLayer();
                // _itemGizmoRoot.TargetItem(itemDrag.Item);
            }
        }
        
        public void Highlight(Item item)
        {
            if (_inputCatcher.EmptyClickAction == null)
            {
                _inputCatcher.EmptyClickAction = () => { UnHighlightAll(); };;
            }
            ItemHighlightList.Add(item);
            _itemGizmoRoot.TargetItem(item);
        }
        
        public void Unhighlight(Item item)
        {
            ItemHighlightList.Remove(item);
            _itemGizmoRoot.UntargetItem();
        }
        
        public void UnHighlightAll(Action<Item> postAction = null)
        {
            for (int i = 0; i < ItemHighlightList.Count; ++i)
            {
                Item item = ItemHighlightList[i];
                item.UnHighlight();
                postAction?.Invoke(item);
            }
        }

        public void SetAllOutlineNormalAttach()
        {
            Action<Item> action = delegate(Item itemRaycast)
            {
                itemRaycast.SetShaderNormal();
                itemRaycast.State = ItemState.Attached;
            };
            CallDelegateAll(action);
        }

        public void SetAllActualPositionToTarget()
        {
            Action<Item> action = (Item item) =>
            {
                ItemDrag dragItemMod = item.GetComponent<ItemDrag>();
                if (dragItemMod != null)
                {
                    dragItemMod.SetActualPositionRotationToTarget();
                }
            };
            CallDelegateAll(action);
        }

        public int CallDelegateTagFilter(Func<Item, bool> filterAction, Action<Item> trueAction, Action<Item> falseAction)
        {
            int trueCount = 0;
            for (int i = 0; i < _rootItems.Count; ++i)
            {
                trueCount += CallDelegateTagFilterItemRaycast(_rootItems[i], filterAction, trueAction, falseAction);
            }

            return trueCount;
        }

        private int CallDelegateTagFilterItemRaycast(Item item, Func<Item, bool> filterAction, Action<Item> trueAction, Action<Item> falseAction)
        {
            int trueCount = 0;
            if (filterAction(item))
            {
                trueAction(item);
                trueCount = 1;
            }
            else if (falseAction != null)
            {
                falseAction(item);
            }
            
            if (item.Drop != null)
            {
                for (int i = 0; i < item.Drop.ChildItemDragList.Count; ++i)
                {
                    trueCount += CallDelegateTagFilterItemRaycast(item.Drop.ChildItemDragList[i].Item, filterAction, trueAction, falseAction);
                }
            }

            return trueCount;
        }

        public void CallDelegateAll(Action<Item> action)
        {
            for (int i = 0; i < _rootItems.Count; ++i)
            {
                CallDelegateItemRaycast(_rootItems[i], action);
            }
        }

        public void CallDelegateItemRaycast(Item item, System.Action<Item> action)
        {
            action(item);
            ItemDrop itemDrop = item.GetComponent<ItemDrop>();
            if (itemDrop != null)
            {
                for (int i = 0; i < itemDrop.ChildItemDragList.Count; ++i)
                {
                    CallDelegateItemRaycast(itemDrop.ChildItemDragList[i].GetComponent<Item>(), action);
                }
            }
        }

        public string Serialize()
        {
            if (_itemRootDatum == null)
            {
                _itemRootDatum = new ItemRootDatum();
            }
            
            _itemRootDatum._itemDatums.Clear();

            for (int i = 0; i < _rootItems.Count; ++i)
            {
                _rootItems[i].Serialize(_itemRootDatum._itemDatums);
            }

            return JsonUtility.ToJson(_itemRootDatum);
        }

        public void Deserialize(string json)
        {
            _itemRootDatum = JsonUtility.FromJson<ItemRootDatum>(json);

            _currentLoadingItemCount = 0;
            _totalLoadingItemCount = 0;
            for (int i = 0; i < _itemRootDatum._itemDatums.Count; ++i)
            {
                if (!string.IsNullOrEmpty(_itemRootDatum._itemDatums[i]._rootName))
                {
                    Item rootItem = _rootItems.Find(item => item.gameObject.name == _itemRootDatum._itemDatums[i]._rootName);
                    if (rootItem != null)
                    {
                        rootItem.Deserialize(_itemRootDatum._itemDatums[i]);
                    }
                    else
                    {
                        Debug.LogWarning($"Did not find root item {_itemRootDatum._itemDatums[i]._rootName}");
                    }
                }
                else if (!string.IsNullOrEmpty(_itemRootDatum._itemDatums[i]._referenceName))
                {
                    ItemDatum datum = _itemRootDatum._itemDatums[i];
                    Addressables.LoadAssetAsync<ItemReference>(_itemRootDatum._itemDatums[i]._referenceName).Completed += handle => OnItemReferenceComplete(handle, datum);
                    _totalLoadingItemCount++;
                }
            }
        }

        private void OnItemReferenceComplete(AsyncOperationHandle<ItemReference> reference, ItemDatum itemDatum)
        {
            _referenceHandles.Add(reference);
            Addressables.InstantiateAsync(reference.Result.AssetPrefabName, new InstantiationParameters(itemDatum._position, itemDatum._rotation, transform))
                .Completed += handle => OnInstantiateComplete(handle.Result, reference.Result, itemDatum);
        }

        private void OnInstantiateComplete(GameObject gameObject, ItemReference itemReference, ItemDatum itemDatum)
        {
            Item item = gameObject.GetComponent<Item>();
            item.Initialize(this, itemReference, _inputCatcher);
            item.Deserialize(itemDatum);

            _currentLoadingItemCount++;
            if (_currentLoadingItemCount == _totalLoadingItemCount) HookupParentChildRelationship();
        }

        private void HookupParentChildRelationship()
        {
            foreach (KeyValuePair<string,MonoBehaviour> tickItem in UniqueTickDictionary)
            {
                if (tickItem.Value is Item)
                {
                    Item item = tickItem.Value as Item;
                    if (UniqueTickDictionary.TryGetValue(item.ItemDatum._parentItemSnap, out var parentItemSnap))
                    {
                        item.Drag.ParentItemSnap = parentItemSnap as ItemSnap;
                        item.Drag.ParentItemDrop = item.Drag.ParentItemSnap.ParentItem.Drop;
                    }
                }
            }
            
            Debug.Log("ItemRoot Deserialization Complete");
            DeserializationComplete?.Invoke();
        }

        private int _currentLoadingItemCount = 0;
        private int _totalLoadingItemCount = 0;
    }
}