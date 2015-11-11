using UnityEngine;
using System.Collections;

namespace Partak
{
	public class CellParticleStore : MonoBehaviour
	{
		public CellParticle[] CellParticleArray { get; private set; }

		private void Awake()
		{
			int particleCount = Persistent.Get<PlayerSettings>().ParticleCount;
			CellParticleArray = new CellParticle[particleCount];
		}
	}
}
