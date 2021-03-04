using System;
using System.Threading.Tasks;
using GeoTetra.GTPooling;
using UnityEngine;

namespace GeoTetra.Partak
{
    [Serializable]
    public class PartakStateRef : ServiceObjectReferenceT<PartakState>
    {
        public PartakStateRef(string guid) : base(guid)
        { }
    }
    
    [CreateAssetMenu(menuName = "GeoTetra/Services/GameState")]
    public class PartakState : ServiceObject
    {
        [SerializeField] private string _version = "2.0.5";
        [SerializeField] private PlayerState[] _playerStates;
        [SerializeField] private int _timeLimitMinutes;
        [SerializeField] private int _levelIndex;
        [SerializeField] private int _editingLevelIndex;
        [SerializeField] private LevelCatalogDatum _levelCatalogDatum;
        [SerializeField] private bool _fullVersion;
        [SerializeField] private int _sessionCount;
        [SerializeField] private Texture2D _playerColorScrollTexture;
        [SerializeField] private string _lastUploadedLevelTime;
        [SerializeField] private bool _initialLevelsDownloaded;

        public const string dateFormat = "yyyy-MM-dd HH:mm:ss,fff";
        public const string ColorScrollKey = "ColorScrollX";
        private const string LevelIndexPrefKey = "LevelIndex";
        private const string FullVersionKey = "isFullVersion";
        private const string InitialLevelsDownloadedKey = "InitialLevelsDownloaded";
        
        public LevelCatalogDatum LevelCatalogDatum => _levelCatalogDatum;
        public bool FullVersion => _fullVersion;
        public int SessionCount => _sessionCount;
        public PlayerState[] PlayerStates => _playerStates;
        public string Version => _version;
        
        public int TimeLimitMinutes
        {
            get => _timeLimitMinutes;
            set => _timeLimitMinutes = value;
        }

        public int LevelIndex
        {
            get => _levelIndex;
            set
            {
                if (value >= _levelCatalogDatum.LevelIDs.Count)
                {
                    _levelIndex = 0;
                }
                else if (value < 0)
                {
                    _levelIndex = _levelCatalogDatum.LevelIDs.Count - 1;
                }
                else
                {
                    _levelIndex = value;
                }
                PlayerPrefs.SetInt(LevelIndexPrefKey, _levelIndex);
            }
        }

        public int EditingLevelIndex
        {
            get => _editingLevelIndex;
            set => _editingLevelIndex = value;
        }

        public DateTime LastUploadedLevelTime
        {
            get =>
                string.IsNullOrWhiteSpace(_lastUploadedLevelTime) ? 
                    new DateTime(0, 0, 0) : 
                    DateTime.ParseExact(_lastUploadedLevelTime, dateFormat, System.Globalization.CultureInfo.InvariantCulture);
            set
            {
                _lastUploadedLevelTime = value.ToString(dateFormat, System.Globalization.CultureInfo.InvariantCulture);
                PlayerPrefs.SetString("LastLevelUploadTime", _lastUploadedLevelTime);
            }
        }

        public bool InitialLevelsDownloaded => _initialLevelsDownloaded;

        [System.Serializable]
        public class PlayerState
        {
            public event Action<Color> ColorChanged;
            
            [SerializeField]
            private Color _playerColor;
            
            [SerializeField]
            private PlayerMode _playerMode;
            
            public Color PlayerColor
            {
                get => _playerColor;
                set
                {
                    _playerColor = value;
                    ColorChanged?.Invoke(_playerColor);
                }
            }
            
            public PlayerMode PlayerMode
            {
                get => _playerMode;
                set => _playerMode = value;
            }
        }
        
