using UnityEngine;
using System;
using System.Collections;

namespace Partak {
public class CellParticleStore : MonoBehaviour {

	public CellParticle[] CellParticleArray { get; private set; }
	public readonly int[] PlayerParticleCount = new int[MenuConfig.MaxPlayers];
	public readonly bool[] PlayerLost = new bool[MenuConfig.MaxPlayers];
	CursorStore _cursorStore;
	MenuConfig _playerSettings;
	int _startParticleCount;
	public event Action WinEvent;
	public event Action<int> LoseEvent;
	LevelConfig _levelConfig;

	void Awake() {
		_levelConfig = FindObjectOfType<LevelConfig>();
		_cursorStore = FindObjectOfType<CursorStore>();
		_playerSettings = Persistent.Get<MenuConfig>();
		CellParticleArray = new CellParticle[_levelConfig.ParticleCount];
		_startParticleCount = _levelConfig.ParticleCount / _playerSettings.ActivePlayerCount();

		FindObjectOfType<CellParticleSpawn>().SpawnComplete += () => {
			StartCoroutine(CalculatePercentages());
		};
	}

	public void IncrementPlayerParticleCount(int playerIndex) {
		PlayerParticleCount[playerIndex]++;
	}

	public void DecrementPlayerParticleCount(int playerIndex) {
		PlayerParticleCount[playerIndex]--;
	}

	IEnumerator CalculatePercentages() {
		yield return new WaitForSeconds(2.0f);
		while (true) {
			for (int playerIndex = 0; playerIndex < PlayerParticleCount.Length; ++playerIndex) {
				float percentage = (float)(PlayerParticleCount[playerIndex] - _startParticleCount) /
				                  (float)(_levelConfig.ParticleCount - _startParticleCount);
				percentage = Mathf.Clamp(percentage, 0f, 1f) * 100f;	
				_cursorStore.SetPlayerCursorMorph(playerIndex, percentage);
				if (percentage == 100f) {
					Win();
					yield break;
				} else if (PlayerParticleCount[playerIndex] == 0f && !PlayerLost[playerIndex]) {
					PlayerLost[playerIndex] = true;
					_cursorStore.PlayerLose(playerIndex);
					LoseEvent(playerIndex);
				}
			}
			yield return new WaitForSeconds(.2f);
		}
	}

	//should be its own object WinSequence
	public void Win() {
		_cursorStore.PlayerWin(WinningPlayer());
		WinEvent();
	}

	public int WinningPlayer() {
		int count = 0;
		int playerIndex = 0;
		for (int i = 0; i < PlayerParticleCount.Length; ++i) {
			if (PlayerParticleCount[i] > count) {
				count = PlayerParticleCount[i];
				playerIndex = i;
			}
		}
		return playerIndex;
	}

	public int LosingPlayer() {
		int count = int.MaxValue;
		int playerIndex = 0;
		for (int i = 0; i < PlayerParticleCount.Length; ++i) {
			if (PlayerParticleCount[i] < count && !PlayerLost[i]) {
				count = PlayerParticleCount[i];
				playerIndex = i;
			}
		}
		return playerIndex;
	}
}
}
