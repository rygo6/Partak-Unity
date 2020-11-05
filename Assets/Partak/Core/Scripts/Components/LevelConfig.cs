using System;
using GeoTetra.GTCommon.Components;
using GeoTetra.GTPooling;
using GeoTetra.GTSnapper;
using UnityEngine;

namespace GeoTetra.Partak
{
    public class LevelConfig : SubscribableBehaviour
    {
        [SerializeField] 
        [AssetReferenceComponentRestriction(typeof(ComponentContainer))]
        private ComponentContainerReference _componentContainer;
        
        [SerializeField] 
        [AssetReferenceComponentRestriction(typeof(GameState))]
        private GameStateReference _gameState;
        
        [SerializeField] private int _fps = 60;
        [SerializeField] private bool _deserializeLevelOnStart;
        [SerializeField] private ItemRoot _itemRoot;
        [SerializeField] private CameraCapture _cameraCapture;
        [SerializeField] private Camera _camera;
        [SerializeField] private GameObject[] _backgrounds;
        [SerializeField] private Item _dropItem;

        public event Action LevelDeserialized;
        public event Action SizeChanged;
        
        public Bounds LevelBounds
        {
            get
            {
                Bounds bounds = new Bounds(
                    new Vector3((_levelDatum.LevelSize.x / 2f)/ 10f, 0,(_levelDatum.LevelSize.y / 2f) / 10f),
                    new Vector3(_levelDatum.LevelSize.x / 10f, 0, _levelDatum.LevelSize.y / 10f));
                
                return bounds;              
            }
        }

        public LocalLevelDatum Datum
        {
            get
            {
                if (_levelDatum == null)
                {
                    _levelDatum = new LocalLevelDatum();
                    _levelDatum.MoveCycleTime = 16;
                }
                return _levelDatum;
            }
        }

        private LocalLevelDatum _levelDatum;

        private void Awake()
        {
            _componentContainer.Service.RegisterComponent(this);
            Application.targetFrameRate = _fps;
            if (_itemRoot != null)
            {
                _itemRoot.DeserializationComplete += () =>
                {
                    LevelDeserialized?.Invoke();
                };
            }
        }

        private void Start()
        {
            if (_deserializeLevelOnStart)
            {
                string levelId = _gameState.Service.GetSelectedLevelId();
                Deserialize(levelId, false);
            }
        }

        public void Deserialize(string levelId, bool editing)
        {
            _levelDatum = LevelUtility.GetLevelDatum(levelId);
            SetLevelSize(_levelDatum.LevelSize, editing);
            _itemRoot.Deserialize(_levelDatum.ItemRootDatumJSON);
        }
        
        /// <summary>
        /// 
        /// </summary>
        public void Serialize(string levelId, bool saveScreenshot)
        {
            string levelPath = LevelUtility.LevelPath(levelId);

            Datum.ItemRootDatumJSON = _itemRoot.Serialize();
            Datum.LevelID = levelId;
            string json = JsonUtility.ToJson(Datum);
            System.IO.File.WriteAllText(levelPath, json);

            if (saveScreenshot)
            {
                string imagePath = LevelUtility.LevelImagePath(levelId);
                _cameraCapture.SaveScreenshotToFile(imagePath);
            }
        }

        public void SetLevelSize(Vector2Int newSize, bool editing)
        {
            Datum.LevelSize = newSize;
            Datum.ParticleCount = SizeToParticleCount(newSize);
            
            Vector3 cameraPos = new Vector3((newSize.x / 2f)/ 10f, SizeToZ(newSize, editing), (newSize.y / 2f)/ 10f);
            _camera.transform.position = cameraPos;
            
            Vector3 dropItemPos = new Vector3((newSize.x / 2f)/ 10f, 0, (newSize.y / 2f)/ 10f);
            Vector3 dropItemScale = new Vector3(newSize.x / 10f, 0, newSize.y / 10f);

            _dropItem.transform.position = dropItemPos;
            _dropItem.transform.localScale = dropItemScale;
            
            _backgrounds[0].gameObject.SetActive(newSize.x == 128 && newSize.y == 128);
            _backgrounds[1].gameObject.SetActive(newSize.x == 192 && newSize.y == 192);
            _backgrounds[2].gameObject.SetActive(newSize.x == 256 && newSize.y == 144);
            _backgrounds[3].gameObject.SetActive(newSize.x == 256 && newSize.y == 256);
            
            SizeChanged?.Invoke();
        }

        /// <summary>
        /// Hard coded cause I dunno what math does this.
        /// </summary>
        private float SizeToZ(Vector2Int size, bool editing)
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
                return editing ? 35.21f : 27.84f;
            }
            
            if (size.x == 256 && size.y == 256)
            {
                return 38;
            }

            return 0;
        }
        
        private int SizeToParticleCount(Vector2Int size)
        {
            if (size.x == 128 && size.y == 128)
            {
                return 3000;
            }

            if (size.x == 192 && size.y == 192)
            {
                return 5000;
            }
            
            if (size.x == 256 && size.y == 144)
            {
                return 5000;
            }
            
            if (size.x == 256 && size.y == 256)
            {
                return 6000;
            }

            return 0;
        }
    }
}