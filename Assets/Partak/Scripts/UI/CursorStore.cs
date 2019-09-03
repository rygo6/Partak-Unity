using UnityEngine;
using System.Collections;

namespace Partak
{
    public class CursorStore : MonoBehaviour
    {
        [SerializeField] private GameState _gameState;
        [SerializeField] private Transform[] _cursorTranforms;

        public Vector3[] CursorPositions { get; private set; }
        private SkinnedMeshRenderer[] _skinnedMeshRenderers;
        private Bounds _levelBounds;
        private bool _updateCursors = true;

        private void Awake()
        {
            CursorPositions = new Vector3[_gameState.PlayerCount()];
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

        public void SetCursorPositionClamp(int playerIndex, Vector3 position)
        {
            position.x = Mathf.Clamp(position.x, _levelBounds.min.x, _levelBounds.max.x);
            position.z = Mathf.Clamp(position.z, _levelBounds.min.z, _levelBounds.max.z);
            CursorPositions[playerIndex] = position;
        }

        public void SetCursorDeltaPositionClamp(int playerIndex, Vector3 deltaPosition)
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
            StartCoroutine(LoseCursorTweenCoroutine(playerIndex));
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

        private IEnumerator LoseCursorTweenCoroutine(int playerIndex)
        {
            Vector3 startScale = Vector3.one;
            Vector3 endScale = Vector3.zero;
            Vector3 newEuler;

            float time = 0f;
            while (time < 1f)
            {
                time += Time.deltaTime;
                _cursorTranforms[playerIndex].localScale = Vector3.Lerp(startScale, endScale, time);
                newEuler = _cursorTranforms[playerIndex].eulerAngles;
                newEuler.y += Time.deltaTime * 120f;
                _cursorTranforms[playerIndex].eulerAngles = newEuler;
                yield return null;
            }

            _cursorTranforms[playerIndex].gameObject.SetActive(false);
        }
    }
}