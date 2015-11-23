//#define DISABLE_PLAYERPREF
using UnityEngine;
using System.Collections;

namespace Partak
{
	public class PlayerSettings : MonoBehaviour
	{
		public const int MaxPlayers = 4;

		public Color[] PlayerColors
		{
			get { return _playerColors; }
		}

		public int ParticleCount
		{
			get { return _particleCount; }
		}
			
		public int LevelCount
		{
			get { return _levelCount; }
		}

		public int LevelIndex
		{ 	
			get { return _levelIndex; } 
			set
			{
				_levelIndex = (int)Mathf.Repeat(value, _levelCount);
				PlayerPrefs.SetInt("LevelIndex", _levelIndex);
			}
		}
		private int _levelIndex;

		[SerializeField]
		private int _levelCount;

		[SerializeField]
		private PlayerMode[] _playerModes;

		[SerializeField]
		private int _particleCount;

		[SerializeField]
		private Color[] _playerColors;

		private void Awake()
		{
#if !DISABLE_PLAYERPREF
			_levelIndex = PlayerPrefs.GetInt("LevelIndex");
			for (int i = 0; i < _playerModes.Length; ++i)
			{
				_playerModes[i] = (PlayerMode)PlayerPrefs.GetInt("PlayerMode" + i);
			}
#endif
		}

		public PlayerMode GetPlayerMode(int PlayerIndex)
		{
			return _playerModes[PlayerIndex];
		}

		public void SetPlayerMode(int playerIndex, PlayerMode playerMode)
		{
			_playerModes[playerIndex] = playerMode;
			PlayerPrefs.SetInt("PlayerMode" + playerIndex, (int)playerMode);
		}

		public bool PlayerActive(int playerIndex)
		{
			if (_playerModes[playerIndex] != PlayerMode.None)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public int ActivePlayerCount()
		{
			int count = 0;
			for (int i = 0; i < _playerModes.Length; ++i)
			{
				if (_playerModes[i] != PlayerMode.None)
				{
					count++;
				}
			}
			return count;
		}
	}
}