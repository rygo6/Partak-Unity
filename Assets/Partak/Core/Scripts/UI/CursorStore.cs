using UnityEngine;
using System.Collections;
using GeoTetra.GTCommon.Components;
using GeoTetra.GTPooling;

namespace GeoTetra.Partak
{
    public class CursorStore : SubscribableBehaviour
    {
        [SerializeField] 
        [AssetReferenceComponentRestriction(typeof(ComponentContainer))]
        private ComponentContainerReference _componentContainer;
        
        [SerializeField]
        private PartakStateRef _partakState;
        
        [SerializeField] private LevelConfig _levelConfig;
        [SerializeField] private Transform[] _cursorTranforms;
        [SerializeField] private float _offset = .8f;

        public Vector3[] CursorPositions { get; private set; }
        private SkinnedMeshRenderer[] _skinnedMeshRenderers;
        private Bounds _levelBounds;
        private bool _updateCursors = true;

        private void Awake()
        {
            _componentContainer.Service.RegisterComponent(this);
        }

        private async void Start()
        {
            await _partakState.Cache();
            
            CursorPositions = new Vector3[_partakState.Service.PlayerCount()];
            _skinnedMeshRenderers = new SkinnedMeshRenderer[_cursorTranforms.Length];
            for (int i = 0; i < CursorPositions.Length; ++i)
            {
                _skinnedMeshRenderers[i] = _cursorTranforms[i].GetComponent<SkinnedMeshRenderer>();
                CursorPositions[i] = _cursorTranforms[i].position;
                _skinnedMeshRenderers[i].materials[1].color = _partakState.Service.PlayerStates[i].PlayerColor;
            }
            
            LevelConfigOnLevelDeserialized();
            _levelConfig.SizeChanged += LevelConfigOnLevelDeserialized;
        }

        private void LevelConfigOnLevelDeserialized()
        {
            _levelBounds = _levelConfig.LevelBounds;
            SetCursorsToStartPosition();
        }

        private void LateUpdate()
        {
            if (_updateCursors)
            {
                UpdateCursorTransforms();
            }
        }
        
        public void SetCursorsToStartPosition()
        {
            CursorPositions[0] = new Vector3(_offset,0, _levelBounds.size.z - _offset);
            CursorPositions[1] = new Vector3( _levelBounds.size.x - _offset,0, _levelBounds.size.z - _offset);         
            CursorPositions[2] = new Vector3(_offset,0, _offset);
            CursorPositions[3] = new Vector3( _levelBounds.size.x - _offset,0, _offset);
        }
        
        public void SetCursorsTo(Vector3 position)
        {
            for (int i = 0; i < CursorPositions.Length; ++i)
            {
                CursorPositions[i] = position;
            }
        }
        
        public void SetCursorsToCenter()
        {
            for (int i = 0; i < CursorPositions.Length; ++i)
            {
                CursorPositions[i] = _levelBounds.center;
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