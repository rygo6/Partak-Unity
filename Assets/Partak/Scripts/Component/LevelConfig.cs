using UnityEngine;
using System.Collections;

public class LevelConfig : MonoBehaviour
{
	public Bounds LevelBounds { get { return _levelBounds; } }

	[SerializeField]
	private Bounds _levelBounds;

	[SerializeField]
	private int _fps = 30;

	private void Awake()
	{
		Application.targetFrameRate = _fps;
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawWireCube(_levelBounds.center, _levelBounds.size);
	}
}
