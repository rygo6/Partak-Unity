using System;
using UnityEngine;
using System.Collections.Generic;
using GeoTetra.GTCommon.Components;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using GeoTetra.GTSnapper.ScriptableObjects;
using UnityEditor;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace GeoTetra.GTSnapper
{
    public class ItemRoot : SubscribableBehaviour
    {
        [SerializeField] private ServiceReference _componentContainer;
        [SerializeField] private Catcher _catcher;
        [SerializeField] private ItemSettings _itemSettings;
        [SerializeField] private List<Item> _rootItems;
        
        public readonly Dictionary<string, MonoBehaviour> UniqueTickDictionary = new Dictionary<string, MonoBehaviour>();

        public readonly List<Item> ItemHoldList = new List<Item>();
        public readonly List<Item> ItemHighlightList = new List<Item>();
        private ItemRootDatum _itemRootDatum;

        public ItemSettings ItemSettings => _itemSettings;
        
        public Catcher Catcher => _catcher;

        private void Awake()
        {
            _componentContainer.Service<ComponentContainer>().RegisterComponent(this);
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
            Action<Item> action = delegate(Item item)
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

        [ContextMenu("Serialize")]
        private void SerializeTest()
        {
            Serialize("TestSave");
        }
        
        public void Serialize(string filename)
        {
            Debug.Log($"Serializing {filename}");
            if (_itemRootDatum == null)
            {
                _itemRootDatum = new ItemRootDatum();
            }

            _itemRootDatum._dateCreated = DateTime.Now.ToFileTimeUtc();
            _itemRootDatum._itemDatums.Clear();

            for (int i = 0; i < _rootItems.Count; ++i)
            {
                _rootItems[i].Serialize(_itemRootDatum._itemDatums);
            }

            string json = JsonUtility.ToJson(_itemRootDatum);

            System.IO.File.WriteAllText(filename, json);
            Debug.Log(json);
        }
        
        [ContextMenu("Deserialize")]
        private void DeserializeTest()
        {
            Deserialize("TestSave");
        }
        
        public void Deserialize(string filename)
        {
            Debug.Log($"Deserializing {filename}");
            if (!System.IO.File.Exists(filename))
            {
                Debug.Log($"{filename} not found to load.");
                return;
            }
            
            string json = System.IO.File.ReadAllText(filename);
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
            Addressables.InstantiateAsync(reference.Result.AssetPrefabName, new InstantiationParameters(itemDatum._position, itemDatum._rotation, null))
                .Completed += handle => OnInstantiateComplete(handle.Result, reference.Result, itemDatum);
        }

        private void OnInstantiateComplete(GameObject gameObject, ItemReference itemReference, ItemDatum itemDatum)
        {
            Item item = gameObject.GetComponent<Item>();
            item.Initialize(item.transform.position, this, itemReference, _catcher);
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
        }

        private int _currentLoadingItemCount = 0;
        private int _totalLoadingItemCount = 0;
    }
}