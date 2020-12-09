// #define LOG

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GeoTetra.GTSnapper.ScriptableObjects;
using UnityEngine.AddressableAssets;
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
        [SerializeField] private ItemPartner[] _ItemPartnerArray; //This was to allow switching of Item to another.
        [SerializeField] private ItemDrag _itemDrag;
        [SerializeField] private ItemDrop _itemDrop;

        // public event Action<Item> Selected;
        
        private InputCatcher _inputCatcher;

        public ItemDrag Drag => _itemDrag;
        public ItemDrop Drop => _itemDrop;
        public ItemDatum ItemDatum { get; private set; }
        public ItemReference ItemReference { get; private set; }
//        public Material[] BlendMaterialArray { get; private set; }
//        public Material[] MaterialArray { get; private set; }
        public Mesh[] MeshArray { get; private set; }
        public MeshRenderer[] MeshRendererArray { get; private set; }
        public Transform[] MeshTransformArray { get; private set; }
        public Collider[] ColliderArray { get; private set; }
        public Vector3[] InitialColliderSizeArray { get; private set; }
        public Vector3[] InitialColliderCenterArray { get; private set; }
        public GameObject[] ColliderGameObjectArray { get; private set; }
        public ItemRoot ItemRoot { get; private set; }
        public ItemCatalogUI LastItemCatalogUUI { get; set; }
        public ItemState State { get; set; }
        public string[] TagArray => _tagArray;
        public string[] ChildTagArray => _childTagArray;
        
        private bool _drawOutline;
        private MaterialPropertyBlock _outlinePropertyBlock;

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
            _outlinePropertyBlock = new MaterialPropertyBlock();
            MeshArrayInitialize();
//            MaterialArrayInitialize();
            ColliderArrayInitialize();
        }

        private void LateUpdate()
        {
            if (_drawOutline)
            {
                for (int i = 0; i < MeshRendererArray.Length; ++i)
                {
                    Matrix4x4 matrix = Matrix4x4.TRS(MeshTransformArray[i].position, MeshTransformArray[i].rotation, MeshTransformArray[i].lossyScale);
                    Graphics.DrawMesh(MeshArray[i], matrix, ItemRoot.HighlightMaterial, 0, (Camera) null, 0, _outlinePropertyBlock, ShadowCastingMode.On, true, (Transform) null, LightProbeUsage.BlendProbes, (LightProbeProxyVolume) null);
                }
            }
        }

        private void OnDestroy()
        {
            Deinitialize();
        }

        public void Initialize(ItemRoot itemRoot, ItemReference itemReference, InputCatcher inputCatcher)
        {
            ItemRoot = itemRoot;
            ItemRoot.ItemCount++;
            transform.SetParent(ItemRoot.transform);
            ItemReference = itemReference;
            _inputCatcher = inputCatcher;

            if (_itemDrag != null)
            {
                _itemDrag.TargetTransform.SetParent(ItemRoot.transform);
            }
        }

        public void Deinitialize()
        {
            ItemRoot.ItemCount--;
            if (ItemReference != null)
            {
//                Debug.Log("Releasing " + ItemReference.AssetPrefabName);
                //These do not actually individually call their references,  it loaded from ItemCatalagUI or ItemRoot
                // Addressables.Release(ItemReference);
            }
        }

        private void OnValidate()
        {
            _itemDrag = GetComponent<ItemDrag>();
            _itemDrop = GetComponent<ItemDrop>();
        }

        public void Deserialize(ItemDatum itemDatum)
        {
            ItemDatum = itemDatum;
            transform.position = itemDatum._position;
            transform.rotation = itemDatum._rotation;
            transform.localScale = itemDatum._scale;
            if (Drag != null)
            {
                Drag.SetTargetToActualPositionDirection();
            }
            
            UniqueTick = itemDatum._uniqueTick;
            if (_itemDrop != null) _itemDrop.Deserialize(ItemDatum);
        }

        public void Serialize(List<ItemDatum> itemDatums)
        {
            if (ItemDatum == null)
            {
                ItemDatum = new ItemDatum();
                ItemDatum._uniqueTick = UniqueTick;
                //levels in the scene won't have a reference
                if (ItemReference == null) ItemDatum._rootName = gameObject.name;
                else ItemDatum._referenceName = ItemReference.name;
                if (_itemDrop != null) _itemDrop.Serialize(ItemDatum);
            }

            ItemDatum._position = transform.position;
            ItemDatum._rotation = transform.rotation;
            ItemDatum._scale = transform.localScale;
            if (_itemDrag != null) ItemDatum._parentItemSnap = _itemDrag.ParentItemSnap.UniqueTick;

            itemDatums.Add(ItemDatum);

            if (_itemDrop != null)
            {
                for (int i = 0; i < _itemDrop.ChildItemDragList.Count; ++i)
                {
                    _itemDrop.ChildItemDragList[i].Item.Serialize(itemDatums);
                }
            }
        }

        private void MeshArrayInitialize()
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
        }

