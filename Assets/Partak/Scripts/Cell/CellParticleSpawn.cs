using System;
using System.Collections;
using GeoTetra.GTCommon.Components;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using UnityEngine;

namespace GeoTetra.Partak
{
    public class CellParticleSpawn : SubscribableBehaviour
    {
        [SerializeField] private ServiceReference _gameStateReference;
        [SerializeField] private CellHiearchy _cellHiearchy;
        [SerializeField] private CellParticleEngine _cellParticleMover;
        [SerializeField] private CellParticleStore _cellParticleStore;
        [SerializeField] private CursorStore _cursorStore;
        [SerializeField] private LevelConfig _levelConfig;
        private GameState _gameState;

        private void Awake()
        {
            _gameState = _gameStateReference.Service<GameState>();
        }

        public IEnumerator Initialize()
        {
            YieldInstruction[] spawnYield = new YieldInstruction[_gameState.PlayerCount()];
            int spawnCount = _levelConfig.Datum.ParticleCount / _gameState.PlayerCount();
            int startIndex = 0;
            int trailingSpawn = 0;
            bool trailingAdded = false;
            for (int playerIndex = 0; playerIndex < _gameState.PlayerCount(); ++playerIndex)
                if (_gameState.PlayerActive(playerIndex))
                {
                    //in odd numbers, 3, first player may need a few extra particles to produce an even number of particles and have the system work
                    if (!trailingAdded)
                    {
                        trailingAdded = true;
                        trailingSpawn = _levelConfig.Datum.ParticleCount - spawnCount * _gameState.ActivePlayerCount();
                    }
                    else
                    {
                        trailingSpawn = 0;
                    }

                    int particleIndex = CellUtility.WorldPositionToGridIndex(
                        _cursorStore.CursorPositions[playerIndex].x, _cursorStore.CursorPositions[playerIndex].z,
                        _cellHiearchy.ParticleCellGrid.Dimension);
                    ParticleCell startParticleCell = _cellHiearchy.ParticleCellGrid.Grid[particleIndex];
                    spawnYield[playerIndex] = StartCoroutine(SpawnPlayerParticles(startParticleCell, playerIndex,
                        startIndex, spawnCount + trailingSpawn));
                    startIndex += spawnCount + trailingSpawn;
                }

            for (int i = 0; i < spawnYield.Length; ++i)
                if (spawnYield[i] != null)
                    yield return spawnYield[i];
        }

        private IEnumerator SpawnPlayerParticles(ParticleCell startParticleCell, int playerIndex, int startIndex, int spawnCount)
        {
            yield return null;
            yield return null;

            int currentIndex = startIndex;
            int endIndex = startIndex + spawnCount;
            int currentAddedIndex = currentIndex + 1;
            ParticleCell[] spawnArray = new ParticleCell[spawnCount * 4];

            spawnArray[currentIndex] = startParticleCell;
            _cellParticleStore.CellParticleArray[currentIndex] =
                new CellParticle(playerIndex, startParticleCell, _cellParticleStore);

            while (currentIndex < endIndex)
            {
                ParticleCell currentParticleCell = spawnArray[currentIndex];
                CellParticle currentCellParticle = _cellParticleStore.CellParticleArray[currentIndex];
                _cellParticleStore.CellParticleArray[currentIndex] = currentCellParticle;
                for (int d = 0; d < Direction12.Count; ++d)
                {
                    ParticleCell directionalParticleCell = currentParticleCell.DirectionalParticleCellArray[d];
                    if (directionalParticleCell != null && directionalParticleCell.InhabitedBy == -1 &&
                        directionalParticleCell.CellParticle == null && currentAddedIndex < endIndex)
                    {
                        _cellParticleStore.CellParticleArray[currentAddedIndex] =
                            new CellParticle(playerIndex, directionalParticleCell, _cellParticleStore);
                        spawnArray[currentAddedIndex] = directionalParticleCell;
                        currentAddedIndex++;
                    }
                }

                currentIndex++;
                if (currentIndex % 16 == 0) yield return null;
            }

            yield return null;
        }
    }
}