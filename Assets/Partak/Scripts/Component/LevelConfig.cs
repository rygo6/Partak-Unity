using System;
using GeoTetra.GTCommon.Components;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using GeoTetra.GTSnapper;
using GeoTetra.Partak;
using UnityEngine;
using UnityEngine.Analytics;

namespace Partak
{
    public class LevelConfig : SubscribableBehaviour
    {
        [SerializeField] private ServiceReference _componentContainer;
        [SerializeField] private ServiceReference _gameState;
        [SerializeField] private Vector2Int _rootDimension = new Vector2Int(192, 192);
        [SerializeField] private int _fps = 60;
        [SerializeField] private int _particleCount = 5000;
        [SerializeField] private int _moveCycleTime = 16;
        [SerializeField] private ItemRoot _itemRoot;
        [SerializeField] private CameraCapture _cameraCapture;
        [SerializeField] private Camera _camera;
        [SerializeField] private bool _loadLevelOnStart;

        public Bounds LevelBounds
        {
            get
            {
                Bounds bounds = new Bounds(new Vector3((_rootDimension.x / 2f)/ 10f, 0,(_rootDimension.y / 2f) / 10f),
                    new Vector3(_rootDimension.x / 10f, 0, _rootDimension.x / 10f));
                
                return bounds;              
            }
        }

        public Vector2Int RootDimension => _rootDimension;
        public int ParticleCount => _particleCount;
        public int MoveCycleTime => _moveCycleTime;

        private LevelDatum _levelDatum;
        
        public class LevelDatum
        {
            public bool Shared;
            public Vector2Int LevelSize;
            public int ParticleCount;
            public int MoveCycleTime;
            public string ItemRootDatumJSON;
        }
        
        private void Awake()
        {
            _componentContainer.Service<ComponentContainer>().RegisterComponent(this);
            Application.targetFrameRate = _fps;
        }

        private void Start()
        {
            if (_loadLevelOnStart)
            {
                int levelIndex = _gameState.Service<GameState>().LevelIndex;
                Deserialize(levelIndex);
            }
        }

        public void Deserialize(int levelIndex)
        {
            string levelPath = LevelUtility.LevelPath(levelIndex);
            
            if (!System.IO.File.Exists(levelPath))
            {
                Debug.Log($"{levelPath} not found to load.");
                return;
            }

            string json = System.IO.File.ReadAllText(levelPath);
            _levelDatum = JsonUtility.FromJson<LevelDatum>(json);
            _itemRoot.Deserialize(_levelDatum.ItemRootDatumJSON);
        }
        
        public void Serialize(int levelIndex)
        {
            string levelPath = LevelUtility.LevelPath(levelIndex);
            
            if (_levelDatum == null)
            {
                _levelDatum = new LevelDatum();
                _levelDatum.LevelSize = new Vector2Int(192,192);
                _levelDatum.ParticleCount = 5000;
                _levelDatum.MoveCycleTime = 20;
            }

            _levelDatum.ItemRootDatumJSON = _itemRoot.Serialize();
            string json = JsonUtility.ToJson(_levelDatum);
            System.IO.File.WriteAllText(levelPath, json);
            
            string imagePath = LevelUtility.LevelImagePath(levelIndex);
            _cameraCapture.SaveScreenshotToFile(imagePath);
        }

        public void SetLevelSize(Vector2Int newSize)
        {
            _rootDimension = newSize;
            
            Vector3 cameraPos = new Vector3((newSize.x / 2f)/ 10f, SizeToZ(newSize), (newSize.y / 2f)/ 10f);
            _camera.transform.position = cameraPos;
        }

        private float SizeToZ(Vector2Int size)
        {
            if (size.x == 128 && size.y == 128)
            {
                return 19.58f;
            }

            if (size.x == 192 && size.y == 192)
            {
                return 25.56f;
            }
            
            if (size.x == 256 && size.y == 144)
            {
                return 27.84f;
            }
            
            if (size.x == 256 && size.y == 256)
            {
                return 38;
            }

            return 0;
        }
    }
}