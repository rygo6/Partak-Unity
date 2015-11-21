using UnityEngine;
using System.Collections;

namespace Partak
{
	public class PlayerSettings : MonoBehaviour
	{
		public const int MaxPlayers = 4;

		public PlayerMode[] PlayerModes
		{
			get { return _playerModes; }
		}
			
		public Color[] PlayerColors
		{
			get { return _playerColors; }
		}

		public int ParticleCount
		{
			get { return _particleCount; }
		}

		[SerializeField]
		private PlayerMode[] _playerModes;

		[SerializeField]
		private int _particleCount;

		[SerializeField]
		private Color[] _playerColors;

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