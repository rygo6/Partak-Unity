using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using GeoTetra;
using GeoTetra.GTSnapper;
using GeoTetra.GTSnapper.ScriptableObjects;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace GeoTetra.GTSnapper
{
    public class ItemCatalogUI : MonoBehaviour
    {
        [SerializeField] private RectTransform _scrollbarContent;
        [SerializeField] private ScrollItem _scrollItemPrefab;
        [SerializeField] private Canvas _scrollItemHighlight;
        [SerializeField] private ItemSettings _itemSettings;
        
        private readonly List<ScrollItem> _scrollItemPool = new List<ScrollItem>();

        private ItemRoot ItemRoot { get; set; }

        public RectTransform ScrollbarContent => _scrollbarContent;
        private Catcher _catcher;
        private ScrollItem _selectedItem;
        private IList<ItemReference> _itemArray;

        private void Start()
        {
            ItemRoot = FindObjectOfType<ItemRoot>();
            _catcher = FindObjectOfType<Catcher>();
            LoadItemReferencesFromAssets("ItemReference");
            _scrollItemHighlight.enabled = false;
        }

        public void UnselectSelectedItem()
        {
            if (_selectedItem != null)
            {
                ItemRoot.SetAllOutlineNormalAttach();
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

                System.Action<Item> trueAction = delegate(Item item)
                {
                    item.SetShaderOutline(_itemSettings.InstantiateOutlineColor);
                    item.State = ItemState.Instantiate;
                    item.LastItemCatalogUUI = this;
                };
                System.Action<Item> falseAction = delegate(Item item)
                {
                    item.SetShaderNormal();
                    item.State = ItemState.NoInstantiate;
                    item.LastItemCatalogUUI = null;
                };
                System.Func<Item, bool> filterAction = delegate(Item item)
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
                };

                int trueCount = ItemRoot.CallDelegateTagFilter(filterAction, trueAction, falseAction);

                if (trueCount == 0)
                {
                    ItemRoot.SetAllOutlineNormalAttach();
                    _selectedItem = null;
                }
                else
                {
                    System.Action action = delegate() { UnselectSelectedItem(); };
                    _catcher.EmptyClickAction = action;

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
            Debug.Log(data.delta);
            ItemRoot.UnHighlightAll();
            _selectedItem = scrollItem;
            InstantiateSelectedItemOnDrag(data, OnDragInstantiateCompleted);
            UnselectSelectedItem();
        }

        public void InstantiateSelectedItemOnDrag(PointerEventData data, System.Action<GameObject, ItemReference, PointerEventData> OnComplete)
        {
            Ray ray = Camera.main.ScreenPointToRay(data.position);
            Vector3 position = ray.GetPoint(3.5f);
            ItemReference itemReference = _selectedItem.ItemReference;
            Debug.Log(data.delta);
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
            Debug.Log(data.delta);
            UnselectSelectedItem();
            Item item = gameObject.GetComponent<Item>();
            item.Initialize(item.transform.position, ItemRoot, itemReference, _catcher);
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
                    scrollItem.transform.SetParent(_scrollbarContent.GetComponent<RectTransform>(), false);
                    _scrollItemPool.Add(scrollItem);
                    _scrollItemPool[i].GetComponent<ScrollItem>().Initialize(this, itemReferences[i]);
                }

                if (itemReferences[i].PreviewImage != null)
                {
                    Resources.UnloadAsset(itemReferences[i].PreviewImage);
                }

                //load preview images
                RawImage image = scrollItem.GetComponent<RawImage>();
                image.texture = itemReferences[i].PreviewImage;
                float ratio = (float) itemReferences[i].PreviewImage.height /
                              (float) itemReferences[i].PreviewImage.width;
                ((RectTransform) scrollItem.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                    scrollItem.RectTransform.rect.width * ratio);
                totalHeight += scrollItem.RectTransform.rect.height;
            }

            _scrollbarContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);

            //disable additional Items
            for (int i = itemReferences.Count; i < _scrollItemPool.Count; ++i)
            {
                _scrollItemPool[i].gameObject.SetActive(false);
            }

            Resources.UnloadUnusedAssets();
        }

        private void OnDownloadCategoryComplete(ItemReference obj)
        {
            Debug.Log("Loaded " + obj);
        }

        private void OnDownloadComplete(AsyncOperationHandle<IList<ItemReference>> listAssets)
        {
            _itemArray = listAssets.Result;
            LoadItemArray(_itemArray);
        }

        [ContextMenu("LoadItemReferencesFromAssets")]
        public void LoadItemReferencesFromAssets(string key)
        {
            Addressables.LoadAssetsAsync<ItemReference>(key, OnDownloadCategoryComplete).Completed +=
                OnDownloadComplete;
        }
    }
}