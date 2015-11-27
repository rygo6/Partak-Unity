using UnityEngine;
using System.Collections;

namespace Partak
{
	public class CellAI : MonoBehaviour
	{
		private CursorStore _cursorStore;

		private CellParticleStore _cellParticleStore;

		private PlayerSettings _playerSettings;

		private GameTimer _gameTimer;

		private LevelConfig _levelConfig;

		private readonly Vector3[] AICursorTarget = new Vector3[PlayerSettings.MaxPlayers];

		private readonly Vector3[] AICursorVelocity = new Vector3[PlayerSettings.MaxPlayers];

		private readonly int[] AICellParticleIndex = new int[PlayerSettings.MaxPlayers];

		private readonly int[] RandomPullCycle = new int[PlayerSettings.MaxPlayers];

		[SerializeField]
		private int _randomCycleRate = 20;

		private bool _run;

		private void Awake()
		{
			_cursorStore = FindObjectOfType<CursorStore>();
			_cellParticleStore = FindObjectOfType<CellParticleStore>();
			_playerSettings = Persistent.Get<PlayerSettings>();
			_gameTimer = FindObjectOfType<GameTimer>();
			_levelConfig = FindObjectOfType<LevelConfig>();

			for (int i = 0; i < PlayerSettings.MaxPlayers; ++i)
			{
				AICellParticleIndex[i] = i * 10;
				RandomPullCycle[i] = i * (_randomCycleRate / PlayerSettings.MaxPlayers);
			}

			FindObjectOfType<CellParticleSpawn>().SpawnComplete += () =>
			{
				_run = true;
				StartCoroutine(UpdateAICursor());
			};

			FindObjectOfType<CellParticleStore>().WinEvent += () =>
			{
				_run = false;
			};
		}

		private void Update()
		{
			if (_run)
				MoveAICursor();
		}

		private void MoveAICursor()
		{
			for (int playerIndex = 0; playerIndex < PlayerSettings.MaxPlayers; ++playerIndex)
			{
				if (_playerSettings.GetPlayerMode(playerIndex) == PlayerMode.Comp)
				{
					_cursorStore.SetCursorPositionClamp(playerIndex, 
						Vector3.SmoothDamp(
							_cursorStore.CursorPositions[playerIndex], 
							AICursorTarget[playerIndex], 
							ref AICursorVelocity[playerIndex], 
							.4f));
				}
			}
		}

		private IEnumerator UpdateAICursor()
		{
			while (_run)
			{
				int winningPlayerIndex = _cellParticleStore.WinningPlayer();
				int losingPlayerIndex = _cellParticleStore.LosingPlayer();
				for (int playerIndex = 0; playerIndex < PlayerSettings.MaxPlayers; ++playerIndex)
				{
					if (_playerSettings.GetPlayerMode(playerIndex) == PlayerMode.Comp)
					{
						int targetPlayerIndex = 0;
						if (_gameTimer.GameTime < 8f)
							targetPlayerIndex = Random.Range(0, PlayerSettings.MaxPlayers);
						else if (playerIndex != winningPlayerIndex)
							targetPlayerIndex = winningPlayerIndex;
						else
							targetPlayerIndex = losingPlayerIndex;

						RandomPullCycle[playerIndex]++;
						if (RandomPullCycle[playerIndex] == _randomCycleRate)
						{
							RandomPullCycle[playerIndex] = 0;
							AICursorTarget[playerIndex].x = Random.Range(-10, _levelConfig.LevelBounds.max.x + 10);
							AICursorTarget[playerIndex].z = Random.Range(-10, _levelConfig.LevelBounds.max.z + 10);
						}
						else if (_cellParticleStore.PlayerCellParticleArray[targetPlayerIndex].Count > 20)
						{
							//_cellParticleStore.PlayerParticleCount[targetPlayerIndex] - 4 is done in case
							//between setting the index, and trying to retrieve it, the count is changed by another thread
							AICellParticleIndex[playerIndex] = 
								(int)Mathf.Repeat(AICellParticleIndex[playerIndex] + 100,
								_cellParticleStore.PlayerParticleCount[targetPlayerIndex] - 10);

							AICursorTarget[playerIndex] = 
								_cellParticleStore.PlayerCellParticleArray[targetPlayerIndex][AICellParticleIndex[playerIndex]].ParticleCell.WorldPosition;
						}
						//unless it's really small, then pull away
						else
						{
							AICursorTarget[playerIndex] = 
								_cellParticleStore.PlayerCellParticleArray[playerIndex][0].ParticleCell.WorldPosition;
						}

						yield return new WaitForSeconds(.5f);
					}
				}
				yield return null;
			}
		}
	}
}