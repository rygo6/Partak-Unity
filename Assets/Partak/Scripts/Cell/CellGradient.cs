//#define DEBUG_GRADIENT

using System.Collections;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using EC.UniThread;

namespace Partak {
/// <summary>
/// Cell gradient.
/// Calculates the gradient for all players.
/// All players are done in one class for ease of multithreading.
/// </summary>
public class CellGradient : MonoBehaviour {

	[SerializeField] private int _cycleTime = 33;
	[SerializeField] CellHiearchy _cellHiearchy;
	readonly CellGroup[] PriorStartCell = new CellGroup[MenuConfig.MaxPlayers];
	CursorStore _cursorStore;
	CellGroup[] _cellGroupStepArray;
	MenuConfig _playerSettings;
	int _currentStepDirectionIndex;
	int _lastAddedGroupStepArrayIndex;
	float _debugRayHeight;
	LoopThread _loopThread;

	/// <summary>
	/// directions which the gradient will rotate around
	/// </summary>
	readonly int[][] _stepDirectionArray = new int[][] {
		new int[]{ 0, 2, 4, 6, 8, 10 },
		new int[]{ 10, 8, 7, 4, 2, 1 },
		new int[]{ 1, 3, 5, 7, 9, 11 },
		new int[]{ 0, 1, 4, 6, 7, 10 },
		new int[]{ 10, 8, 6, 4, 2, 0 },
		new int[]{ 10, 7, 6, 4, 1, 0 },
		new int[]{ 11, 9, 7, 5, 3, 1 },
		new int[]{ 1, 2, 4, 7, 8, 10 },
	};

	int CurrentStepDirectionIndex { 
		get { return _currentStepDirectionIndex; }
		set {
			if (value >= _stepDirectionArray.Length)
				_currentStepDirectionIndex = _stepDirectionArray.Length - value;
			else
				_currentStepDirectionIndex = value;
		}
	}

	void Awake() {
		_cursorStore = FindObjectOfType<CursorStore>();
		_playerSettings = Persistent.Get<MenuConfig>();
		_cellGroupStepArray = new CellGroup[_cellHiearchy.ParticleCellGrid.Grid.Length * 2];
	}

	void Start() {
		_loopThread = LoopThread.Create(CalculateGradient, "CellGradient", UniThreadPriority.Low, _cycleTime);
		_loopThread.Start();
	}

	void OnDestroy() {
		_loopThread.Stop();
	}

	void CalculateGradient() {
		for (int playerIndex = 0; playerIndex < MenuConfig.MaxPlayers; playerIndex++) {
			if (_playerSettings.PlayerActive(playerIndex)) {
				ResetCellHiearchyInStepArray();

				CurrentStepDirectionIndex++;

				int particleIndex = CellUtility.WorldPositionToGridIndex(
					                    _cursorStore.CursorPositions[playerIndex].x, 
					                    _cursorStore.CursorPositions[playerIndex].z, 
					                    _cellHiearchy.ParticleCellGrid.Dimension);
				ParticleCell startParticleCell = _cellHiearchy.ParticleCellGrid.Grid[particleIndex];

				if (startParticleCell != null) {
					CellGroup startCellGroup = startParticleCell.TopCellGroup;
					CalculatePlayerGradient(startCellGroup, playerIndex);
					PriorStartCell[playerIndex] = startCellGroup;
				} else {
					CalculatePlayerGradient(PriorStartCell[playerIndex], playerIndex);
				}
			}
		}
	}

	void CalculatePlayerGradient(CellGroup startCellGroup, int playerIndex) {
		int currentIndex = 0;
		int direction, primaryDirection, d;
		CellGroup currentCellGroup;
		CellGroup nextCellGroup;

		AddFirstCellGroupToStepArray(startCellGroup);

		bool runCalculation = true;
		while (runCalculation) {
			currentCellGroup = _cellGroupStepArray[currentIndex];
			if (currentCellGroup != null) {
				for (d = 0; d < _stepDirectionArray[_currentStepDirectionIndex].Length; ++d) {
					direction = _stepDirectionArray[_currentStepDirectionIndex][d];
					nextCellGroup = currentCellGroup.DirectionalCellGroupArray[direction];
					if (nextCellGroup != null && !nextCellGroup.InStepArray) {			
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
			} else {
				runCalculation = false;	
			}
		}
	}

	void AddFirstCellGroupToStepArray(CellGroup cellGroup) {
#if DEBUG_GRADIENT
			_debugRayHeight = .01f;
#endif
		_lastAddedGroupStepArrayIndex = 0;
		AddCellGroupToStepArray(cellGroup);
	}

	void AddCellGroupToStepArray(CellGroup cellGroup) {
		cellGroup.InStepArray = true;
		_cellGroupStepArray[_lastAddedGroupStepArrayIndex] = cellGroup;
		_lastAddedGroupStepArrayIndex++;
	}

	/// <summary>
	/// Iterates through entire CellHiearchy setting the bool InStepArray
	/// to false to prepare for next cycle of calculation
	/// </summary>
	/// <param name="cellhiearchy">Cellhiearchy.</param>
	void ResetCellHiearchyInStepArray() {
		int limit = _cellHiearchy.CombinedFlatCellGroups.Length;
		int i;
		for (i = 0; i < limit; ++i) {
			_cellHiearchy.CombinedFlatCellGroups[i].InStepArray = false;
		}
	}
}
}