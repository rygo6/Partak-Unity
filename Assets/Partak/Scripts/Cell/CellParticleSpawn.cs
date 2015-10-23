using UnityEngine;
using System.Collections;

namespace Partak
{
	public class CellParticleSpawn : MonoBehaviour
	{
		[SerializeField]
		private CellHiearchy _cellHiearchy;

		[SerializeField]
		private int _playerIndex;

		[SerializeField]
		private int _particleCount;

		private void Start()
		{
			int particleIndex = CellUtility.WorldPositionToGridIndex(transform.position.x, transform.position.z, _cellHiearchy.ParticleCellGrid.Dimension);
			ParticleCell startParticleCell = _cellHiearchy.ParticleCellGrid.Grid[particleIndex];
			StartCoroutine(SpawnPlayerParticles(startParticleCell, _playerIndex, _particleCount));
		}

		private IEnumerator SpawnPlayerParticles(ParticleCell startParticleCell, int playerIndex, int spawnCount)
		{
			int particlesSpawned = 0;
			int currentIndex = 0;
			int maxIndex = 1;
			ParticleCell[] spawnArray = new ParticleCell[spawnCount];
			spawnArray[0] = startParticleCell;

			while (currentIndex < maxIndex)
			{
				for (int d = 0; d < Direction12.Count; ++d)
				{
					ParticleCell cell = spawnArray[currentIndex].DirectionalParticleCellArray[d];
					if (cell != null && cell.InhabitedBy == -1)
					{
						cell.InhabitedBy = playerIndex;
						cell.BottomCellGroup.AddPlayerParticle(playerIndex);
						if (maxIndex < spawnCount)
						{
							spawnArray[maxIndex] = cell;
							maxIndex++;
						}
					}
				}
				currentIndex++;
				yield return null;
			}
		}
	}
}