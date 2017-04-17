using System.Collections;
using System.Threading;
using UnityEngine;
using GT.Threading;

namespace Partak {
public class CellAI : MonoBehaviour {

	[SerializeField] int _randomCycleRate = 20;
	readonly Vector3[] AICursorTarget = new Vector3[MenuConfig.MaxPlayers];
	readonly Vector3[] AICursorVelocity = new Vector3[MenuConfig.MaxPlayers];
	readonly int[] AICellParticleIndex = new int[MenuConfig.MaxPlayers];
	readonly int[] RandomPullCycle = new int[MenuConfig.MaxPlayers];
	readonly System.Random _random = new System.Random();
	CursorStore _cursorStore;
	CellParticleStore _cellParticleStore;
	MenuConfig _playerSettings;
	GameClock _gameTimer;
	LevelConfig _levelConfig;
	LoopThread _loopThread;

	private void Awake() {
		_cursorStore = FindObjectOfType<CursorStore>();
		_cellParticleStore = FindObjectOfType<CellParticleStore>();
		_playerSettings = Persistent.Get<MenuConfig>();
		_gameTimer = FindObjectOfType<GameClock>();
		_levelConfig = FindObjectOfType<LevelConfig>();
		for (int i = 0; i < MenuConfig.MaxPlayers; ++i) {
			AICellParticleIndex[i] = i * 10;
			RandomPullCycle[i] = i * (_randomCycleRate / MenuConfig.MaxPlayers);
		}
		FindObjectOfType<CellParticleSpawn>().SpawnComplete += () => {
			_loopThread = LoopThread.Create(UpdateAICursor, "CellAI", Priority.Low);
			_loopThread.Start();
		};
	}

	private void Update() {
		MoveAICursor();
	}

	private void OnDestroy() {
		if (_loopThread != null)
			_loopThread.Stop();
	}

	private void MoveAICursor() {
		for (int playerIndex = 0; playerIndex < MenuConfig.MaxPlayers; ++playerIndex) {
			if (_playerSettings.PlayerModes[playerIndex] == PlayerMode.Comp &&
			    !_cellParticleStore.PlayerLost[playerIndex]) {
				_cursorStore.SetCursorPositionClamp(playerIndex, 
					Vector3.SmoothDamp(
						_cursorStore.CursorPositions[playerIndex], 
						AICursorTarget[playerIndex], 
						ref AICursorVelocity[playerIndex], 
						.4f));
			}
		}
	}

	private void UpdateAICursor() {
		int winningPlayerIndex = _cellParticleStore.WinningPlayer();
		int losingPlayerIndex = _cellParticleStore.LosingPlayer();
		int targetPlayerIndex, playerIndex, newIndex;
		int particleLimit = _cellParticleStore.CellParticleArray.Length;
		int playerLimit = MenuConfig.MaxPlayers;
		for (playerIndex = 0; playerIndex < playerLimit; ++playerIndex) {
			if (_playerSettings.PlayerModes[playerIndex] == PlayerMode.Comp &&
			    !_cellParticleStore.PlayerLost[playerIndex]) {
				targetPlayerIndex = 0;
				if (_gameTimer.GameTime < 8f)
					targetPlayerIndex = _random.Next(0, playerLimit);
				else if (playerIndex != winningPlayerIndex)
					targetPlayerIndex = winningPlayerIndex;
				else
					targetPlayerIndex = losingPlayerIndex;

				RandomPullCycle[playerIndex]++;
				if (RandomPullCycle[playerIndex] == _randomCycleRate) {
					RandomPullCycle[playerIndex] = 0;
					AICursorTarget[playerIndex].x = _random.Next(-10, (int)_levelConfig.LevelBounds.max.x + 10);
					AICursorTarget[playerIndex].z = _random.Next(-10, (int)_levelConfig.LevelBounds.max.z + 10);
				} else if (_cellParticleStore.PlayerParticleCount[targetPlayerIndex] > 20) {
					newIndex = AICellParticleIndex[playerIndex] + 1;
					if (newIndex >= particleLimit)
						newIndex = 0;
					while (_cellParticleStore.CellParticleArray[newIndex].PlayerIndex != targetPlayerIndex &&
					       !_cellParticleStore.PlayerLost[targetPlayerIndex]) {
						++newIndex;
						if (newIndex >= particleLimit)
							newIndex = 0;
					}
					AICellParticleIndex[playerIndex] = newIndex;
					AICursorTarget[playerIndex] = _cellParticleStore.CellParticleArray[newIndex].ParticleCell.WorldPosition;
				} else {
					AICursorTarget[playerIndex] = _cellParticleStore.CellParticleArray[AICellParticleIndex[playerIndex]].ParticleCell.WorldPosition;
				}
				_loopThread.Wait(500);
			}
		}
	}
}
}