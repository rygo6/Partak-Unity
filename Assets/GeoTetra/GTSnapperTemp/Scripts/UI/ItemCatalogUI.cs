using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
using GeoTetra.GTCommon.Components;
using GeoTetra.GTPooling;
using GeoTetra.GTSnapper.ScriptableObjects;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace GeoTetra.GTSnapper
{
    public class ItemCatalogUI : SubscribableBehaviour
    {
        [SerializeField] 
        private ComponentContainerRef _componentContainer;
        
        [SerializeField] private Canvas _canvas;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private ScrollItem _scrollItemPrefab;
        [SerializeField] private Canvas _scrollItemHighlight;
        [SerializeField] private ItemSettings _itemSettings;

        private readonly List<ScrollItem> _scrollItemPool = new List<ScrollItem>();

        private ItemRoot _itemRoot;
        private ScrollItem _selectedItem;
        private AsyncOperationHandle<IList<ItemReference>> _loadHandle;

        public ItemRoot ItemRoot => _itemRoot;
        public Canvas Canvas => _canvas;

        protected override async Task StartAsync()
        {
            _scrollItemHighlight.enabled = false;
            await _componentContainer.CacheAndRegister(this);
            await base.StartAsync();
        }
        
        public void Initialize()
        {
            _itemRoot = _componentContainer.Ref.Get<ItemRoot>();
            _itemRoot.ItemEvent += ItemRootOnItemEvent;
            LoadItemReferencesFromAssets("ItemReference");
        }
        
        public void Deinitialize()
        {
            _itemRoot.ItemEvent -= ItemRootOnItemEvent;
            _itemRoot = null;
            foreach (ScrollItem scrollItem in _scrollItemPool)
            {
                scrollItem.Deinitialize();
            }
            Addressables.Release(_loadHandle);
        }

        protected override void OnDestroy()
        {
            Deinitialize();
            base.OnDestroy();
        }

        private void ItemRootOnItemEvent(Item item, ItemState state, ItemAction action)
        {
            // this needs to be thought through more
            // if ((state == ItemState.NoInstantiate) && action == ItemAction.Up)
            // {
            //     UnselectSelectedItem();
            // }
        }
        
        public void UnselectSelectedItem()
        {
            if (_selectedItem != null)
            {
                _itemRoot.SetAllOutlineNormalAttach();
                _scrollItemHighlight.enabled = false;
                _selectedItem = null;
            }
        }

        public void ItemButtonClick(ScrollItem scrollItem)
        {
            if (_selectedItem != scrollItem)
            {
                UnselectSelectedItem();
                _selectedItem = scrollItem;

                void TrueAction(Item item)
                {
                    item.SetShaderOutline(_itemSettings.InstantiateOutlineColor);
                    item.State = ItemState.Instantiate;
                    item.LastItemCatalogUUI = this;
                }

                void FalseAction(Item item)
                {
                    item.SetShaderNormal();
                    item.State = ItemState.NoInstantiate;
                    item.LastItemCatalogUUI = null;
                }

                bool FilterAction(Item item)
                {
                    if (item.Drop != null)
                    {
                        return item.Drop.CanAttach(_selectedItem.ItemReference.TagArray);
                    }

                    ItemColor itemColor = item.GetComponent<ItemColor>();
                    const string colorTag = "Color";
                    if (itemColor != null && _selectedItem.ItemReference.HasTag(colorTag))
                    {
                        return true;
                    }

                    return false;
                }

                int trueCount = _itemRoot.CallDelegateTagFilter(FilterAction, TrueAction, FalseAction);

                if (trueCount == 0)
                {
                    _itemRoot.SetAllOutlineNormalAttach();
                    _selectedItem = null;
                }
                else
                {
//                    void Action()
//                    {
//                        _itemRoot.UnHighlightAll();
//                        UnselectSelectedItem();
//                    }
//
//                    _itemRoot.InputCatcher.EmptyClickAction = Action;

                    _scrollItemHighlight.enabled = true;
                    _scrollItemHighlight.transform.SetParent(_selectedItem.transform);
                    ((RectTransform)_scrollItemHighlight.transform).anchoredPosition3D = Vector3.zero;
                    ((RectTransform)_scrollItemHighlight.transform).sizeDelta = Vector2.zero;
                }
            }
            else
            {
                UnselectSelectedItem();
            }
        }

        public void SpawnItemFromMenuDrag(PointerEventData data, ScrollItem scrollItem)
        {
            Debug.Log("SpawnItemFromMenuDrag");
            _itemRoot.UnHighlightAll();
            UnselectSelectedItem();
            InstantiateSelectedItemOnDrag(data, scrollItem.ItemReference, OnDragInstantiateCompleted);
        }

        public void InstantiateSelectedItemOnDrag(PointerEventData data, ItemReference itemReference, System.Action<GameObject, ItemReference, PointerEventData> OnComplete)
        {
            Ray ray = Camera.main.ScreenPointToRay(data.position);
            Vector3 position = ray.GetPoint(5);
            Addressables.InstantiateAsync(itemReference.AssetPrefabName,
                    new InstantiationParameters(position, Quaternion.identity, null)) //TODO does the transform ref break this going on stack?
                .Completed += handle => OnComplete(handle.Result, itemReference, data);
        }
        
        public void InstantiateSelectedItemOnClick(PointerEventData data, System.Action<GameObject, ItemReference, PointerEventData> OnComplete)
        {
            ItemReference itemReference = _selectedItem.ItemReference;
            Addressables.InstantiateAsync(itemReference.AssetPrefabName,
                    new InstantiationParameters(data.pointerCurrentRaycast.worldPosition, Quaternion.identity, null)) //TODO does the transform ref break this going on stack?
                    .Completed += handle => OnComplete(handle.Result, itemReference, data);
        }

        public void OnDragInstantiateCompleted(GameObject gameObject, ItemReference itemReference, PointerEventData data)
        {
            data.useDragThreshold = false;
            UnselectSelectedItem();
            Item item = gameObject.GetComponent<Item>();
            item.Initialize(_itemRoot, itemReference, _itemRoot.InputCatcher);
            item.Highlight();
            SwitchStandaloneInputModule.SwitchToGameObject(item.gameObject, data);
        }

        public void LoadItemArray(IList<ItemReference> itemReferences)
        {
            float totalHeight = 0f;
            for (int i = 0; i < itemReferences.Count; ++i)
            {
                ScrollItem scrollItem;
                //if menu item already has instance, recycle it, otherwise instantiate a new one
                if (i < _scrollItemPool.Count)
                {
                    scrollItem = _scrollItemPool[i];
                    if (!scrollItem.gameObject.activeSelf)
                    {
                        scrollItem.gameObject.SetActive(true);
                    }
                }
                else
                {
                    scrollItem = Instantiate(_scrollItemPrefab);
                    scrollItem.transform.SetParent(_scrollRect.content.transform, false);
                    _scrollItemPool.Add(scrollItem);
                }
                
                scrollItem.Initialize(this, itemReferences[i], _scrollRect);

                //load preview images
                RawImage image = scrollItem.GetComponent<RawImage>();
                image.texture = itemReferences[i].PreviewImage;
                float ratio = (float) itemReferences[i].PreviewImage.height /
                              (float) itemReferences[i].PreviewImage.width;
                ((RectTransform) scrollItem.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                    scrollItem.RectTransform.rect.width * ratio);
                totalHeight += scrollItem.RectTransform.rect.height;
            }

            ((RectTransform)_scrollRect.content.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);

            //disable additional Items
            for (int i = itemReferences.Count; i < _scrollItemPool.Count; ++i)
            {
                _scrollItemPool[i].gameObject.SetActive(false);
            }
        }

        private void OnDownloadCategoryComplete(ItemReference obj)
        {
            Debug.Log("Loaded " + obj);
        }
        
        [ContextMenu("LoadItemReferencesFromAssets")]
        public async void LoadItemReferencesFromAssets(string key)
        {
            _loadHandle = Addressables.LoadAssetsAsync<ItemReference>(key, OnDownloadCategoryComplete);
            await _loadHandle.Task;
            LoadItemArray(_loadHandle.Result);
        }
    }
}