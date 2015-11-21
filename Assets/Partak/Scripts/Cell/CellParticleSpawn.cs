using UnityEngine;
using System.Collections;

namespace Partak
{
	public class CellParticleSpawn : MonoBehaviour
	{
		[SerializeField]
		private CursorStore _cursorStore;

		[SerializeField]
		private CellHiearchy _cellHiearchy;

		[SerializeField]
		private CellParticleStore _cellParticleStore;

		[SerializeField]
		private CellParticleMover _cellParticleMover;

		private IEnumerator Start()
		{
			PlayerSettings playerSettings = Persistent.Get<PlayerSettings>();
			YieldInstruction[] spawnYield = new YieldInstruction[PlayerSettings.MaxPlayers];
			int particleCount = playerSettings.ParticleCount;
			int spawnCount = playerSettings.ParticleCount / playerSettings.ActivePlayerCount();
			int startIndex = 0;
			for (int playerIndex = 0; playerIndex < PlayerSettings.MaxPlayers; ++playerIndex)
			{
				if (playerSettings.PlayerActive(playerIndex))
				{
					int particleIndex = CellUtility.WorldPositionToGridIndex(_cursorStore.CursorPositions[playerIndex].x, _cursorStore.CursorPositions[playerIndex].z, _cellHiearchy.ParticleCellGrid.Dimension);
					ParticleCell startParticleCell = _cellHiearchy.ParticleCellGrid.Grid[particleIndex];
					spawnYield[playerIndex] = StartCoroutine(SpawnPlayerParticles(startParticleCell, playerIndex, startIndex, spawnCount));
					startIndex += spawnCount;
				}
			}

			for (int i = 0; i < spawnYield.Length; ++i)
			{
				if (spawnYield[i] != null)
					yield return spawnYield[i];
			}

			_cellParticleMover.StartThread();
		}
			
		private IEnumerator SpawnPlayerParticles(ParticleCell startParticleCell, int playerIndex, int startIndex, int spawnCount)
		{
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
				_cellParticleStore.CellParticleArray[currentIndex] = currentCellParticle;
				for (int d = 0; d < Direction12.Count; ++d)
				{
					ParticleCell directionalParticleCell = currentParticleCell.DirectionalParticleCellArray[d];
					if (directionalParticleCell != null && directionalParticleCell.CellParticle == null && currentAddedIndex < endIndex)
					{
						_cellParticleStore.CellParticleArray[currentAddedIndex] = new CellParticle(playerIndex, directionalParticleCell, _cellParticleStore);
						spawnArray[currentAddedIndex] = directionalParticleCell;
						currentAddedIndex++;
					}
				}
				currentIndex++;
				if (currentIndex % 16 == 0)
				{
					yield return null;
				}
			}
			yield return null;
		}
	}
}