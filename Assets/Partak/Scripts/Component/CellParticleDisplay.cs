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

		private int _playerCount = 4;

		private void Awake()
		{
			int systemCount = _cellHiearchy.CellGroupGridArray.Length;
			int particleCount = PlayerUtility.maxParticleCount;
			_cellParticleSystemArray = new CellParticleSystem[systemCount];
			for (int i = 0; i < systemCount; ++i)
			{
				_cellParticleSystemArray[i] = Instantiate(_cellParticleSystemPrefab).GetComponent<CellParticleSystem>();
				_cellParticleSystemArray[i].Initialize(particleCount, ((float)i + 1f ) / 10f);

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

				if (particleCell != null && particleCell.InhabitedBy == 0)
				{
					particleArray[currentParticle].position = particleCell.WorldPosition;
					currentParticle++;
				}
			}
			_cellParticleSystemArray[0].UpdateParticleSystem(currentParticle);
		}
	}
}
