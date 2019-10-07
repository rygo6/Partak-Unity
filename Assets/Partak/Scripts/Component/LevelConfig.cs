using System;
using GeoTetra.GTCommon.ScriptableObjects;
using UnityEngine;

namespace Partak
{
    public class LevelConfig : MonoBehaviour
    {
        [SerializeField] private ComponentContainer _componentContainer;
        [SerializeField] private Bounds _levelBounds;
        [SerializeField] private Vector2Int _rootDimension = new Vector2Int(192, 192);
        [SerializeField] private int _fps = 30;
        [SerializeField] private int _particleCount = 5000;
        [SerializeField] private int _moveCycleTime = 16;
        public Bounds LevelBounds => _levelBounds;
        public Vector2Int RootDimension => _rootDimension;
        public int ParticleCount => _particleCount;
        public int MoveCycleTime => _moveCycleTime;
        
        private void Awake()
        {
            _componentContainer.RegisterComponent(this);
            Application.targetFrameRate = _fps;
        }

        private void OnDestroy()
        {
            _componentContainer.UnregisterComponent(this);
        }

        private void OnDrawSelectedGizmos()
        {
            Vector3 center = new Vector3(
                (_rootDimension.x / 2f) / 10f,
                0f,
                (_rootDimension.y / 2f) / 10f);
            Vector3 size = new Vector3(
                _rootDimension.x / 10f,
                0f,
                _rootDimension.y / 10f);

            if (_levelBounds.center != center || _levelBounds.size != size)
            {
                _levelBounds.center = center;
                _levelBounds.size = size;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(_levelBounds.center, _levelBounds.size);
        }
    }
}