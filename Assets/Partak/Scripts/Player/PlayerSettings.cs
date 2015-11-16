using UnityEngine;
using System.Collections;

namespace Partak
{
	public class PlayerSettings : MonoBehaviour
	{
		public int PlayerCount
		{
			get { return _playerCount; }
		}

		public Color[] PlayerColors
		{
			get { return _playerColors; }
		}

		public int ParticleCount
		{
			get { return _particleCount; }
		}

		[SerializeField]
		private int _playerCount;

		[SerializeField]
		private int _particleCount;

		[SerializeField]
		[UnityEngine.Serialization.FormerlySerializedAs("_playerColor")]
		private Color[] _playerColors;
	}
}