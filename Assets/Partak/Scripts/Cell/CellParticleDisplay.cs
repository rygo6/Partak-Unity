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

		private PlayerSettings _playerSettings;

		private void Awake()
		{
			_playerSettings = Persistent.Get<PlayerSettings>();

			int systemCount = _cellHiearchy.CellGroupGridArray.Length;
			int particleCount = _playerSettings.ParticleCount;

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

				if (particleCell != null && particleCell.InhabitedBy != -1)
				{
					particleArray[currentParticle].position = particleCell.WorldPosition;
					particleArray[currentParticle].color = particleCell.CellParticle.Color;
					currentParticle++;
				}
			}
			_cellParticleSystemArray[0].UpdateParticleSystem(currentParticle);
		}
	}
}
