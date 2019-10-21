//#define LOG

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GeoTetra;
using GeoTetra.GTSnapper;
using GeoTetra.GTSnapper.ScriptableObjects;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

namespace GeoTetra.GTSnapper
{
    public class Item : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        [SerializeField] private bool _disableOutline;
        [SerializeField] private Vector3 _colliderExpand = new Vector3(1.0f, 1.0f, 1.0f);
        [SerializeField] private string[] _tagArray;
        [SerializeField] private string[] _childTagArray;
        [SerializeField] private ItemPartner[] _ItemPartnerArray;
        [SerializeField] private ItemDrag _itemDrag;
        [SerializeField] private ItemDrop _itemDrop;

        private Catcher _catcher;
        
        public ItemDrag Drag => _itemDrag;

        public ItemDrop Drop => _itemDrop;

        public ItemReference ItemReference { get; private set; }

        public Material[] BlendMaterialArray { get; private set; }

        public Material[] MaterialArray { get; private set; }

        public Mesh[] MeshArray { get; private set; }

        public MeshRenderer[] MeshRendererArray { get; private set; }

        public Transform[] MeshTransformArray { get; private set; }

        public Collider[] ColliderArray { get; private set; }

        public Vector3[] InitialColliderSizeArray { get; private set; }

        public Vector3[] InitialColliderCenterArray { get; private set; }

        public GameObject[] ColliderGameObjectArray { get; private set; }

        public ItemRoot ItemRoot { get; private set; }

        public ItemState State { get; set; }
        
        public string[] TagArray => _tagArray;
        
        public string[] ChildTagArray => _childTagArray;

        public string UniqueTick
        {
            get
            {
                if (string.IsNullOrEmpty(_uniqueTick))
                {
                    _uniqueTick = ItemUtility.TickToString();
                    ItemRoot.UniqueTickDictionary.Add(_uniqueTick, this);
                }

                return _uniqueTick;
            }
            set
            {
                if (!string.IsNullOrEmpty(_uniqueTick))
                {
                    ItemRoot.UniqueTickDictionary.Remove(_uniqueTick);
                }

                _uniqueTick = value;
                ItemRoot.UniqueTickDictionary.Add(_uniqueTick, this);
            }
        }
        
        private string _uniqueTick;

        private void Awake()
        {
            MeshRendererArray = GetComponentsInChildren<MeshRenderer>();

            MeshArray = new Mesh[MeshRendererArray.Length];
            for (int i = 0; i < MeshArray.Length; ++i)
            {
                MeshArray[i] = MeshRendererArray[i].GetComponent<MeshFilter>().sharedMesh;
            }

            MeshTransformArray = new Transform[MeshRendererArray.Length];
            for (int i = 0; i < MeshTransformArray.Length; ++i)
            {
                MeshTransformArray[i] = MeshRendererArray[i].GetComponent<Transform>();
            }

            MaterialArrayInitialize();
            ColliderArrayInitialize();
        }

        public void Initialize(Vector3 position, ItemRoot itemRoot, ItemReference itemReference, Catcher catcher)
        {
            ItemRoot = itemRoot;
            ItemReference = itemReference;
            _catcher = catcher;
        }

        private void Start()
        {
            //for debug, should be set through initialize
            if (ItemRoot == null) ItemRoot = FindObjectOfType<ItemRoot>();
            if (_catcher == null) _catcher = FindObjectOfType<Catcher>();
        }

        private void MaterialArrayInitialize()
        {
            Renderer[] renderers = this.GetComponentsInChildren<Renderer>();
            List<Material> blendMateriaList = new List<Material>();
            List<Material> materiaList = new List<Material>();
            for (int i = 0; i < renderers.Length; ++i)
            {
                for (int m = 0; m < renderers[i].materials.Length; ++m)
                {
                    if (renderers[i].materials[m].shader.ToString() ==
                        "Mobile/VertexLit (Only Directional Lights) Blend (UnityEngine.Shader)")
                    {
                        blendMateriaList.Add(renderers[i].materials[m]);
                    }
                    else
                    {
                        materiaList.Add(renderers[i].materials[m]);
                    }
                }
            }

            BlendMaterialArray = blendMateriaList.ToArray();
            MaterialArray = materiaList.ToArray();
        }

