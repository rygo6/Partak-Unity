using System;
using System.Collections;
using System.Threading.Tasks;
using GeoTetra.GTCommon.Components;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using UnityEngine;

namespace GeoTetra.Partak
{
    public class CellParticleSpawn : SubscribableBehaviour
    {
        [SerializeField] private PartakStateRef _partakState;
        [SerializeField] private CellHiearchy _cellHiearchy;
        [SerializeField] private CellParticleEngine _cellParticleMover;
        [SerializeField] private CellParticleStore _cellParticleStore;
        [SerializeField] private CursorStore _cursorStore;
        [SerializeField] private LevelConfig _levelConfig;

        public bool SpawnSuccessful { get; private set; }

        public async Task Initialize()
        {
            await _partakState.Cache();
            await Initialize(_levelConfig.Datum.ParticleCount, _partakState.Service.PlayerStates);
        }

        public async Task Initialize(int particleCount, PartakState.PlayerState[] PlayerStates)
        {
            SpawnSuccessful = true;
            
            int playerCount = PlayerStates.Length;
            int activePlayerCount = 0;
            for (int i = 0; i < PlayerStates.Length; ++i)
            {
                if (PlayerStates[i].PlayerMode != PlayerMode.None)
                {
                    activePlayerCount++;
                }
            }
            
            Task[] spawnYield = new Task[playerCount];
            int spawnCount = particleCount / playerCount;
            int startIndex = 0;
            int trailingSpawn = 0;
            bool trailingAdded = false;
            for (int playerIndex = 0; playerIndex < playerCount; ++playerIndex)
            {
                if (PlayerStates[playerIndex].PlayerMode != PlayerMode.None)
                {
                    //in odd numbers, 3, first player may need a few extra particles to produce an even number of particles and have the system work
                    if (!trailingAdded)
                    {
                        trailingAdded = true;
                        trailingSpawn = particleCount - spawnCount * activePlayerCount;
                    }
                    else
                    {
                        trailingSpawn = 0;
                    }

                    int particleIndex = CellUtility.WorldPositionToGridIndex(_cursorStore.CursorPositions[playerIndex].x, _cursorStore.CursorPositions[playerIndex].z, _cellHiearchy.ParticleCellGrid.Dimension);
                    ParticleCell startParticleCell = _cellHiearchy.ParticleCellGrid.Grid[particleIndex];
                    
                    spawnYield[playerIndex] = SpawnPlayerParticles(startParticleCell, playerIndex, startIndex, spawnCount + trailingSpawn);
                    startIndex += spawnCount + trailingSpawn;
                }
            }

            await Task.WhenAll(spawnYield);
            Debug.Log(spawnYield);
        }

        private async Task SpawnPlayerParticles(ParticleCell startParticleCell, int playerIndex, int startIndex, int spawnCount)
        {
            await Task.Yield();
            await Task.Yield();

            int currentIndex = startIndex;
            int endIndex = startIndex + spawnCount;
            int currentAddedIndex = currentIndex + 1;
            ParticleCell[] spawnArray = new ParticleCell[spawnCount * 4];

            spawnArray[currentIndex] = startParticleCell;
            _cellParticleStore.CellParticleArray[currentIndex] = new CellParticle(playerIndex, startParticleCell, _cellParticleStore);

            while (currentIndex < endIndex)
            {
                ParticleCell currentParticleCell = spawnArray[currentIndex];
                CellParticle currentCellParticle = _cellParticleStore.CellParticleArray[currentIndex];
                if (currentCellParticle == null)
                {
                    SpawnSuccessful = false;
                    return;
                }
                _cellParticleStore.CellParticleArray[currentIndex] = currentCellParticle;
                for (int d = 0; d < Direction12.Count; ++d)
                {
                    ParticleCell directionalParticleCell = currentParticleCell.DirectionalParticleCellArray[d];
                    if (directionalParticleCell != null && directionalParticleCell.InhabitedBy == -1 && directionalParticleCell.CellParticle == null && currentAddedIndex < endIndex)
                    {
                        _cellParticleStore.CellParticleArray[currentAddedIndex] = new CellParticle(playerIndex, directionalParticleCell, _cellParticleStore);
                        spawnArray[currentAddedIndex] = directionalParticleCell;
                        currentAddedIndex++;
                    }
                }

                currentIndex++;
                if (currentIndex % 16 == 0) await Task.Yield();
            }
        }
    }
}