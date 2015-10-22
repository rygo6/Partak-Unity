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

		[SerializeField]
		private int _particleCount;

		private int _playerCount = 4;

		private void Awake()
		{
			int systemCount = _cellHiearchy.cellGroupGridArray.Length;
			int particleCount = _particleCount;
			_cellParticleSystemArray = new CellParticleSystem[systemCount];
			for (int i = 0; i < systemCount; ++i)
			{
				_cellParticleSystemArray[i] = Instantiate(_cellParticleSystemPrefab).GetComponent<CellParticleSystem>();
				_cellParticleSystemArray[i].Initialize(_particleCount, 1);

				_particleCount /= 4;
			}
		}
	}
}