        protected override async Task OnServiceStart()
        {
            _levelIndex = PlayerPrefs.GetInt("LevelIndex", 0);

            _fullVersion = PlayerPrefs.HasKey(FullVersionKey);

            _sessionCount = PlayerPrefs.GetInt("SessionCount");
            PlayerPrefs.SetInt("SessionCount", ++_sessionCount);
            
            _lastUploadedLevelTime = PlayerPrefs.GetString("LastLevelUploadTime", new DateTime(0).ToString(dateFormat, System.Globalization.CultureInfo.InvariantCulture));
            
            switch (PlayerPrefs.GetInt("muted"))
            {
                case 1:
                    AudioListener.volume = 1f;
                    break;
                case 2:
                    AudioListener.volume = 0f;
                    break;
            }

            _levelCatalogDatum = LevelCatalogDatum.LoadLevelCatalogDatum();

            SetColors(PlayerPrefs.GetFloat(ColorScrollKey, -.125f));
            
            _initialLevelsDownloaded = PlayerPrefs.HasKey(InitialLevelsDownloadedKey);

            await base.OnServiceStart();
        }

        public void SetInitialLevelsDownloaded()
        {
            PlayerPrefs.SetInt(InitialLevelsDownloadedKey, 1);
            _initialLevelsDownloaded = true;
        }
        
        public bool AllowLevelUpload()
        {
            if (_fullVersion)
            { 
                return (DateTime.Now - LastUploadedLevelTime).TotalHours > 1;
            }
            else
            {
                return (DateTime.Now - LastUploadedLevelTime).TotalDays > 1;
            }
        }

        public void EnableFullVersion()
        {
            Debug.Log("Enabling Full Version");
            PlayerPrefs.SetInt(FullVersionKey, 1);
            _fullVersion = true;
        }

        public string GetSelectedLevelId()
        {
            if (_levelIndex == -1 && _levelCatalogDatum.LevelIDs.Count > 0) _levelIndex = 0;
            return _levelIndex > -1 && _levelIndex < _levelCatalogDatum.LevelIDs.Count && _levelCatalogDatum.LevelIDs.Count > 0 ? LevelCatalogDatum.LevelIDs[_levelIndex] : "";
        }
        
        public string GetEditingLevelId()
        {
            return _editingLevelIndex < LevelCatalogDatum.LevelIDs.Count && LevelCatalogDatum.LevelIDs.Count > 0 && _editingLevelIndex > -1 ? LevelCatalogDatum.LevelIDs[_editingLevelIndex] : Guid.NewGuid().ToString();
        }

        public void AddLevelId(string id)
        {
            if (!LevelCatalogDatum.LevelIDs.Contains(id))
            {
                if (_editingLevelIndex < LevelCatalogDatum.LevelIDs.Count)
                {
                    LevelCatalogDatum.LevelIDs[_editingLevelIndex] = id;
                }
                else
                {
                    LevelCatalogDatum.LevelIDs.Add(id);
                }
                
                LevelCatalogDatum.SaveLevelCatalogDatum();
            }
        }
        
        public void RemoveLevelId(string id)
        {
            if (LevelCatalogDatum.LevelIDs.Contains(id))
            {
                LevelCatalogDatum.LevelIDs.Remove(id);
                LevelCatalogDatum.SaveLevelCatalogDatum();
            }
        }

        public int PlayerCount()
        {
            return PlayerStates.Length;
        }

        public bool PlayerActive(int playerIndex)
        {
            return PlayerStates[playerIndex].PlayerMode != PlayerMode.None;
        }

        public int ActivePlayerCount()
        {
            int count = 0;
            for (int i = 0; i < PlayerStates.Length; ++i)
            {
                if (PlayerStates[i].PlayerMode != PlayerMode.None)
                {
                    count++;
                }
            }

            return count;
        }

        public int ActiveHumanPlayerCount()
        {
            int count = 0;
            for (int i = 0; i < PlayerStates.Length; ++i)
            {
                if (PlayerStates[i].PlayerMode == PlayerMode.Human)
                {
                    count++;
                }
            }

            return count;
        }
        
        public void SetColors(float x)
        {
            float u = 0.125f + x;
            for (int i = 0; i < PlayerCount(); ++i)
            {
                PlayerStates[i].PlayerColor = _playerColorScrollTexture.GetPixelBilinear(u, .5f);
                u += .25f;
            }
        }
    }
}