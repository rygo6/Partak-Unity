﻿using UnityEngine;
using System.Collections;

namespace Partak
{
	public class CursorStore : MonoBehaviour
	{
		[SerializeField]
		private Transform[] _cursorTranforms;

		private SkinnedMeshRenderer[] _skinnedMeshRenderers;

		public readonly Vector3[] CursorPositions = new Vector3[PlayerSettings.MaxPlayers];

		private readonly LineRenderer[] _lineRenderers = new LineRenderer[PlayerSettings.MaxPlayers];

		private Bounds _levelBounds;

		private bool _updateCursors = true;

		private void Awake()
		{
			_levelBounds = FindObjectOfType<LevelConfig>().LevelBounds;
			_skinnedMeshRenderers = new SkinnedMeshRenderer[_cursorTranforms.Length];
			for (int i = 0; i < CursorPositions.Length; ++i)
			{
				_skinnedMeshRenderers[i] = _cursorTranforms[i].GetComponent<SkinnedMeshRenderer>();
				CursorPositions[i] = _cursorTranforms[i].position;
			}
		}

		private void LateUpdate()
		{
			if (_updateCursors)
			{
				UpdateCursorTransforms();
			}
		}

		public void SetCursorPositionClamp(int playerIndex, Vector3 deltaPosition)
		{
			Vector3 newPos = CursorPositions[playerIndex];
			newPos.x += deltaPosition.x;
			newPos.z += deltaPosition.z;
			newPos.x = Mathf.Clamp(newPos.x, _levelBounds.min.x, _levelBounds.max.x);
			newPos.z = Mathf.Clamp(newPos.z, _levelBounds.min.z, _levelBounds.max.z);
			CursorPositions[playerIndex] = newPos;
		}

		private void UpdateCursorTransforms()
		{
			for (int i = 0; i < CursorPositions.Length; ++i)
			{
				_cursorTranforms[i].position = CursorPositions[i];
			}
		}

		public void SetPlayerCursorMorph(int playerIndex, float percentage)
		{
			_skinnedMeshRenderers[playerIndex].SetBlendShapeWeight(0, percentage);
		}

		public void PlayerWin(int playerIndex)
		{
			_updateCursors = false;
			StartCoroutine(WinCursorTweenCoroutine(_cursorTranforms[playerIndex]));
		}

		public void PlayerLose(int playerIndex)
		{
			StartCoroutine(LoseCursorTweenCoroutine(_cursorTranforms[playerIndex]));		
		}

		private IEnumerator WinCursorTweenCoroutine(Transform cursorTransform)
		{
			cursorTransform.parent = Camera.main.transform;
			Vector3 startPos = cursorTransform.localPosition;
			Vector3 endPos = new Vector3(0f, 0f, 5f);

			float time = 0f;
			while (time < 1f)
			{
				time += Time.deltaTime;
				cursorTransform.localPosition = Vector3.Lerp(startPos, endPos, time);
				yield return null;
			}
		}

		private IEnumerator LoseCursorTweenCoroutine(Transform cursorTransform)
		{
			Vector3 startPos = cursorTransform.position;
			Vector3 endPos = new Vector3(startPos.x, startPos.y, startPos.z - 20f);

			float time = 0f;
			while (time < 1f)
			{
				time += Time.deltaTime;
				cursorTransform.position = Vector3.Lerp(startPos, endPos, time);
				yield return null;
			}

			cursorTransform.gameObject.SetActive(false);
		}

	}
}