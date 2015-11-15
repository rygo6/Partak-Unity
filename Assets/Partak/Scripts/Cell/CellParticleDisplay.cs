using UnityEngine;
using System.Collections;

namespace Partak
{
	public class CellParticleDisplay : MonoBehaviour
	{
		[SerializeField]
		private CellParticleSystem _cellParticleSystemPrefab;

		[SerializeField]
		private CellHiearchy _cellHiearchy;

		private CellParticleSystem[] _cellParticleSystemArray;

		private void Awake()
		{
			int systemCount = _cellHiearchy.CellGroupGridArray.Length;
			//you add 100 onto the particle count as a buffer in case when it is reading in
			//particle positions one of the particles is updated from another thread and ends up registering twice
			int particleCount = Persistent.Get<PlayerSettings>().ParticleCount + 100;

			_cellParticleSystemArray = new CellParticleSystem[systemCount];
			for (int i = 0; i < systemCount; ++i)
			{
				_cellParticleSystemArray[i] = Instantiate(_cellParticleSystemPrefab).GetComponent<CellParticleSystem>();
				_cellParticleSystemArray[i].Initialize(particleCount, (((float)i + 1f) / 10f) * 1.1f);

				particleCount /= 4;
			}
		}

		private void Update()
		{
			TestDraw();
		}

		private void TestDraw()
		{
			int currentParticle = 0;
			ParticleSystem.Particle[] particleArray = _cellParticleSystemArray[0].ParticleArray;
			for (int i = 0; i < _cellHiearchy.ParticleCellGrid.Grid.Length; ++i)
			{
				ParticleCell particleCell = _cellHiearchy.ParticleCellGrid.Grid[i];
				if (particleCell != null)
				{
					CellParticle cellparticle = particleCell.CellParticle;
					if (cellparticle != null)
					{
						particleArray[currentParticle].position = particleCell.WorldPosition;
						particleArray[currentParticle].color = cellparticle.PlayerColor;
						currentParticle++;
					}
				}
			}
			_cellParticleSystemArray[0].UpdateParticleSystem(currentParticle);
		}
	}
}
