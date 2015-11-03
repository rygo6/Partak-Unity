using UnityEngine;
using System.Collections;

namespace Partak
{
	public class CellParticleSpawn : MonoBehaviour
	{
		[SerializeField]
		private Transform[] _spawnTransform;

		[SerializeField]
		private CellHiearchy _cellHiearchy;

		[SerializeField]
		private CellParticleStore _cellParticleStore;

		private PlayerSettings _playerSettings;

		private void Awake()
		{
			_playerSettings = Persistent.Get<PlayerSettings>();
		}

		private void Start()
		{
			int spawnCount = _playerSettings.ParticleCount / _playerSettings.PlayerCount;
			int startIndex = 0;
			for (int i = 0; i < _spawnTransform.Length; ++i)
			{
				int particleIndex = CellUtility.WorldPositionToGridIndex(_spawnTransform[i].position.x, _spawnTransform[i].position.z, _cellHiearchy.ParticleCellGrid.Dimension);
				ParticleCell startParticleCell = _cellHiearchy.ParticleCellGrid.Grid[particleIndex];
				StartCoroutine(SpawnPlayerParticles(startParticleCell, i, startIndex, spawnCount));
				startIndex += spawnCount;
			}
		}

		private IEnumerator SpawnPlayerParticles(ParticleCell startParticleCell, int playerIndex, int startIndex, int spawnCount)
		{
			int currentIndex = startIndex;
			int endIndex = startIndex + spawnCount;
			int currentAddedIndex = currentIndex + 1;
			ParticleCell[] spawnArray = new ParticleCell[spawnCount * 4];
			spawnArray[currentIndex] = startParticleCell;

			while (currentIndex < endIndex)
			{
				ParticleCell currentParticleCell = spawnArray[currentIndex];
				CellParticle currentCellParticle = _cellParticleStore.CellParticleArray[currentIndex];
//				currentCellParticle.PlayerColor = _playerSettings.PlayerColor[playerIndex];
				currentCellParticle.ParticleCell = currentParticleCell;
				currentCellParticle.PlayerIndex = playerIndex;
				currentParticleCell.CellParticle = currentCellParticle;
				for (int d = 0; d < Direction12.Count; ++d)
				{
					ParticleCell directionalParticleCell = currentParticleCell.DirectionalParticleCellArray[d];
					if (directionalParticleCell != null && directionalParticleCell.CellParticle == null && currentAddedIndex < endIndex)
					{
						directionalParticleCell.CellParticle = _cellParticleStore.CellParticleArray[currentAddedIndex];
						spawnArray[currentAddedIndex] = directionalParticleCell;
						currentAddedIndex++;
					}
				}
				currentIndex++;
				if (currentIndex % 4 == 0)
				{
					yield return null;
				}
			}
			yield return null;
		}
	}
}