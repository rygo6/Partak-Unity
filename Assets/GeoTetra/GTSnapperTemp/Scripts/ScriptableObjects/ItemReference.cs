using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeoTetra.GTSnapper.ScriptableObjects
{
    [CreateAssetMenu(menuName = "GeoTetra/ItemReference")]
    public class ItemReference : ScriptableObject
    {
        [SerializeField] private Texture _previewImage;
        [SerializeField] private string _displayName;
        [SerializeField] private string _assetPrefabName;
        [SerializeField] private string[] _tagArray;

        public string[] TagArray => _tagArray;

        public Texture PreviewImage => _previewImage;
        public string DisplayName => _displayName;
        public string AssetPrefabName => _assetPrefabName;

        public bool HasTag(string tag)
        {
            for (int i = 0; i < _tagArray.Length; ++i)
            {
                if (_tagArray[i].Equals(tag))
                {
                    return true;
                }
            }

            return false;
        }
    }
}