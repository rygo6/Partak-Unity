using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using GeoTetra.GTPooling;
using GeoTetra.Partak;
using UnityEngine;
using UnityEngine.Analytics;

namespace GeoTetra.Partak
{
    [Serializable]
    public class GameStateReference : ServiceReferenceT<GameState>
    {
        public GameStateReference(string guid) : base(guid)
        { }
    }

    [CreateAssetMenu(menuName = "GeoTetra/Partak/Services/GameState")]
    public class GameState : ScriptableObject
    {
        [SerializeField] private string _version = "2.0.5";
        [SerializeField] private PlayerState[] _playerStates;
        [SerializeField] private int _timeLimitMinutes;
        [SerializeField] private int _levelIndex;
        [SerializeField] private int _editingLevelIndex;
        [SerializeField] private LevelCatalogDatum _levelCatalogDatum;
        [SerializeField] private bool _fullVersion;
        [SerializeField] private int _sessionCount;

        private const string LevelIndexPrefKey = "LevelIndex";
        private const string FullVersioNKey = "isFullVersion";
        
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

        private void OnEnable()
        {
            _levelIndex = PlayerPrefs.GetInt("LevelIndex", 0);
            
            if (PlayerPrefs.HasKey(FullVersioNKey))
            {
                _fullVersion = true;
            }

            _sessionCount = PlayerPrefs.GetInt("SessionCount");
            PlayerPrefs.SetInt("SessionCount", ++_sessionCount);

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
        }

        public void EnableFullVersion()
        {
            PlayerPrefs.SetInt(FullVersioNKey, 1);
            _fullVersion = true;
        }

        public string GetSelectedLevelId()
        {
            return _levelIndex < _levelCatalogDatum.LevelIDs.Count && _levelCatalogDatum.LevelIDs.Count > 0 ? LevelCatalogDatum.LevelIDs[_levelIndex] : "";
        }
        
        public string GetEditingLevelId()
        {
            return _editingLevelIndex < LevelCatalogDatum.LevelIDs.Count && LevelCatalogDatum.LevelIDs.Count > 0 && _editingLevelIndex > 0 ? LevelCatalogDatum.LevelIDs[_editingLevelIndex] : Guid.NewGuid().ToString();
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
        
//        public bool[] ActivePlayers()
//        {
//            bool[] activePlayers = new bool[PlayerStates.Length];
//            for (int i = 0; i < PlayerStates.Length; ++i)
//            {
//                activePlayers
//            }
//
//            return count;
//        }

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
    }
}