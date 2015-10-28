using UnityEngine;
using System.Collections;

namespace Partak
{
	public class CellParticleStore : MonoBehaviour
	{
		public CellParticle[] CellParticleArray
		{
			get { return _cellParticleArray; }
		}

		/// <summary> Primary store of all particles and their relevant game data. </summary>
		private CellParticle[] _cellParticleArray;

		private void Awake()
		{
			int particleCount = Persistent.Get<PlayerSettings>().ParticleCount;
			_cellParticleArray = new CellParticle[particleCount];
			for (int i = 0; i < particleCount; ++i)
			{
				_cellParticleArray[i] = new CellParticle();
			}
		}
	}
}
