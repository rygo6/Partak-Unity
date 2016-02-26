using UnityEngine;
using System;
using System.Collections;

namespace Partak {
public class CellParticleSpawn : MonoBehaviour {
	
	public event Action SpawnComplete;
	[SerializeField] CellHiearchy _cellHiearchy;
	[SerializeField] CellParticleStore _cellParticleStore;
	[SerializeField] CellParticleEngine _cellParticleMover;
	CursorStore _cursorStore;

	private IEnumerator Start() {
		LevelConfig levelConfig = FindObjectOfType<LevelConfig>();
		_cursorStore = FindObjectOfType<CursorStore>();
		MenuConfig playerSettings = Persistent.Get<MenuConfig>();
		YieldInstruction[] spawnYield = new YieldInstruction[MenuConfig.MaxPlayers];
		int spawnCount = levelConfig.ParticleCount / playerSettings.ActivePlayerCount();
		int startIndex = 0;
		int trailingSpawn = 0;
		bool trailingAdded = false;
		for (int playerIndex = 0; playerIndex < MenuConfig.MaxPlayers; ++playerIndex) {
			if (playerSettings.PlayerActive(playerIndex)) {
				//in odd numbers, 3, first player may need a few extra particles to produce an even number of particles and have the system work
				if (!trailingAdded) {	
					trailingAdded = true;
					trailingSpawn = levelConfig.ParticleCount - (spawnCount * playerSettings.ActivePlayerCount());
				} else
					trailingSpawn = 0;

				int particleIndex = CellUtility.WorldPositionToGridIndex(_cursorStore.CursorPositions[playerIndex].x, _cursorStore.CursorPositions[playerIndex].z, _cellHiearchy.ParticleCellGrid.Dimension);
				ParticleCell startParticleCell = _cellHiearchy.ParticleCellGrid.Grid[particleIndex];
				spawnYield[playerIndex] = StartCoroutine(SpawnPlayerParticles(startParticleCell, playerIndex, startIndex, spawnCount + trailingSpawn));
				startIndex += spawnCount + trailingSpawn;
			}
		}

		for (int i = 0; i < spawnYield.Length; ++i) {
			if (spawnYield[i] != null)
				yield return spawnYield[i];
		}
				
		SpawnComplete();
	}
			
	private IEnumerator SpawnPlayerParticles(ParticleCell startParticleCell, int playerIndex, int startIndex, int spawnCount) {
		yield return null;
		yield return null;

		int currentIndex = startIndex;
		int endIndex = startIndex + spawnCount;
		int currentAddedIndex = currentIndex + 1;
		ParticleCell[] spawnArray = new ParticleCell[spawnCount * 4];

		spawnArray[currentIndex] = startParticleCell;
		_cellParticleStore.CellParticleArray[currentIndex] = new CellParticle(playerIndex, startParticleCell, _cellParticleStore); 

		while (currentIndex < endIndex) {
			ParticleCell currentParticleCell = spawnArray[currentIndex];
			CellParticle currentCellParticle = _cellParticleStore.CellParticleArray[currentIndex];
			_cellParticleStore.CellParticleArray[currentIndex] = currentCellParticle;
			for (int d = 0; d < Direction12.Count; ++d) {
				ParticleCell directionalParticleCell = currentParticleCell.DirectionalParticleCellArray[d];
				if (directionalParticleCell != null && directionalParticleCell.InhabitedBy == -1 && directionalParticleCell.CellParticle == null && currentAddedIndex < endIndex) {
					_cellParticleStore.CellParticleArray[currentAddedIndex] = new CellParticle(playerIndex, directionalParticleCell, _cellParticleStore);
					spawnArray[currentAddedIndex] = directionalParticleCell;
					currentAddedIndex++;
				}
			}
			currentIndex++;
			if (currentIndex % 16 == 0) {
				yield return null;
			}
		}
		yield return null;
	}
}
}