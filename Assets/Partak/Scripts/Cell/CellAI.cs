//#define COROUTINE

using System.Collections;
using System.Threading;
using UnityEngine;

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

		private bool _runThread;

		private Thread _thread;

		private readonly System.Random _random = new System.Random();

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
				_runThread = true;
				#if COROUTINE
				StartCoroutine(RunCoroutine());
				#elif UNITY_WSA_10_0

				#else
				_thread = new Thread(RunThread);
				_thread.IsBackground = true;
				_thread.Priority = System.Threading.ThreadPriority.Lowest;
				_thread.Name = "CellAI";
				_thread.Start();
				#endif
			};

			FindObjectOfType<CellParticleStore>().WinEvent += () =>
			{
				_runThread = false;
			};
		}

		private void Update()
		{
			if (_runThread)
				MoveAICursor();
		}

		private void OnDestroy()
		{
			StopThread();
		}

		private void StopThread()
		{
			if (_thread != null)
			{		
				_runThread = false;
				while (_thread.IsAlive)
				{
				}
			}
		}

		private void MoveAICursor()
		{
			for (int playerIndex = 0; playerIndex < PlayerSettings.MaxPlayers; ++playerIndex)
			{
				if (_playerSettings.GetPlayerMode(playerIndex) == PlayerMode.Comp &&
				    !_cellParticleStore.PlayerLose[playerIndex])
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

		private void RunThread()
		{
			while (_runThread)
			{
				UpdateAICursor();
			}
		}

		private IEnumerator RunCoroutine()
		{
			while (_runThread)
			{
				UpdateAICursor();
				yield return null;
			}
		}

		private void UpdateAICursor()
		{
			int winningPlayerIndex = _cellParticleStore.WinningPlayer();
			int losingPlayerIndex = _cellParticleStore.LosingPlayer();
			int targetPlayerIndex, playerIndex, newIndex;
			int particleLimit = _cellParticleStore.CellParticleArray.Length;
			int playerLimit = PlayerSettings.MaxPlayers;

			for (playerIndex = 0; playerIndex < playerLimit; ++playerIndex)
			{
				if (_playerSettings.GetPlayerMode(playerIndex) == PlayerMode.Comp &&
				    !_cellParticleStore.PlayerLose[playerIndex])
				{
					targetPlayerIndex = 0;
					if (_gameTimer.GameTime < 8f)
						targetPlayerIndex = _random.Next(0, playerLimit);
					else if (playerIndex != winningPlayerIndex)
						targetPlayerIndex = winningPlayerIndex;
					else
						targetPlayerIndex = losingPlayerIndex;

					RandomPullCycle[playerIndex]++;
					if (RandomPullCycle[playerIndex] == _randomCycleRate)
					{
						RandomPullCycle[playerIndex] = 0;
						AICursorTarget[playerIndex].x = _random.Next(-10, (int)_levelConfig.LevelBounds.max.x + 10);
						AICursorTarget[playerIndex].z = _random.Next(-10, (int)_levelConfig.LevelBounds.max.z + 10);
					}
					else if (_cellParticleStore.PlayerParticleCount[targetPlayerIndex] > 20)
					{
						newIndex = AICellParticleIndex[playerIndex] + 1;
						if (newIndex >= particleLimit)
							newIndex = 0;
						while (_cellParticleStore.CellParticleArray[newIndex].PlayerIndex != targetPlayerIndex &&
						       !_cellParticleStore.PlayerLose[targetPlayerIndex])
						{
							++newIndex;
							if (newIndex >= particleLimit)
								newIndex = 0;
						}
						AICellParticleIndex[playerIndex] = newIndex;
						AICursorTarget[playerIndex] = _cellParticleStore.CellParticleArray[newIndex].ParticleCell.WorldPosition;
					}
					else
					{
						AICursorTarget[playerIndex] = _cellParticleStore.CellParticleArray[AICellParticleIndex[playerIndex]].ParticleCell.WorldPosition;
					}

					#if COROUTINE
					yield return new WaitForSeconds(.5f);
					#elif UNITY_WSA_10_0

					#else
					Thread.Sleep(500);
					#endif
				}
			}
		}
	}
}