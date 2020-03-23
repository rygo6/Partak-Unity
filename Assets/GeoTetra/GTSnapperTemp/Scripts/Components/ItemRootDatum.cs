using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeoTetra.GTSnapper
{
    [Serializable]
    public class ItemRootDatum
    {
        public List<ItemDatum> _itemDatums = new List<ItemDatum>();
    }
    
    [Serializable]
    public class ItemDatum
    {
        public string _uniqueTick;
        public Vector3 _position;
        public Quaternion _rotation;
        public Vector3 _scale;
        public string _rootName;
        public string _referenceName;
        public string _parentItemSnap;
        public List<string> _itemSnapUniqueTicks;
    }
}