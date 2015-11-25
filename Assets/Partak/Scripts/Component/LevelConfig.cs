using UnityEngine;
using System.Collections;

public class LevelConfig : MonoBehaviour
{
	public Bounds LevelBounds { get { return _levelBounds; } }

	public Vector2Int RootDimension { get { return _rootDimension; } }

	[SerializeField]
	private Bounds _levelBounds;

	[SerializeField]
	private Vector2Int _rootDimension = new Vector2Int(192, 192);

	[SerializeField]
	private int _fps = 30;

	private void Awake()
	{
		Application.targetFrameRate = _fps;
	}

	private void OnDrawGizmos()
	{
		_levelBounds.center = new Vector3(
			(_rootDimension.x / 2f) / 10f,
			0f,
			(_rootDimension.y / 2f) / 10f);
		_levelBounds.size = new Vector3(
			_rootDimension.x / 10f,
			0f, 
			_rootDimension.y / 10f);

		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(_levelBounds.center, _levelBounds.size);
	}
}
