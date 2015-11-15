using UnityEngine;
using System.Collections;

public class CursorStore : MonoBehaviour
{
	[SerializeField]
	private Transform[] _cursorTranforms;

	public readonly Vector3[] CursorPositions = new Vector3[4];

	private void Update()
	{
		for (int i = 0; i < CursorPositions.Length; ++i)
		{
			CursorPositions[i] = _cursorTranforms[i].position;
		}
	}
}
