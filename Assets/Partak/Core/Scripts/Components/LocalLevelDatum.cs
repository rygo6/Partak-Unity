﻿using System;
using GeoTetra.GTCommon.Components;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using GeoTetra.GTSnapper;
using GeoTetra.Partak;
using UnityEngine;
using UnityEngine.Analytics;

namespace GeoTetra.Partak
{
    public class LocalLevelDatum
    {
        public bool Shared;
        public bool Downloaded;
        public bool Rated;
        
        /// <summary>
        /// id key in dynamo
        /// </summary>
        public string LevelID;
        public Vector2Int LevelSize;
        public int ParticleCount;
        public int MoveCycleTime;
        public int ThumbsUp;
        public int ThumbsDown;
        public string ItemRootDatumJSON;
    }
}