//        private void MaterialArrayInitialize()
//        {
//            Renderer[] renderers = this.GetComponentsInChildren<Renderer>();
//            List<Material> blendMateriaList = new List<Material>();
//            List<Material> materiaList = new List<Material>();
//            for (int i = 0; i < renderers.Length; ++i)
//            {
//                for (int m = 0; m < renderers[i].materials.Length; ++m)
//                {
//                    if (renderers[i].materials[m].shader.ToString() ==
//                        "Mobile/VertexLit (Only Directional Lights) Blend (UnityEngine.Shader)")
//                    {
//                        blendMateriaList.Add(renderers[i].materials[m]);
//                    }
//                    else
//                    {
//                        materiaList.Add(renderers[i].materials[m]);
//                    }
//                }
//            }
//
//            BlendMaterialArray = blendMateriaList.ToArray();
//            MaterialArray = materiaList.ToArray();
//        }

        private void ColliderArrayInitialize()
        {
            ColliderArray = GetComponentsInChildren<Collider>();
            ColliderGameObjectArray = new GameObject[ColliderArray.Length];
            InitialColliderSizeArray = new Vector3[ColliderArray.Length];
            InitialColliderCenterArray = new Vector3[ColliderArray.Length];
            for (int i = 0; i < ColliderArray.Length; ++i)
            {
                ColliderArray[i].contactOffset = 1;
                ColliderGameObjectArray[i] = ColliderArray[i].gameObject;
                InitialColliderSizeArray[i] = ColliderArray[i].transform.localScale;
            }
        }

        public void OnPointerDown(PointerEventData data)
        {
#if LOG
			Debug.Log( "OnPointerDown " + this.name + " " + State);
#endif
            ItemRoot.CurrentlyUsedItem = this;

            ItemUtility.StateSwitch(data, State,
                OnPointerDownAttached,
                OnPointerDownAttachedHighlighted,
                null,
                OnPointerDownAttachedHighlighted,
                OnPointerDownInstantiate,
                OnPointerDownNoInstantiate
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
            
            SetShaderOutline(ItemRoot.ItemSettings.DownHighlightItemColor);
        }

        private void OnPointerDownInstantiate(PointerEventData data)
        {
#if LOG
			Debug.Log( "OnPointerDownInstantiate " + this.name );
#endif

            SetShaderOutline(ItemRoot.ItemSettings.DropOutlineColor);
            LastItemCatalogUUI.InstantiateSelectedItemOnClick(data, OnClickInstantiateCompleted);
        }
        
        private void OnPointerDownNoInstantiate(PointerEventData data)
        {
#if LOG
            Debug.Log( "OnPointerDownNoInstantiate " + this.name );
#endif

            SetShaderOutline(ItemRoot.ItemSettings.DownHighlightItemColor);
        }
        
        private void OnClickInstantiateCompleted(GameObject gameObject, ItemReference itemReference, PointerEventData data)
        {
            ItemRoot.UnHighlightAll(i => i.State = ItemState.NoInstantiate);

            Item item = gameObject.GetComponent<Item>();
            item.Initialize(ItemRoot, itemReference, _inputCatcher);
            
            item.Highlight();
            
            item.Drag.ThisEnteredDropItem = _itemDrop;
            item.Drag.ParentItemDrop = _itemDrop;
            item.Drag.ParentItemSnap = item.Drag.NearestItemSnap(data);
            Ray ray = item.Drag.ParentItemSnap.Snap(item, data);
            item.Drag.SetTargetPositionRotation(ray.origin, ray.direction);
            item.Drag.ThisEnteredDropItem = null;
            SwitchStandaloneInputModule.SwitchToGameObject(item.gameObject, data);
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
                OnPointerUpAttachedHighlighted,
                OnPointerUpInstantiate,
                OnPointerUpNoInstantiate
            );
            
            ItemRoot.CurrentlyUsedItem = null;
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

            SetLayerRecursive(ItemRoot.ItemLayer);
            ItemRoot.UnHighlightAll();
        }

        private void OnPointerUpDragging(PointerEventData data)
        {
#if LOG
			Debug.Log( "OnPointerUpDragging " + this.name );
#endif
        }

        private void OnPointerUpInstantiate(PointerEventData data)
        {
#if LOG
			Debug.Log( "OnPointerUpInstantiate " + this.name );
#endif
        }
        
        private void OnPointerUpNoInstantiate(PointerEventData data)
        {
#if LOG
            Debug.Log( "OnPointerUpInstantiate " + this.name );
#endif
            //if you click on a non-instantiable item, exit instantiate mode and highlight clicked item to start moving it.
            if (data.pointerCurrentRaycast.gameObject == data.pointerPressRaycast.gameObject)
            {
                ItemRoot.InputCatcher.OnPointerClick(data);
                Highlight();
            }
            else
            {
                SetShaderNormal();
            }
        }

        public IEnumerator DestroyItemCoroutine()
        {
            UnHighlight();
            SetShaderOutline(Color.red);
            
            if (Drop != null)
            {
                for (int i = 0; i < Drop.ChildItemDragList.Count; ++i)
                {
                    StartCoroutine(Drop.ChildItemDragList[i].GetComponent<Item>().DestroyItemCoroutine());
                }
            }

            yield return new WaitForSeconds(.4f);

            RemoveUniqueTickRecursive(this);
            Destroy(GetComponent<ItemDrag>().TargetTransform.gameObject);
            Addressables.ReleaseInstance(gameObject);
//            Destroy(gameObject);

            //TODO use Resources.UnloadAsset and find all assets to do this faster
            Resources.UnloadUnusedAssets(); //TODO use pooling
            System.GC.Collect();
        }

        private void RemoveUniqueTickRecursive(Item item)
        {
            ItemRoot.UniqueTickDictionary.Remove(item.UniqueTick);
            if (item.Drop != null)
            {
                for (int i = 0; i < item.Drop.ItemSnapArray.Length; ++i)
                {
                    ItemRoot.UniqueTickDictionary.Remove(item.Drop.ItemSnapArray[i].UniqueTick);
                }
            }
            
            if (item.Drop != null)
            {
                for (int i = 0; i < item.Drop.ChildItemDragList.Count; ++i)
                {
                    RemoveUniqueTickRecursive(item.Drop.ChildItemDragList[i].Item);
                }
            }
        }

        public void Highlight()
        {
            SetShaderOutline(ItemRoot.ItemSettings.HighlightItemColor);
            State = ItemState.AttachedHighlighted;
            ItemRoot.Highlight(this);
        }

        public void UnHighlight()
        {
            if (_itemDrag != null)
            {
                _itemDrag.SetActualPositionRotationToTarget();
            }

            SetShaderNormal();
            State = ItemState.Attached;
            ItemRoot.Unhighlight(this);
        }

        public void SetBlendMaterial(Texture texture)
        {
//            for (int i = 0; i < BlendMaterialArray.Length; ++i)
//            {
//                BlendMaterialArray[i].SetTexture("_Blend", texture);
//                //for some reason shader needs to be reapplied some times to get texture to update
//                BlendMaterialArray[i].shader = Shader.Find("Mobile/VertexLit (Only Directional Lights) Blend");
//            }
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
        private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");

        public void ResetColliderSize()
        {
            for (int i = 0; i < ColliderArray.Length; ++i)
            {
                ColliderArray[i].transform.localScale = InitialColliderSizeArray[i];
            }
        }

        public void SetShaderOutline(Color color)
        {
            if (_outlinePropertyBlock.GetColor(OutlineColor) != color && !_disableOutline)
            {
                _drawOutline = true;
                _outlinePropertyBlock.SetColor(OutlineColor, color);
                
//                for (int i = 0; i < BlendMaterialArray.Length; ++i)
//                {
//                    BlendMaterialArray[i].shader = Shader.Find("GeoTetra/VertexLightedBlendOutline");
//                    BlendMaterialArray[i].color = Color.white;
//                    BlendMaterialArray[i].SetFloat("_Outline", ItemRoot.ItemSettings.OutlineSize);
//                    BlendMaterialArray[i].SetColor("_OutlineColor", color);
//                }
//
//                for (int i = 0; i < MaterialArray.Length; ++i)
//                {
//                    MaterialArray[i].shader = Shader.Find("GeoTetra/VertexLightedBlendOutline");
//                    MaterialArray[i].color = Color.white;
//                    MaterialArray[i].SetFloat("_Outline", ItemRoot.ItemSettings.OutlineSize);
//                    MaterialArray[i].SetColor("_OutlineColor", color);
//                }
            }
        }

        public void SetShaderNormal()
        {
            _drawOutline = false;
            _outlinePropertyBlock.SetColor(OutlineColor, Color.clear);
//                if (BlendMaterialArray != null)
//                {
//                    for (int i = 0; i < BlendMaterialArray.Length; ++i)
//                    {
//                        BlendMaterialArray[i].shader = Shader.Find("Mobile/VertexLit (Only Directional Lights) Blend");
//                    }
//                }
//
//                if (MaterialArray != null)
//                {
//                    for (int i = 0; i < MaterialArray.Length; ++i)
//                    {
//                        MaterialArray[i].shader = Shader.Find("Mobile/VertexLit (Only Directional Lights)");
//                    }
        }
    }
}