//#define DISABLE_PLAYERPREF

using UnityEngine;

namespace Partak
{
    public class MenuConfig : MonoBehaviour
    {
        public const int MaxPlayers = 4;
        
        [SerializeField]
        private Color[] _playerColors = new Color[MaxPlayers];
        
        [SerializeField]
        private PlayerMode[] _playerModes = new PlayerMode[MaxPlayers];
        
        [SerializeField]
        private IPlayerInput[] _playerInputs = new IPlayerInput[MaxPlayers];
        
        public int TimeLimitMinutes { get; set; }
        
        public int LevelIndex { get; set; }

        public IPlayerInput[] PlayerInputs => _playerInputs;

        public PlayerMode[] PlayerModes => _playerModes;

        public Color[] PlayerColors => _playerColors;

        public bool PlayerActive(int playerIndex)
        {
            return PlayerModes[playerIndex] != PlayerMode.None;
        }

        public int ActivePlayerCount()
        {
            int count = 0;
            for (int i = 0; i < PlayerModes.Length; ++i)
            {
                if (PlayerModes[i] != PlayerMode.None)
                {
                    count++;
                }
            }

            return count;
        }

        public int ActiveHumanPlayerCount()
        {
            int count = 0;
            for (int i = 0; i < PlayerModes.Length; ++i)
            {
                if (PlayerModes[i] == PlayerMode.Human)
                {
                    count++;
                }
            }

            return count;
        }
    }
}