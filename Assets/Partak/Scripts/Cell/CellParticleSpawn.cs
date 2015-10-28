using UnityEngine;
using System.Collections;

namespace Partak
{
	public class CellParticleSpawn : MonoBehaviour
	{
		[SerializeField]
		private CellHiearchy _cellHiearchy;

		[SerializeField]
		private CellParticleStore _cellParticleStore;

		[SerializeField]
		private int _playerIndex;

		private PlayerSettings _playerSettings;

		private void Awake()
		{
			_playerSettings = Persistent.Get<PlayerSettings>();
		}

		private void Start()
		{
			int particleCount = _playerSettings.ParticleCount;
			int particleIndex = CellUtility.WorldPositionToGridIndex(transform.position.x, transform.position.z, _cellHiearchy.ParticleCellGrid.Dimension);
			ParticleCell startParticleCell = _cellHiearchy.ParticleCellGrid.Grid[particleIndex];
			StartCoroutine(SpawnPlayerParticles(startParticleCell, _playerIndex, particleCount));
		}

		private IEnumerator SpawnPlayerParticles(ParticleCell startParticleCell, int playerIndex, int spawnCount)
		{
			int currentIndex = 0;
			int maxIndex = 1;
			ParticleCell[] spawnArray = new ParticleCell[spawnCount];
			spawnArray[0] = startParticleCell;

			while (currentIndex < spawnCount)
			{
				ParticleCell currentParticleCell = spawnArray[currentIndex];
				CellParticle currentCellParticle = _cellParticleStore.CellParticleArray[currentIndex];
				currentCellParticle.Color = _playerSettings.PlayerColor[playerIndex];
				currentCellParticle.ParticleCell = currentParticleCell;
				currentCellParticle.PlayerIndex = playerIndex;
				currentParticleCell.CellParticle = currentCellParticle;
				for (int d = 0; d < Direction12.Count; ++d)
				{
					ParticleCell directionalParticleCell = currentParticleCell.DirectionalParticleCellArray[d];
					if (directionalParticleCell != null && directionalParticleCell.CellParticle == null && maxIndex < spawnCount)
					{
						directionalParticleCell.CellParticle = _cellParticleStore.CellParticleArray[maxIndex];
						spawnArray[maxIndex] = directionalParticleCell;
						maxIndex++;
					}
				}
				currentIndex++;
//				yield return null;
			}
			yield return null;
			Debug.Log("Spawned " + currentIndex + " Particles");
		}
	}
}