        private void ColliderArrayInitialize()
        {
            ColliderArray = GetComponentsInChildren<Collider>();
            ColliderGameObjectArray = new GameObject[ColliderArray.Length];
            InitialColliderSizeArray = new Vector3[ColliderArray.Length];
            InitialColliderCenterArray = new Vector3[ColliderArray.Length];
            for (int i = 0; i < ColliderGameObjectArray.Length; ++i)
            {
                ColliderGameObjectArray[i] = ColliderArray[i].gameObject;
                InitialColliderSizeArray[i] = ColliderArray[i].transform.localScale;
            }
        }

        public void OnPointerDown(PointerEventData data)
        {
#if LOG
			Debug.Log( "OnPointerDown " + this.name );
#endif

            ItemUtility.StateSwitch(data, State,
                OnPointerDownAttached,
                OnPointerDownAttachedHighlighted,
                null,
                null,
                OnPointerDownInstantiate,
                null
            );
        }

        private void OnPointerDownAttached(PointerEventData data)
        {
#if LOG
			Debug.Log( "OnPointerDownAttached " + this.name );
#endif

            SetShaderOutline(ItemRoot.ItemSettings.DownHighlightItemColor);
        }

        private void OnPointerDownAttachedHighlighted(PointerEventData data)
        {
#if LOG
			Debug.Log( "OnPointerDownAttachedHighlighted " + this.name );
#endif

            SetLayerRecursive(2);
            SetShaderOutline(ItemRoot.ItemSettings.DownHighlightItemColor);
        }

        private void OnPointerDownInstantiate(PointerEventData data)
        {
#if LOG
			Debug.Log( "OnPointerDownAttachedHighlighted " + this.name );
#endif

            SetShaderOutline(ItemRoot.ItemSettings.DropOutlineColor);
        }

        public void OnPointerUp(PointerEventData data)
        {
#if LOG
			Debug.Log( "OnPointerUp " + this.name );
#endif

            ItemUtility.StateSwitch(data, State,
                OnPointerUpAttached,
                OnPointerUpAttachedHighlighted,
                OnPointerUpDragging,
                null,
                OnPointerUpInstantiate,
                null
            );
        }

        private void OnPointerUpAttached(PointerEventData data)
        {
#if LOG
			Debug.Log( "OnPointerUpAttached " + this.name );
#endif

            if (data.pointerCurrentRaycast.gameObject == data.pointerPressRaycast.gameObject)
            {
                ItemRoot.UnHighlightAll();
                Highlight();
            }
            else
            {
                SetShaderNormal();
            }
        }

        private void OnPointerUpAttachedHighlighted(PointerEventData data)
        {
#if LOG
			Debug.Log( "OnPointerUpAttachedHighlighted " + this.name );
#endif

            SetLayerRecursive(0);
            ItemRoot.UnHighlightAll();
        }

        private void OnPointerUpDragging(PointerEventData data)
        {
#if LOG
			Debug.Log( "OnPointerUpAttachedHighlighted " + this.name );
#endif
        }

        private void OnPointerUpInstantiate(PointerEventData data)
        {
#if LOG
			Debug.Log( "OnPointerUpAttachedHighlighted " + this.name );
#endif

            ItemCatalogUI plannerUI = FindObjectOfType<ItemCatalogUI>();
			plannerUI.InstantiateSelectedItem(data, OnClickInstantiateCompleted);
        }
        
