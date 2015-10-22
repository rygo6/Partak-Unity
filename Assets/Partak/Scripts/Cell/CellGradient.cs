using UnityEngine;
using System.Collections;
using System.Threading;

namespace Partak
{
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
				_currentStepDirectionIndex = (int)Mathf.Repeat(value, _stepDirectionArray.Length);
			}
		}
		private int _currentStepDirectionIndex;

		/// <summary>
		/// Pseudo Random directions which the gradient will rotate around
		/// </summary>
		private static readonly int[][] _stepDirectionArray = { 
			new int[] { 11, 8, 1, 4, 3, 0 }, 
			new int[] { 8, 9, 7, 5, 1, 3 }, 
			new int[] { 2, 9, 11, 1, 3, 7 }, 
			new int[] { 5, 2, 0, 10, 9, 6 }, 
			new int[] { 10, 9, 7, 4, 3, 0 }, 
			new int[] { 1, 3, 4, 7, 5, 11 }, 
			new int[] { 5, 3, 1, 11, 8, 7 }, 
			new int[] { 11, 8, 10, 1, 3, 4 }, 
			new int[] { 0, 1, 8, 6, 9, 10 } 
		};

		private CellGroup[] CellGroupStepArray
		{
			get
			{
				if (_cellGroupStepArray == null)
				{
					_cellGroupStepArray = new CellGroup[_cellHiearchy.particleCellGrid.Grid.Length * 2];		
				}
				return _cellGroupStepArray;
			}
		}
		private CellGroup[] _cellGroupStepArray;

		private bool Run { get; set; }

		private void Start()
		{
			StartCoroutine(RunCoroutine());
		}

		private void RunThread()
		{
			Run = true;
			while (Run)
			{
				Thread.Sleep(0);
				CalculateGradient();
			}
		}

		private IEnumerator RunCoroutine()
		{
			Run = true;
			while (Run)
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

				int particleIndex = CellUtility.WorldPositionToGridIndex(_playerInputArray[playerIndex].position.x, _playerInputArray[playerIndex].position.z, _cellHiearchy.particleCellGrid.Dimension);
				ParticleCell startParticleCell = _cellHiearchy.particleCellGrid.Grid[particleIndex];

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

			AddFirstCellGroupToStepArray(startCellGroup);

			bool runCalculation = true;
			while (runCalculation)
			{
				CellGroup currentCellGroup = CellGroupStepArray[currentIndex];
				if (currentCellGroup != null)
				{
					for (int d = 0; d < _stepDirectionArray[CurrentStepDirectionIndex].Length; ++d)
					{
						int direction = _stepDirectionArray[CurrentStepDirectionIndex][d];
						CellGroup nextCellGroup = currentCellGroup.DirectionalCellGroupArray[direction];
						if (nextCellGroup != null && !nextCellGroup.InStepArray)
						{			
							int primaryDirection = CellUtility.InvertDirection(direction);
							nextCellGroup.SetPrimaryDirectionChldParticleCell(primaryDirection, playerIndex);
							AddCellGroupToStepArray(nextCellGroup);
						}
					}
						
					Debug.DrawRay(currentCellGroup.WorldPosition, Vector3.up * _debugRayHeight);
					_debugRayHeight += .001f;

					CurrentStepDirectionIndex++;
					CellGroupStepArray[currentIndex] = null;
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
			_debugRayHeight = .01f;
			_lastAddedGroupStepArrayIndex = 0;
			AddCellGroupToStepArray(cellGroup);
		}
		private float _debugRayHeight;

		private void AddCellGroupToStepArray(CellGroup cellGroup)
		{
			cellGroup.InStepArray = true;
			CellGroupStepArray[_lastAddedGroupStepArrayIndex] = cellGroup;
			_lastAddedGroupStepArrayIndex++;
		}
		private int _lastAddedGroupStepArrayIndex;

		private void ResetCellHiearchyInStepArray(CellHiearchy cellhiearchy)
		{
			for (int i = 0; i < _cellHiearchy.cellGroupGridArray.Length; ++i)
			{
				for (int o = 0; o < _cellHiearchy.cellGroupGridArray[i].Grid.Length; ++o)
				{
					if (_cellHiearchy.cellGroupGridArray[i].Grid[o] != null)
					{
						_cellHiearchy.cellGroupGridArray[i].Grid[o].InStepArray = false;
					}
				}
			}
		}
	}
}