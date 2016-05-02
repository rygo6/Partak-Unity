using UnityEngine;

namespace Partak
{
    public class LevelConfig : MonoBehaviour
	{
		public Bounds LevelBounds { get { return _levelBounds; } }

		public Vector2Int RootDimension { get { return _rootDimension; } }

		public int ParticleCount { get { return _particleCount; } }

		public int MoveCycleTime { get { return _moveCycleTime; } }

		[SerializeField]
		private Bounds _levelBounds;

		[SerializeField]
		private Vector2Int _rootDimension = new Vector2Int(192, 192);

		[SerializeField]
		private int _fps = 30;

		[SerializeField]
		private int _particleCount = 5000;

		[SerializeField]
		private int _moveCycleTime = 16;

		private void Awake()
		{
			Application.targetFrameRate = _fps;
		}

		private void OnDrawSelectedGizmos()
		{
			Vector3 center = new Vector3(
				                (_rootDimension.X / 2f) / 10f,
				                0f,
				                (_rootDimension.Y / 2f) / 10f);
			Vector3 size = new Vector3(
				              _rootDimension.X / 10f,
				              0f, 
				              _rootDimension.Y / 10f);

			if (_levelBounds.center != center || _levelBounds.size != size)
			{
				_levelBounds.center = center;
				_levelBounds.size = size;
			}

			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(_levelBounds.center, _levelBounds.size);
		}
	}
}