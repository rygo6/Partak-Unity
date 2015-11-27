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

		private bool _floatParticles;

		private void Awake()
		{
			_playerSettings = Persistent.Get<PlayerSettings>();
			int systemCount = _cellHiearchy.CellGroupGridArray.Length;
			LevelConfig levelConfig = FindObjectOfType<LevelConfig>();

			//you add additional positions onto the particle count as a buffer in case when it is reading in
			//particle positions one of the particles is updated from another thread and ends up registering twice
			int particleCount = levelConfig.ParticleCount + (levelConfig.ParticleCount / 4);

			_cellParticleSystems = new CellParticleSystem[systemCount];
			for (int i = 0; i < systemCount; ++i)
			{
				_cellParticleSystems[i] = Instantiate(_cellParticleSystemPrefab).GetComponent<CellParticleSystem>();
				_cellParticleSystems[i].transform.position = levelConfig.LevelBounds.center;
				_cellParticleSystems[i].transform.parent = Camera.main.transform;
				float particleSize = Mathf.Pow(2, i);
				particleSize /= 10f;
				_cellParticleSystems[i].Initialize(particleCount, particleSize);

				particleCount /= 2;
			}

			FindObjectOfType<CellParticleStore>().WinEvent += () =>
			{
				_floatParticles = true;
			};
		}

		private void Update()
		{
			DrawParticleSystems();
			if (_floatParticles)
				FloatParticles();
		}

		private void DrawCellGroup(CellGroup cellGroup)
		{
			int parentLevel = cellGroup.CellGroupGrid.ParentLevel;
			int levelCount = (int)Mathf.Pow(4, parentLevel) - ((parentLevel / 2) + 1);
			for (int playerIndex = 0; playerIndex < PlayerSettings.MaxPlayers; ++playerIndex)
			{
				if (_playerSettings.PlayerActive(playerIndex) &&
				    cellGroup.PlayerParticleCount[playerIndex] > levelCount)
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

		private void FloatParticles()
		{
			CellGroupGrid cellGroupGrid;
			CellGroup cellGroup;
			Vector3 newPos;
			int cellIndex;
			int cellLimit;
			for (int levelIndex = 0; levelIndex < _cellHiearchy.CellGroupGridArray.Length; ++levelIndex)
			{
				cellGroupGrid = _cellHiearchy.CellGroupGridArray[levelIndex];
				cellLimit = cellGroupGrid.Grid.Length;
				for (cellIndex = 0; cellIndex < cellLimit; ++cellIndex)
				{
					cellGroup = cellGroupGrid.Grid[cellIndex];
					if (cellGroup != null)
					{
						newPos = cellGroup.WorldPosition;
						newPos.y += Time.deltaTime * ((float)cellIndex / (float)cellLimit);
						cellGroup.WorldPosition = newPos;
					}
				}
			}
		}

		private void DrawParticleSystems()
		{
			ResetParticleSystemsCount();

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
