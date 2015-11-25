using UnityEngine;
using System;
using System.Collections;

namespace Partak
{
	public class CellParticleStore : MonoBehaviour
	{
		public CellParticle[] CellParticleArray { get; private set; }

		private readonly int[] PlayerParticleCount = new int[PlayerSettings.MaxPlayers];

		/// <summary> Keeps track of which players have lost. </summary>
		public readonly bool[] PlayerLose = new bool[PlayerSettings.MaxPlayers];

		[SerializeField]
		private CursorStore _cursorStore;

		private PlayerSettings _playerSettings;

		private int _startParticleCount;

		private bool _recalculatePercentages;

		private bool _update;

		public event Action WinEvent;

		public event Action<int> LoseEvent;

		private void Awake()
		{
			_playerSettings = Persistent.Get<PlayerSettings>();
			CellParticleArray = new CellParticle[Persistent.Get<PlayerSettings>().ParticleCount];
			_startParticleCount = _playerSettings.ParticleCount / _playerSettings.ActivePlayerCount();

			FindObjectOfType<CellParticleSpawn>().SpawnComplete += () => { _update = true; };
		}

		private void LateUpdate()
		{
			if (_recalculatePercentages && _update)
			{
				_recalculatePercentages = false;
				CalculatePercentages();
			}
		}

		public void IncrementPlayerParticleCount(int playerIndex)
		{
			PlayerParticleCount[playerIndex]++;
			_recalculatePercentages = true;
		}

		public void DecrementPlayerParticleCount(int playerIndex)
		{
			PlayerParticleCount[playerIndex]--;
			_recalculatePercentages = true;
		}

		private void CalculatePercentages()
		{
			for (int playerIndex = 0; playerIndex < PlayerParticleCount.Length; ++playerIndex)
			{
				float percentage = (float)(PlayerParticleCount[playerIndex] - _startParticleCount) /
				                   (float)(_playerSettings.ParticleCount - _startParticleCount);
				percentage = Mathf.Clamp(percentage, 0f, 1f) * 100f;	
				_cursorStore.SetPlayerCursorMorph(playerIndex, percentage);

				if (percentage == 100f)
				{
					_update = false;
					_cursorStore.PlayerWin(playerIndex);
					WinEvent();
				}
				else if (PlayerParticleCount[playerIndex] == 0f && !PlayerLose[playerIndex])
				{
					PlayerLose[playerIndex] = true;
					_cursorStore.PlayerLose(playerIndex);
					LoseEvent(playerIndex);
				}
			}
		}
	}
}
