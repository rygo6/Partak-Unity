﻿using UnityEngine;
using System;

namespace Partak
{
    public class CellParticleStore : MonoBehaviour
	{
		public CellParticle[] CellParticleArray { get; private set; }

		public readonly int[] PlayerParticleCount = new int[MenuConfig.MaxPlayers];

		/// <summary> Keeps track of which players have lost. </summary>
		public readonly bool[] PlayerLose = new bool[MenuConfig.MaxPlayers];

		private CursorStore _cursorStore;

		private MenuConfig _playerSettings;

		private int _startParticleCount;

//		private bool _recalculatePercentages;

//		private bool _update;

		public event Action WinEvent;

		public event Action<int> LoseEvent;

		private LevelConfig _levelConfig;

		private void Awake()
		{
			_levelConfig = FindObjectOfType<LevelConfig>();
			_cursorStore = FindObjectOfType<CursorStore>();
			_playerSettings = Persistent.Get<MenuConfig>();
			CellParticleArray = new CellParticle[_levelConfig.ParticleCount];
			_startParticleCount = _levelConfig.ParticleCount / _playerSettings.ActivePlayerCount();

			FindObjectOfType<CellParticleSpawn>().SpawnComplete += () =>
			{
				InvokeRepeating("CalculatePercentages", 1f, .1f);
			};
		}

		public void IncrementPlayerParticleCount(int playerIndex)
		{
			PlayerParticleCount[playerIndex]++;
		}

		public void DecrementPlayerParticleCount(int playerIndex)
		{
			PlayerParticleCount[playerIndex]--;
		}

		private void CalculatePercentages()
		{
			for (int playerIndex = 0; playerIndex < PlayerParticleCount.Length; ++playerIndex)
			{
				float percentage = (float)(PlayerParticleCount[playerIndex] - _startParticleCount) /
				                   (float)(_levelConfig.ParticleCount - _startParticleCount);
				percentage = Mathf.Clamp(percentage, 0f, 1f) * 100f;	
				_cursorStore.SetPlayerCursorMorph(playerIndex, percentage);

				if (percentage == 100f)
				{
					CancelInvoke();
					Win();
				}
				else if (PlayerParticleCount[playerIndex] == 0f && !PlayerLose[playerIndex])
				{
					PlayerLose[playerIndex] = true;
					_cursorStore.PlayerLose(playerIndex);
					LoseEvent(playerIndex);
				}
			}
		}

		//should be its own object WinSequence
		public void Win()
		{
			_cursorStore.PlayerWin(WinningPlayer());
			WinEvent();
		}

		public int WinningPlayer()
		{
			int count = 0;
			int playerIndex = 0;
			for (int i = 0; i < PlayerParticleCount.Length; ++i)
			{
				if (PlayerParticleCount[i] > count)
				{
					count = PlayerParticleCount[i];
					playerIndex = i;
				}
			}
			return playerIndex;
		}

		public int LosingPlayer()
		{
			int count = int.MaxValue;
			int playerIndex = 0;
			for (int i = 0; i < PlayerParticleCount.Length; ++i)
			{
				if (PlayerParticleCount[i] < count && !PlayerLose[i])
				{
					count = PlayerParticleCount[i];
					playerIndex = i;
				}
			}
			return playerIndex;
		}
	}
}