        private void OnClickInstantiateCompleted(GameObject gameObject, ItemReference itemReference, PointerEventData data)
        {
            Item instantiatedItem = gameObject.GetComponent<Item>();
            ItemDrag instantiatedItemDrag = gameObject.GetComponent<ItemDrag>();
            instantiatedItem.Initialize(data.pointerCurrentRaycast.worldPosition, ItemRoot, itemReference, _catcher);
            if (_itemDrop != null)
            {
                instantiatedItem._itemDrag.ThisEnteredDropItem = _itemDrop;
                instantiatedItem._itemDrag.ParentItemDrop = _itemDrop;
		
                ItemSnap itemSnap = instantiatedItemDrag.NearestItemSnap(data);
                instantiatedItemDrag.ParentItemSnap = itemSnap;
                Ray ray = itemSnap.Snap(instantiatedItem, data);
                instantiatedItemDrag.SetTargetPositionRotation(ray.origin, ray.direction); 		
                //set to outline and normal to get rid of quirk where instantied shader isn't immediately properly lit
                instantiatedItem.SetShaderOutline(ItemRoot.ItemSettings.InstantiateOutlineColor);
                instantiatedItem.SetShaderNormal();
                instantiatedItem.State = ItemState.NoInstantiate;

                //TODO this should always be able to attach, why are we checking?
                if (_itemDrop.CanAttach(instantiatedItem.TagArray))
                {
                    SetShaderOutline(ItemRoot.ItemSettings.InstantiateOutlineColor);
                }
                else
                {
                    SetShaderNormal();
                    State = ItemState.NoInstantiate;
                }
            }
//            ItemColor itemColor = GetComponent<ItemColor>();
//            if (itemColor != null)
//            {
//                Item item = GetComponent<Item>();
//                item.SetBlendMaterial(instantiatedItem.MaterialArray[0].mainTexture);
//                SetShaderNormal();
//                State = ItemState.NoInstantiate;
//                StartCoroutine(instantiatedItem.DestroyItemCoroutine());
//            }
        }

        public IEnumerator DestroyItemCoroutine()
        {
            UnHighlight();
            SetShaderOutline(Color.red);

            ItemDrop dropItem = GetComponent<ItemDrop>();
            if (dropItem != null)
            {
                for (int i = 0; i < dropItem.ChildItemDragList.Count; ++i)
                {
                    StartCoroutine(dropItem.ChildItemDragList[i].GetComponent<Item>().DestroyItemCoroutine());
                }
            }

            yield return new WaitForSeconds(.4f);

            RemoveUniqueTickRecursive(this);
            Destroy(GetComponent<ItemDrag>().TargetTransform.gameObject);
            Destroy(gameObject);

            //TODO use Resources.UnloadAsset and find all assets to do this faster
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }

        private void RemoveUniqueTickRecursive(Item item)
        {
            ItemRoot.UniqueTickDictionary.Remove(item.UniqueTick);
            ItemDrop item_Drop = item.GetComponent<ItemDrop>();
            if (item_Drop != null)
            {
                for (int i = 0; i < item_Drop.ItemSnapArray.Length; ++i)
                {
                    ItemRoot.UniqueTickDictionary.Remove(item_Drop.ItemSnapArray[i].UniqueTick);
                }
            }

            ItemDrop itemDrop = item.GetComponent<ItemDrop>(); //TODO WHY IS THIS TWICE??
            if (itemDrop != null)
            {
                for (int i = 0; i < itemDrop.ChildItemDragList.Count; ++i)
                {
                    RemoveUniqueTickRecursive(itemDrop.ChildItemDragList[i].GetComponent<Item>());
                }
            }
        }

        public void Highlight()
        {
            System.Action action = delegate() { ItemRoot.UnHighlightAll(); };
            _catcher.EmptyClickAction = action;

            SetShaderOutline(ItemRoot.ItemSettings.HighlightItemColor);
            State = ItemState.AttachedHighlighted;
            ItemRoot.ItemHighlightList.Add(this);
        }

        public void UnHighlight()
        {
            ItemDrag dragItem = GetComponent<ItemDrag>();
            if (dragItem != null)
            {
                dragItem.SetActualPositionRotationToTarget();
            }

            SetShaderNormal();
            State = ItemState.Attached;
            ItemRoot.ItemHighlightList.Remove(this);
        }

        public void SetBlendMaterial(Texture texture)
        {
            for (int i = 0; i < BlendMaterialArray.Length; ++i)
            {
                BlendMaterialArray[i].SetTexture("_Blend", texture);
                //for some reason shader needs to be reapplied some times to get texture to update
                BlendMaterialArray[i].shader = Shader.Find("Mobile/VertexLit (Only Directional Lights) Blend");
            }
        }

