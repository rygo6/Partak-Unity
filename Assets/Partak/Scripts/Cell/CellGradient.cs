﻿//#define DEBUG_GRADIENT

using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

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
				_currentStepDirectionIndex = (int)Mathf.Repeat(value, _stepDirectionArray.Length);
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

		private Thread _thread;

		private PlayerSettings _playerSettings;

		[SerializeField]
		private int _cycleTime = 33;

		private void Awake()
		{
			_cursorStore = FindObjectOfType<CursorStore>();
			_playerSettings = Persistent.Get<PlayerSettings>();
			_cellGroupStepArray = new CellGroup[_cellHiearchy.ParticleCellGrid.Grid.Length * 2];

//			_stepDirectionArray = new int[4][] {
//				new int[6]{ 0, 1, 2, 3, 4, 5 },
//				new int[6]{ 11, 10, 9, 8, 7, 6 },	
//				new int[6]{ 0, 2, 4, 6, 8, 10 },	
//				new int[6]{ 1, 3, 5, 7, 9, 11 },	
//			};

			//pre-generate random numbers
			int[] randomArray;
			_stepDirectionArray = new int[1000][];
			for (int x = 0; x < _stepDirectionArray.Length; ++x)
			{
				_stepDirectionArray[x] = new int[Random.Range(6, 10)];
				//generate non repeating array of 6 random numbers between 0 and 12
				randomArray = Enumerable.Range(0, Direction12.Count)
					.OrderBy(t => Random.Range(0, Direction12.Count))
					.Take(_stepDirectionArray[x].Length)
					.ToArray();
				for (int y = 0; y < _stepDirectionArray[x].Length; ++y)
				{
					_stepDirectionArray[x][y] = randomArray[y];
				}
			}

			FindObjectOfType<CellParticleStore>().WinEvent += StopThread;
		}

		private void Start()
		{
			_runThread = true;
			_thread = new Thread(RunThread);
			_thread.Name = "CellGradient";
			_thread.IsBackground = true;
			_thread.Priority = System.Threading.ThreadPriority.BelowNormal;
			_thread.Start();
//			StartCoroutine(RunCoroutine());
		}
			
		private void OnDestroy()
		{
			StopThread();
		}

		private void StopThread()
		{
			if (_thread != null)
			{
#if UNITY_EDITOR
				_thread.Abort();	
#endif
				_runThread = false;
				while (_thread.IsAlive)
				{
				}
			}
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
						Thread.Sleep(_cycleTime - deltaTime);
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
					ResetCellHiearchyInStepArray(_cellHiearchy);

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
			int direction;
			int primaryDirection;
			int d;

			AddFirstCellGroupToStepArray(startCellGroup);

			bool runCalculation = true;
			while (runCalculation)
			{
				CellGroup currentCellGroup = _cellGroupStepArray[currentIndex];
				if (currentCellGroup != null)
				{
					for (d = 0; d < _stepDirectionArray[_currentStepDirectionIndex].Length; ++d)
					{
						direction = _stepDirectionArray[_currentStepDirectionIndex][d];
						CellGroup nextCellGroup = currentCellGroup.DirectionalCellGroupArray[direction];
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
		private void ResetCellHiearchyInStepArray(CellHiearchy cellhiearchy)
		{
			int hiearchyLimit = _cellHiearchy.CellGroupGridArray.Length;
			int gridLimit;
			int g;
			int c;

			for (c = 0; c < hiearchyLimit; ++c)
			{
				gridLimit = _cellHiearchy.CellGroupGridArray[c].FlatGrid.Length;
				for (g = 0; g < gridLimit; ++g)
				{
					_cellHiearchy.CellGroupGridArray[c].FlatGrid[g].InStepArray = false;
				}
			}
		}
	}
}