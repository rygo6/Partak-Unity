using System;
using System.Collections.Generic;
using GeoTetra.GTCommon.Utility;
using UnityEngine;

namespace GeoTetra.GTSnapper
{
    public class ItemGizmoRoot : MonoBehaviour
    {
        [SerializeField] private Color _highlightColor = Color.yellow;
        [SerializeField] private List<ItemMoveGizmo> _itemMoveGizmos;
        [SerializeField] private List<ItemScaleGizmo> _itemScaleGizmos;
        [SerializeField] private List<ItemRotateGizmo> _itemRotateGizmos;

        public Item TargetedItem { get; private set; }

        public Color HighlightColor => _highlightColor;

        private void Awake()
        {
            for (int i = 0; i < _itemMoveGizmos.Count; ++i)
            {
                _itemMoveGizmos[i].Initialize(this);
            }
            for (int i = 0; i < _itemScaleGizmos.Count; ++i)
            {
                _itemScaleGizmos[i].Initialize(this);
            }
            for (int i = 0; i < _itemRotateGizmos.Count; ++i)
            {
                _itemRotateGizmos[i].Initialize(this);
            }

            gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (TargetedItem != null)
            {
                transform.position = TargetedItem.transform.position;
            }
        }

        public void TargetItem(Item item)
        {
            if (item.Drag == null) return;
            
            TargetedItem = item;
            gameObject.SetActive(true);
        }
        
        public void UntargetItem()
        {
            TargetedItem = null;
            gameObject.SetActive(false);
        }
        
        public void Translate(float deltaX, float deltaY, float deltaZ)
        {
            TargetedItem.Drag.TargetTransform.Translate(deltaX, deltaY, deltaZ, Space.World);
        }

        public void Rotate(float deltaX, float deltaY, float deltaZ)
        {
            TargetedItem.Drag.TargetTransform.Rotate(deltaX, deltaY, deltaZ);
        }
        
        public void Scale(float delta)
        {
            Vector3 newScale = TargetedItem.Drag.TargetTransform.localScale;
            newScale  *= (1f + delta);
            newScale = Vector3Utility.Clamp(newScale, TargetedItem.Drag.MinScale, TargetedItem.Drag.MaxScale);
            TargetedItem.Drag.TargetTransform.localScale = newScale;
        }
    }
}