        public void SetLayerRecursive(int layer)
        {
            for (int i = 0; i < ColliderGameObjectArray.Length; i++)
            {
                ColliderGameObjectArray[i].layer = layer;
            }

            ItemDrop dropItem = GetComponent<ItemDrop>();
            if (dropItem != null)
            {
                for (int i = 0; i < dropItem.ChildItemDragList.Count; i++)
                {
                    dropItem.ChildItemDragList[i].GetComponent<Item>().SetLayerRecursive(layer);
                }
            }
        }

        public bool TestTagArrays(string[] firstArray, string[] secondArray)
        {
            for (int i = 0; i < firstArray.Length; ++i)
            {
                for (int o = 0; o < secondArray.Length; ++o)
                {
                    if (firstArray[i] == secondArray[o])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool HasTag(string tag)
        {
            for (int i = 0; i < _tagArray.Length; ++i)
            {
                if (_tagArray[i] == tag)
                {
                    return true;
                }
            }

            return false;
        }

        private void ExpandColliderSize()
        {
            if (_unExpandColliderSize == null)
            {
                _unExpandColliderSize = new Vector3[ColliderArray.Length];
            }

            for (int i = 0; i < ColliderArray.Length; ++i)
            {
                _unExpandColliderSize[i] = ColliderArray[i].transform.localScale;
                ColliderArray[i].transform.localScale =
                    Vector3.Scale(ColliderArray[i].transform.localScale, _colliderExpand);
            }
        }

        private void UnExpandColliderSize()
        {
            for (int i = 0; i < ColliderArray.Length; ++i)
            {
                ColliderArray[i].transform.localScale = _unExpandColliderSize[i];
            }
        }

        private Vector3[] _unExpandColliderSize;

        public void ResetColliderSize()
        {
            for (int i = 0; i < ColliderArray.Length; ++i)
            {
                ColliderArray[i].transform.localScale = InitialColliderSizeArray[i];
            }
        }

        public void SetShaderOutline(Color color)
        {
            if (_currentOutlineColor != color && !_disableOutline)
            {
                _currentOutlineColor = color;
                _outlineNormal = false;
                for (int i = 0; i < BlendMaterialArray.Length; ++i)
                {
                    BlendMaterialArray[i].shader = Shader.Find("GeoTetra/VertexLightedBlendOutline");
                    BlendMaterialArray[i].color = Color.white;
                    BlendMaterialArray[i].SetFloat("_Outline", ItemRoot.ItemSettings.OutlineSize);
                    BlendMaterialArray[i].SetColor("_OutlineColor", color);
                }

                for (int i = 0; i < MaterialArray.Length; ++i)
                {
                    MaterialArray[i].shader = Shader.Find("GeoTetra/VertexLightedBlendOutline");
                    MaterialArray[i].color = Color.white;
                    MaterialArray[i].SetFloat("_Outline", ItemRoot.ItemSettings.OutlineSize);
                    MaterialArray[i].SetColor("_OutlineColor", color);
                }
            }
        }

        public void SetShaderNormal()
        {
            if (!_outlineNormal)
            {
                _outlineNormal = true;
                _currentOutlineColor = Color.clear;
                if (BlendMaterialArray != null)
                {
                    for (int i = 0; i < BlendMaterialArray.Length; ++i)
                    {
                        BlendMaterialArray[i].shader = Shader.Find("Mobile/VertexLit (Only Directional Lights) Blend");
                    }
                }

                if (MaterialArray != null)
                {
                    for (int i = 0; i < MaterialArray.Length; ++i)
                    {
                        MaterialArray[i].shader = Shader.Find("Mobile/VertexLit (Only Directional Lights)");
                    }
                }
            }
        }

        private bool _outlineNormal = true;
        private Color _currentOutlineColor;

        public void AddToHoldList()
        {
            ItemRoot.ItemHoldList.Add(this);
        }

        public void RemoveFromHoldList()
        {
            ItemRoot.ItemHoldList.Remove(this);
        }
    }
}