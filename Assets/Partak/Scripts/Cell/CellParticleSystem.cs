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

		private int _currentIndex;

		private void Reset()
		{
			Init();
		}

		[ContextMenu("Init")]
		private void Init()
		{
			_particleSystem = GetComponent<ParticleSystem>();
		}

		public void Initialize(int particleCount, float particleSize)
		{
			_particleArray = new ParticleSystem.Particle[particleCount];

			for (int i = 0; i < _particleArray.Length; ++i)
			{
				_particleArray[i].startSize = particleSize;
			}

			_particleSystem.maxParticles = particleCount;
		}

		public void ResetCount()
		{
			_currentIndex = 0;
		}

		public void SetNextParticle(Vector3 position, Color color)
		{
			_particleArray[_currentIndex].position = position;
			_particleArray[_currentIndex].startColor = color;
			_currentIndex++;
		}

		public void UpdateParticleSystem()
		{
			_particleSystem.SetParticles(_particleArray, _currentIndex);
		}
	}
}
