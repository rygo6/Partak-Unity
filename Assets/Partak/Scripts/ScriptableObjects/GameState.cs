using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Partak
{
    [CreateAssetMenu]
    public class GameState : ScriptableObject
    {
        [SerializeField] private PlayerState[] _playerStates;
        [SerializeField] private int _timeLimitMinutes;
        [SerializeField] private int _levelIndex;

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

        public PlayerState[] PlayerStates => _playerStates;

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
    }
}