using UnityEngine;
using System.Collections;

public class CursorStore : MonoBehaviour
{
	[SerializeField]
	private Transform[] _cursorTranforms;

	private SkinnedMeshRenderer[] _skinnedMeshRenderers;

	public readonly Vector3[] CursorPositions = new Vector3[4];

	private Bounds _levelBounds;

	private void Awake()
	{
		_levelBounds = FindObjectOfType<LevelConfig>().LevelBounds;
		_skinnedMeshRenderers = new SkinnedMeshRenderer[_cursorTranforms.Length];
		for (int i = 0; i < CursorPositions.Length; ++i)
		{
			_skinnedMeshRenderers[i] = _cursorTranforms[i].GetComponent<SkinnedMeshRenderer>();
		}
		for (int i = 0; i < CursorPositions.Length; ++i)
		{
			CursorPositions[i] = _cursorTranforms[i].position;
		}
	}

	private void LateUpdate()
	{
		ClampToLevelBounds();
		UpdateCursorTransforms();
	}

	private void ClampToLevelBounds()
	{
		for (int i = 0; i < CursorPositions.Length; ++i)
		{
			CursorPositions[i].x = Mathf.Clamp(CursorPositions[i].x, _levelBounds.min.x, _levelBounds.max.x);
			CursorPositions[i].z = Mathf.Clamp(CursorPositions[i].z, _levelBounds.min.z, _levelBounds.max.z);
		}
	}

	private void UpdateCursorTransforms()
	{
		for (int i = 0; i < CursorPositions.Length; ++i)
		{
			_cursorTranforms[i].position = CursorPositions[i];
//			CursorPositions[i] = _cursorTranforms[i].position;
		}
	}

	public void SetPlayerCursorMorph(int playerIndex, float percentage)
	{
		_skinnedMeshRenderers[playerIndex].SetBlendShapeWeight(0, percentage);
	}
}
