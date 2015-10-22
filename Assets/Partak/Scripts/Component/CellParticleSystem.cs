using UnityEngine;
using System.Collections;

namespace Partak
{
	[RequireComponent(typeof(ParticleSystem))]
	public class CellParticleSystem : MonoBehaviour
	{
		private ParticleSystem particleSystem { get; set; }

		private void Reset()
		{
			particleSystem = GetComponent<ParticleSystem>();
		}
	}
}
