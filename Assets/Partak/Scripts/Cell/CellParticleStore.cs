﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Partak
{
	public class CellParticleStore : MonoBehaviour
	{
		public CellParticle[] CellParticleArray { get; private set; }

		public readonly List<CellParticle>[] PlayerCellParticleArray = new List<CellParticle>[PlayerSettings.MaxPlayers];

		public readonly int[] PlayerParticleCount = new int[PlayerSettings.MaxPlayers];

		/// <summary> Keeps track of which players have lost. </summary>
		public readonly bool[] PlayerLose = new bool[PlayerSettings.MaxPlayers];

		private CursorStore _cursorStore;

		private PlayerSettings _playerSettings;

		private int _startParticleCount;

		private bool _recalculatePercentages;

		private bool _update;

		public event Action WinEvent;

		public event Action<int> LoseEvent;

		private LevelConfig _levelConfig;

		private void Awake()
		{
			_levelConfig = FindObjectOfType<LevelConfig>();
			_cursorStore = FindObjectOfType<CursorStore>();
			_playerSettings = Persistent.Get<PlayerSettings>();
			CellParticleArray = new CellParticle[_levelConfig.ParticleCount];
			_startParticleCount = _levelConfig.ParticleCount / _playerSettings.ActivePlayerCount();
			for (int playerIndex = 0; playerIndex < PlayerSettings.MaxPlayers; ++playerIndex)
			{
				PlayerCellParticleArray[playerIndex] = new List<CellParticle>();
				PlayerCellParticleArray[playerIndex].Capacity = _levelConfig.ParticleCount;
			}

			FindObjectOfType<CellParticleSpawn>().SpawnComplete += () =>
			{
				_update = true;
			};
		}

		private void LateUpdate()
		{
			if (_recalculatePercentages && _update)
			{
				_recalculatePercentages = false;
				CalculatePercentages();
			}
		}

		public void IncrementPlayerParticleCount(CellParticle cellParticle)
		{
			PlayerCellParticleArray[cellParticle.PlayerIndex].Add(cellParticle);
			PlayerParticleCount[cellParticle.PlayerIndex]++;
			_recalculatePercentages = true;
		}

		public void DecrementPlayerParticleCount(CellParticle cellParticle)
		{
			PlayerCellParticleArray[cellParticle.PlayerIndex].Remove(cellParticle);
			PlayerParticleCount[cellParticle.PlayerIndex]--;
			_recalculatePercentages = true;
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
