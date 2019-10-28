using UnityEngine;
using System.Collections;

namespace GeoTetra
{
    [CreateAssetMenu(menuName = "GeoTetra/ItemSettings")]
    public class ItemSettings : ScriptableObject
    {
        [SerializeField] private Color _downHighlightItemColor = Color.blue;

        [SerializeField] private Color _highlightItemColor = Color.blue;

        [SerializeField] private Color _dropOutlineColor = Color.green;

        [SerializeField] private Color _instantiateOutlineColor = Color.cyan;

        [SerializeField] private Color _deleteOutlineColor = Color.red;

        [SerializeField] private float _outlineSize = .003f;
        
        [SerializeField] private float _floatDistance = 10;

        public Color DownHighlightItemColor => _downHighlightItemColor;

        public Color HighlightItemColor => _highlightItemColor;

        public Color DropOutlineColor => _dropOutlineColor;

        public Color InstantiateOutlineColor => _instantiateOutlineColor;

        public Color DeleteOutlineColor => _deleteOutlineColor;

        public float OutlineSize => _outlineSize;

        public float FloatDistance => _floatDistance;
    }
}