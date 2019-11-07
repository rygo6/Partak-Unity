using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeoTetra.GTSnapper
{
    [Serializable]
    public class ItemRootDatum
    {
        public string rootName;
        public long dateCreated;
        public List<ItemDatum> ItemDatums = new List<ItemDatum>();
    }
    
    [Serializable]
    public class ItemDatum
    {
        public string uniqueTick;
        public Vector3 position;
        public Quaternion rotation;
        public string rootName;
        public string referenceName;
        public string parentItemSnap;
        public List<string> ItemSnapUniqueTicks;
    }
}