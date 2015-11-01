using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Partak
{
	/// <summary>
	/// Cell gradient.
	/// Calculates the gradient for all players.
	/// All players are done in one class for ease of multithreading.
	/// </summary>
	public class CellGradient : MonoBehaviour
	{
		[SerializeField]
		private Transform[] _playerInputArray;

		[SerializeField]
		private CellHiearchy _cellHiearchy;

		private int CurrentStepDirectionIndex
		{ 
			get { return _currentStepDirectionIndex; }
			set
			{
				_currentStepDirectionIndex = (int)Mathf.Repeat(value, _stepDirectionArray.GetLength(0));
			}
		}
		private int _currentStepDirectionIndex;

		/// <summary>
		/// Pseudo Random directions which the gradient will rotate around
		/// </summary>
		private int[,] _stepDirectionArray;
			
		private CellGroup[] _cellGroupStepArray;

		private bool _run;

		private void Awake()
		{
			_cellGroupStepArray = new CellGroup[_cellHiearchy.ParticleCellGrid.Grid.Length * 2];

			//pre-generate random numbers
			int[] randomArray;
			_stepDirectionArray = new int[1000, 6];
			for (int x = 0; x < _stepDirectionArray.GetLength(0); ++x)
			{
				//generate non repeating array of 6 random numbers between 0 and 12
				randomArray = Enumerable.Range(0, Direction12.Count)
					.OrderBy(t => Random.Range(0, Direction12.Count))
					.Take(_stepDirectionArray.GetLength(1))
					.ToArray();
				for (int y = 0; y < _stepDirectionArray.GetLength(1); ++y)
				{
					_stepDirectionArray[x, y] = randomArray[y];
				}
			}
		}

		private void Start()
		{
			StartCoroutine(RunCoroutine());
		}

		private void RunThread()
		{
			_run = true;
			while (_run)
			{
				Thread.Sleep(0);
				CalculateGradient();
			}
		}

		private IEnumerator RunCoroutine()
		{
			_run = true;
			while (_run)
			{
				CalculateGradient();
				yield return null;
			}
		}

		private void CalculateGradient()
		{
			CurrentStepDirectionIndex++; //TODO is this needed here?

			for (int playerIndex = 0; playerIndex < _playerInputArray.Length; playerIndex++)
			{
				ResetCellHiearchyInStepArray(_cellHiearchy);

				int particleIndex = CellUtility.WorldPositionToGridIndex(_playerInputArray[playerIndex].position.x, _playerInputArray[playerIndex].position.z, _cellHiearchy.ParticleCellGrid.Dimension);
				ParticleCell startParticleCell = _cellHiearchy.ParticleCellGrid.Grid[particleIndex];

				if (startParticleCell != null)
				{
					CellGroup startCellGroup = startParticleCell.TopCellGroup;
					CalculatePlayerGradient(startCellGroup, playerIndex);
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
					for (d = 0; d < _stepDirectionArray.GetLength(1); ++d)
					{
						direction = _stepDirectionArray[_currentStepDirectionIndex, d];
						CellGroup nextCellGroup = currentCellGroup.DirectionalCellGroupArray[direction];
						if (nextCellGroup != null && !nextCellGroup.InStepArray)
						{			
							primaryDirection = CellUtility.InvertDirection(direction);
							nextCellGroup.SetPrimaryDirectionChldParticleCell(primaryDirection, playerIndex);
							AddCellGroupToStepArray(nextCellGroup);
						}
					}

#if UNITY_EDITOR
					CellGroup nextGroup = currentCellGroup.DirectionalCellGroupArray[currentCellGroup.ChildParticleCellArray[0].PrimaryDirectionArray[playerIndex]];
					if (nextGroup != null)
					{
						Debug.DrawRay(currentCellGroup.WorldPosition, (currentCellGroup.WorldPosition - nextGroup.WorldPosition), Color.cyan);
					}
//					Debug.DrawRay(currentCellGroup.WorldPosition, Vector3.up * _debugRayHeight);
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
#if UNITY_EDITOR			
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
					if (_cellHiearchy.CellGroupGridArray[c].FlatGrid[g] != null)
					{
						_cellHiearchy.CellGroupGridArray[c].FlatGrid[g].InStepArray = false;
					}
				}
			}
		}
	}
}