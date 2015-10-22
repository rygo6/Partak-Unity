using UnityEngine;
using System.Collections;

namespace Partak
{
	[RequireComponent(typeof(ParticleSystem))]
	public class CellParticleSystem : MonoBehaviour
	{
		[SerializeField]
		private ParticleSystem _particleSystem;

		private ParticleSystem.Particle[] _particleArray;

		private void Reset()
		{
			_particleSystem = GetComponent<ParticleSystem>();
		}

		public void Initialize(int particleCount, int particleSize)
		{
			_particleArray = new ParticleSystem.Particle[particleCount];

			for (int i = 0; i < _particleArray.Length; ++i)
			{
				_particleArray[i].size = particleSize;
			}

			_particleSystem.maxParticles = particleCount;
		}
	}
}
