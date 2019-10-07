using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Partak
{
    [CreateAssetMenu]
    public class LevelState : ScriptableObject
    {
        [SerializeField] private Vector2Int _levelDimensions;
        [SerializeField] private List<LevelObject> _levelObjects;
        
        [Serializable]
        public struct LevelObject
        {
            public string Name;
            public Vector2 Position;
        }
    }
}