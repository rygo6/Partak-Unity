using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Partak
{
    [CreateAssetMenu(menuName = "GeoTetra/Partak/GameState")]
    public class GameState : ScriptableObject
    {
        [SerializeField] private string _version = "2.0.5";
        [SerializeField] private PlayerState[] _playerStates;
        [SerializeField] private int _timeLimitMinutes;
        [SerializeField] private int _levelIndex;
        [SerializeField] private int _editingLevelIndex;
        
        public bool FullVersion { get; set; }
        public int SessionCount { get; private set; }

        public int TimeLimitMinutes
        {
            get => _timeLimitMinutes;
            set => _timeLimitMinutes = value;
        }

        public int LevelIndex
        {
            get => _levelIndex;
            set => _levelIndex = value;
        }
        
        public int EditingLevelIndex
        {
            get => _editingLevelIndex;
            set => _editingLevelIndex = value;
        }

        public PlayerState[] PlayerStates => _playerStates;

        public string Version => _version;

        [System.Serializable]
        public struct PlayerState
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

        private void Awake()
        {
            if (PlayerPrefs.HasKey("isFullVersion"))
            {
                Debug.Log("isFullVersion");
                FullVersion = true;
            }

            SessionCount = PlayerPrefs.GetInt("SessionCount");
            Debug.Log("SessionCount: " + SessionCount);
            PlayerPrefs.SetInt("SessionCount", ++SessionCount);

            switch (PlayerPrefs.GetInt("muted"))
            {
                case 1:
                    AudioListener.volume = 1f;
                    break;
                case 2:
                    AudioListener.volume = 0f;
                    break;
            }

//		CrashReporting.Init("ff1d2528-adf9-4ba4-bf2d-d34f2ccfe587", Version);
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

        public string EditingLevelPath()
        {
            return System.IO.Path.Combine(Application.persistentDataPath, $"level{_editingLevelIndex}");
        }
    }
}