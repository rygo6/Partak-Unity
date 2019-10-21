using UnityEngine;
using System.Collections;

namespace GeoTetra.GTSnapper
{
    /// <summary>
    /// Specify alternative point for ItemDrag to attach to.
    /// Should be on a GameObject which is a child of an ItemDrag
    /// </summary>
    public class AttachPoint : MonoBehaviour
    {
        public string[] TagArray
        {
            get => tagArray;
            set => tagArray = value;
        }

        [SerializeField] private string[] tagArray;
    }
}