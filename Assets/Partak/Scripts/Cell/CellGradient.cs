//#define DEBUG_GRADIENT
//#define COROUTINE

using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace Partak
{
    /// <summary>
    /// Cell gradient.
    /// Calculates the gradient for all players.
    /// All players are done in one class for ease of multithreading.
    /// </summary>
    public class CellGradient : MonoBehaviour
	{
		private CursorStore _cursorStore;

		[SerializeField]
		private CellHiearchy _cellHiearchy;

		private int CurrentStepDirectionIndex
		{ 
			get { return _currentStepDirectionIndex; }
			set
			{
				if (value >= _stepDirectionArray.Length)
					_currentStepDirectionIndex = _stepDirectionArray.Length - value;
				else
					_currentStepDirectionIndex = value;
			}
		}
		private int _currentStepDirectionIndex;

		/// <summary>
		/// Pseudo Random directions which the gradient will rotate around
		/// </summary>
		private int[][] _stepDirectionArray;
			
		private CellGroup[] _cellGroupStepArray;

		private readonly CellGroup[] PriorStartCell = new CellGroup[PlayerSettings.MaxPlayers];

		private bool _runThread;

#if UNITY_WSA_10_0 && !UNITY_EDITOR
        private Windows.Foundation.IAsyncAction _async;
        private System.Threading.ManualResetEvent _wait = new System.Threading.ManualResetEvent(false);
#else
        private Thread _thread;
#endif

        private PlayerSettings _playerSettings;

		[SerializeField]
		private int _cycleTime = 33;

		private void Awake()
		{
			_cursorStore = FindObjectOfType<CursorStore>();
			_playerSettings = Persistent.Get<PlayerSettings>();
			_cellGroupStepArray = new CellGroup[_cellHiearchy.ParticleCellGrid.Grid.Length * 2];

			int patternStepDirectionIndex = 0;
			int[][] patternStepDirections = new int[][] {
				new int[]{ 0, 2, 4, 6, 8, 10 },
				new int[]{ 1, 3, 5, 7, 9, 11 },
				new int[]{ 10, 8, 6, 4, 2, 0 },
				new int[]{ 11, 9, 7, 5, 3, 1 },
			};

			//pre-generate random numbers
			int[] randomArray;
			_stepDirectionArray = new int[128][];
			for (int x = 0; x < _stepDirectionArray.Length; ++x)
			{
				if (x % 2 == 0)
				{
					_stepDirectionArray[x] = new int[4];
					randomArray = Enumerable.Range(0, Direction12.Count)
											.OrderBy(t => Random.Range(0, Direction12.Count))
											.Take(_stepDirectionArray[x].Length)
											.ToArray();
					for (int y = 0; y < _stepDirectionArray[x].Length; ++y)
					{
						_stepDirectionArray[x][y] = randomArray[y];
					}
				}
				else
				{
					_stepDirectionArray[x] = patternStepDirections[patternStepDirectionIndex];
					patternStepDirectionIndex = (int)Mathf.Repeat(++patternStepDirectionIndex, patternStepDirections.Length);
				}
			}

			FindObjectOfType<CellParticleStore>().WinEvent += () =>
			{
				_runThread = false;
			};
		}

		private void Start()
		{
			_runThread = true;
#if COROUTINE
			StartCoroutine(RunCoroutine());
#elif UNITY_WSA_10_0 && !UNITY_EDITOR
            _async = Windows.System.Threading.ThreadPool.RunAsync((workItem) =>
            {
                RunThread();
            }, Windows.System.Threading.WorkItemPriority.Low);
#else
            _thread = new Thread(RunThread);
			_thread.Name = "CellGradient";
			_thread.IsBackground = true;
			_thread.Priority = System.Threading.ThreadPriority.Lowest;
			_thread.Start();
#endif

#if UNITY_IOS && !UNITY_EDITOR
			SetGradientThreadPriority();
#endif
        }

		#if UNITY_IOS && !UNITY_EDITOR
		[DllImport("__Internal")]
		private static extern bool SetGradientThreadPriority();
		#endif

		private void OnDestroy()
		{
			StopThread();
		}

        private void StopThread()
        {
#if UNITY_WSA_10_0 && !UNITY_EDITOR
            _async.Cancel();
            _async.Close();
#else
            if (_thread != null)
            {
                _runThread = false;
                while (_thread.IsAlive)
                {
                }
            }
#endif
        }

        private void RunThread()
		{
			while (_runThread)
			{
				Stopwatch stopWatch = new Stopwatch();
				stopWatch.Start();
				int startTime;
				int deltaTime;
				while (_runThread)
				{
					startTime = (int)stopWatch.ElapsedMilliseconds;
					CalculateGradient();
					deltaTime = (int)stopWatch.ElapsedMilliseconds - startTime;
					if (deltaTime < _cycleTime)
					{
#if UNITY_WSA_10_0 && !UNITY_EDITOR
                        _wait.WaitOne(_cycleTime - deltaTime);
#else
                        Thread.Sleep(_cycleTime - deltaTime);
#endif
                    }
					stopWatch.Reset();
				}
			}
		}

        private IEnumerator RunCoroutine()
		{
			_runThread = true;
			while (_runThread)
			{
				CalculateGradient();
				yield return null;
			}
		}

		private void CalculateGradient()
		{
			for (int playerIndex = 0; playerIndex < PlayerSettings.MaxPlayers; playerIndex++)
			{
				if (_playerSettings.PlayerActive(playerIndex))
				{
					ResetCellHiearchyInStepArray();

					CurrentStepDirectionIndex++;

					int particleIndex = CellUtility.WorldPositionToGridIndex(
						                    _cursorStore.CursorPositions[playerIndex].x, 
						                    _cursorStore.CursorPositions[playerIndex].z, 
						                    _cellHiearchy.ParticleCellGrid.Dimension);
					ParticleCell startParticleCell = _cellHiearchy.ParticleCellGrid.Grid[particleIndex];

					if (startParticleCell != null)
					{
						CellGroup startCellGroup = startParticleCell.TopCellGroup;
						CalculatePlayerGradient(startCellGroup, playerIndex);
						PriorStartCell[playerIndex] = startCellGroup;
					}
					else
					{
						CalculatePlayerGradient(PriorStartCell[playerIndex], playerIndex);
					}
				}
			}
		}

		private void CalculatePlayerGradient(CellGroup startCellGroup, int playerIndex)
		{
			int currentIndex = 0;
			int direction, primaryDirection, d;
			CellGroup currentCellGroup;
			CellGroup nextCellGroup;

			AddFirstCellGroupToStepArray(startCellGroup);

			bool runCalculation = true;
			while (runCalculation)
			{
				currentCellGroup = _cellGroupStepArray[currentIndex];
				if (currentCellGroup != null)
				{
					for (d = 0; d < _stepDirectionArray[_currentStepDirectionIndex].Length; ++d)
					{
						direction = _stepDirectionArray[_currentStepDirectionIndex][d];
						nextCellGroup = currentCellGroup.DirectionalCellGroupArray[direction];
						if (nextCellGroup != null && !nextCellGroup.InStepArray)
						{			
							primaryDirection = CellUtility.InvertDirection(direction);
							nextCellGroup.SetPrimaryDirectionChldParticleCell(primaryDirection, playerIndex);
							AddCellGroupToStepArray(nextCellGroup);
						}
					}
				
#if DEBUG_GRADIENT
					CellGroup nextGroup = currentCellGroup.DirectionalCellGroupArray[currentCellGroup.ChildParticleCellArray[0].PrimaryDirectionArray[playerIndex]];
					if (nextGroup != null)
					{
						Debug.DrawRay(currentCellGroup.WorldPosition, (currentCellGroup.WorldPosition - nextGroup.WorldPosition), Color.cyan);
					}
					Debug.DrawRay(currentCellGroup.WorldPosition, Vector3.up * _debugRayHeight);
					_debugRayHeight += .001f;
#endif

					CurrentStepDirectionIndex++;
					_cellGroupStepArray[currentIndex] = null;
					currentIndex++;
				}
				else
				{
					runCalculation = false;	
				}
			}
		}

		private void AddFirstCellGroupToStepArray(CellGroup cellGroup)
		{
#if DEBUG_GRADIENT
			_debugRayHeight = .01f;
#endif
			_lastAddedGroupStepArrayIndex = 0;
			AddCellGroupToStepArray(cellGroup);
		}
		private float _debugRayHeight;

		private void AddCellGroupToStepArray(CellGroup cellGroup)
		{
			cellGroup.InStepArray = true;
			_cellGroupStepArray[_lastAddedGroupStepArrayIndex] = cellGroup;
			_lastAddedGroupStepArrayIndex++;
		}
		private int _lastAddedGroupStepArrayIndex;

		/// <summary>
		/// Iterates through entire CellHiearchy setting the bool InStepArray
		/// to false to prepare for next cycle of calculation
		/// </summary>
		/// <param name="cellhiearchy">Cellhiearchy.</param>
		private void ResetCellHiearchyInStepArray()
		{
			int limit = _cellHiearchy.CombinedFlatCellGroups.Length;
			int i;
			for (i = 0; i < limit; ++i)
			{
				_cellHiearchy.CombinedFlatCellGroups[i].InStepArray = false;
			}
		}
	}
}