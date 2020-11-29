//#define DEBUG_GRADIENT

using System;
using System.Threading.Tasks;
using GeoTetra.GTCommon.Components;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using GT.Threading;
using UnityEngine;

namespace GeoTetra.Partak
{
    /// <summary>
    ///     Cell gradient.
    ///     Calculates the gradient for all players.
    ///     All players are done in one class for ease of multithreading.
    /// </summary>
    public class CellGradient : SubscribableBehaviour
    {
        [SerializeField] private PartakStateRef _partakState;
        [SerializeField] private CellParticleStore _cellParticleStore;
        [SerializeField] private CursorStore _cursorStore;
        [SerializeField] private CellHiearchy _cellHiearchy;
        [SerializeField] private int _cycleTime = 33;

        /// <summary>
        ///     directions which the gradient will rotate around
        /// </summary>
        private readonly int[][] _stepDirectionArray =
        {
            new[] {0, 2, 4, 6, 8, 10},
            new[] {10, 8, 7, 4, 2, 1},
            new[] {1, 3, 5, 7, 9, 11},
            new[] {0, 1, 4, 6, 7, 10},
            new[] {10, 8, 6, 4, 2, 0},
            new[] {10, 7, 6, 4, 1, 0},
            new[] {11, 9, 7, 5, 3, 1},
            new[] {1, 2, 4, 7, 8, 10}
        };

        private CellGroup[] PriorStartCell;
        private CellGroup[] _cellGroupStepArray;
        private int _currentStepDirectionIndex;

        private float _debugRayHeight;
        private int _lastAddedGroupStepArrayIndex;
        private LoopThread _loopThread;
        private bool _reverseMovement;
        private PartakState.PlayerState[] _playerStates;

        private int CurrentStepDirectionIndex
        {
            get => _currentStepDirectionIndex;
            set
            {
                if (value >= _stepDirectionArray.Length)
                    _currentStepDirectionIndex = _stepDirectionArray.Length - value;
                else
                    _currentStepDirectionIndex = value;
            }
        }

        public LoopThread Thread => _loopThread;

        public async Task Initialize(bool startThread)
        {
            await _partakState.Cache(this);
            await Initialize(startThread, _partakState.Service.PlayerStates);
        }

        public async Task Initialize(bool startThread, PartakState.PlayerState[] playerStates)
        {
            await _partakState.Cache(this);
            
            _playerStates = playerStates;
            PriorStartCell = new CellGroup[_playerStates.Length];
            _cellGroupStepArray = new CellGroup[_cellHiearchy.ParticleCellGrid.Grid.Length * 2];
            _cellParticleStore.WinEvent += () => { _reverseMovement = true; };

            if (startThread) StartThread();
        }

        public void StartThread()
        {
            if (_loopThread != null) _loopThread.Stop();
            _loopThread = LoopThread.Create(CalculateGradient, "CellGradient", Priority.Low, _cycleTime);
            _loopThread.Start();
        }

        protected override void OnDestroy()
        {
            if (_loopThread != null)
                _loopThread.Stop();
            base.OnDestroy();
        }

        private void CalculateGradient()
        {
            for (int playerIndex = 0; playerIndex < _partakState.Service.PlayerCount(); playerIndex++)
            {
                if (_playerStates[playerIndex].PlayerMode != PlayerMode.None)
                {
                    CurrentStepDirectionIndex++;
                    int particleIndex = CellUtility.WorldPositionToGridIndex(_cursorStore.CursorPositions[playerIndex].x,_cursorStore.CursorPositions[playerIndex].z, _cellHiearchy.ParticleCellGrid.Dimension);
                    ParticleCell startParticleCell = _cellHiearchy.ParticleCellGrid.Grid[particleIndex];
                    if (startParticleCell.InhabitedBy != 255)
                    {
                        CellGroup startCellGroup = startParticleCell.BottomCellGroup;
                        CalculatePlayerGradient(startCellGroup, playerIndex);
                        PriorStartCell[playerIndex] = startCellGroup;
                    }
                    else
                    {
                        CalculatePlayerGradient(PriorStartCell[playerIndex], playerIndex);
                    }

                    ResetCellHiearchyInStepArray();
                }
            }
        }

