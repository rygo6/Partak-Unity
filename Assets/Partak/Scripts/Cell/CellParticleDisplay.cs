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

		private CellParticleSystem[] _cellParticleSystems;

		private Color[] _playerColors;

		private PlayerSettings _playerSettings;

		private int[] _levelCount;

		private void Awake()
		{
			_playerSettings = Persistent.Get<PlayerSettings>();
			int systemCount = _cellHiearchy.CellGroupGridArray.Length;

			//you add additional positions onto the particle count as a buffer in case when it is reading in
			//particle positions one of the particles is updated from another thread and ends up registering twice
			int particleCount = _playerSettings.ParticleCount + (_playerSettings.ParticleCount / 4);

			_cellParticleSystems = new CellParticleSystem[systemCount];
			for (int i = 0; i < systemCount; ++i)
			{
				_cellParticleSystems[i] = Instantiate(_cellParticleSystemPrefab).GetComponent<CellParticleSystem>();
				float particleSize = Mathf.Pow(2, i);
				particleSize /= 10f;
				_cellParticleSystems[i].Initialize(particleCount, particleSize);

				particleCount /= 2;
			}
		}

		private void Update()
		{
			DrawParticleSystems();
		}

		private void DrawCellGroup(CellGroup cellGroup)
		{
			int parentLevel = cellGroup.CellGroupGrid.ParentLevel;
			int levelCount = (int)Mathf.Pow(4, parentLevel) - (parentLevel + 1);
			for (int playerIndex = 0; playerIndex < _playerSettings.PlayerCount; ++playerIndex)
			{
				if (cellGroup.PlayerParticleCount[playerIndex] > levelCount)
				{
					_cellParticleSystems[parentLevel].SetNextParticle(
						cellGroup.WorldPosition,
						_playerSettings.PlayerColors[playerIndex]);
					return;
				}
			}

			if (cellGroup.ChildCellGroupArray != null)
			{
				for (int i = 0; i < cellGroup.ChildCellGroupArray.Length; ++i)
				{
					DrawCellGroup(cellGroup.ChildCellGroupArray[i]);
				}
			}
		}

		private void ResetParticleSystemsCount()
		{
			for (int i = 0; i < _cellParticleSystems.Length; ++i)
			{
				_cellParticleSystems[i].ResetCount();
			}
		}

		private void UpdateParticleSystems()
		{
			for (int i = 0; i < _cellParticleSystems.Length; ++i)
			{
				_cellParticleSystems[i].UpdateParticleSystem();
			}
		}

		private void DrawParticleSystems()
		{
			ResetParticleSystemsCount();

			CellGroup cellGroup;
			CellGroupGrid cellGroupGrid;
			int cellIndex;
			int cellLimit;
			for (int levelIndex = 0; levelIndex < _cellHiearchy.CellGroupGridArray.Length; ++levelIndex)
			{
				cellGroupGrid = _cellHiearchy.CellGroupGridArray[levelIndex];
				cellLimit = cellGroupGrid.FlatGrid.Length;
				for (cellIndex = 0; cellIndex < cellLimit; ++cellIndex)
				{
					DrawCellGroup(cellGroupGrid.FlatGrid[cellIndex]);
				}
			}

			UpdateParticleSystems();
		}
								

	}
}
