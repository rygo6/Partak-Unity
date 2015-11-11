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

		public Color[] PlayerColor
		{
			get { return _playerColor; }
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
		private Color[] _playerColor;
	}
}