using UnityEngine;
using System.Collections;

namespace Partak
{
	[RequireComponent(typeof(ParticleSystem))]
	public class CellParticleSystem : MonoBehaviour
	{
		[SerializeField]
		private ParticleSystem _particleSystem;

		public ParticleSystem.Particle[] ParticleArray { get; private set; }

		private void Reset()
		{
			_particleSystem = GetComponent<ParticleSystem>();
		}

		public void Initialize(int particleCount, float particleSize)
		{
			ParticleArray = new ParticleSystem.Particle[particleCount];

			for (int i = 0; i < ParticleArray.Length; ++i)
			{
				ParticleArray[i].size = particleSize;
			}

			_particleSystem.maxParticles = particleCount;
		}

		public void UpdateParticleSystem(int maxParticles)
		{
			_particleSystem.SetParticles(ParticleArray, maxParticles);
		}
	}
}