        private void CalculatePlayerGradient(CellGroup startCellGroup, int playerIndex)
        {
            int currentIndex = 0;
            int direction, d;
            CellGroup currentCellGroup;
            CellGroup nextCellGroup;
            AddFirstCellGroupToStepArray(startCellGroup);
            while (currentIndex < _lastAddedGroupStepArrayIndex)
            {
                currentCellGroup = _cellGroupStepArray[currentIndex];
                for (d = 0; d < _stepDirectionArray[_currentStepDirectionIndex].Length; ++d)
                {
                    direction = _stepDirectionArray[_currentStepDirectionIndex][d];
                    nextCellGroup = currentCellGroup.DirectionalCellGroupArray[direction];
                    if (nextCellGroup != null && !nextCellGroup.InStepArray)
                    {
                        if (_reverseMovement)
                            nextCellGroup.SetPrimaryDirectionChldParticleCell(direction, playerIndex);
                        else
                            nextCellGroup.SetPrimaryDirectionChldParticleCell(CellUtility.InvertDirection(direction),
                                playerIndex);
                        AddCellGroupToStepArray(nextCellGroup);
                    }
                }
#if DEBUG_GRADIENT
			if (playerIndex == 0) {
				CellGroup nextGroup =
 currentCellGroup.DirectionalCellGroupArray[currentCellGroup.ChildParticleCellArray[0].PrimaryDirectionArray[playerIndex]];
				if (nextGroup != null) {
					Vector3 pos = currentCellGroup.WorldPosition;
					pos.y += (float)currentCellGroup.ParentCellGroups.Length / 4f;
					UnityEngine.Debug.DrawRay(pos, (currentCellGroup.WorldPosition - nextGroup.WorldPosition) * .5f, Color.red);
				}
				UnityEngine.Debug.DrawRay(currentCellGroup.WorldPosition, Vector3.up * _debugRayHeight);
				_debugRayHeight += .001f;
			}
#endif
                CurrentStepDirectionIndex++;
                currentIndex++;
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

        private void AddCellGroupToStepArray(CellGroup cellGroup)
        {
            cellGroup.InStepArray = true;
            for (int i = 0; i < cellGroup.ParentCellGroups.Length; ++i)
                cellGroup.ParentCellGroups[i].InStepArray = true;

            _cellGroupStepArray[_lastAddedGroupStepArrayIndex] = cellGroup;
            _lastAddedGroupStepArrayIndex++;
        }

        /// <summary>
        ///     Iterates through entire CellHiearchy setting the bool InStepArray
        ///     to false to prepare for next cycle of calculation
        /// </summary>
        /// <param name="cellhiearchy">Cellhiearchy.</param>
        private void ResetCellHiearchyInStepArray()
        {
//		flatten only flat grids
//		int limit = _cellHiearchy.CombinedFlatCellGroups.Length;
//		int i;
//		for (i = 0; i < limit; ++i) {
//			_cellHiearchy.CombinedFlatCellGroups[i].InStepArray = 0;
//		}
//		flatten everything
//		for (int i = 0; i < _cellHiearchy.CellGroupGrids.Length; ++i) {
//			for (int o = 0; o < _cellHiearchy.CellGroupGrids[i].Grid.Length; ++o) {
//				if (_cellHiearchy.CellGroupGrids[i].Grid[o] != null)
//					_cellHiearchy.CellGroupGrids[i].Grid[o].InStepArray = false;
//			}
//		}
//		flatten step array
            for (int i = 0; i < _lastAddedGroupStepArrayIndex; ++i)
            {
                _cellGroupStepArray[i].InStepArray = false;
                for (int o = 0; o < _cellGroupStepArray[i].ParentCellGroups.Length; ++o)
                    _cellGroupStepArray[i].ParentCellGroups[o].InStepArray = false;
            }
        }
    }
}